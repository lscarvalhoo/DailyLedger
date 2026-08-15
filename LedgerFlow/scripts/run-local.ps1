$ErrorActionPreference = "Stop"

function Assert-NativeCommandSucceeded
{
	param([string]$Message)

	if ($LASTEXITCODE -ne 0)
	{
		throw $Message
	}
}

function Assert-DockerIsRunning
{
	$previousPreference = $ErrorActionPreference
	$ErrorActionPreference = "SilentlyContinue"
	docker info *> $null
	$exitCode = $LASTEXITCODE
	$ErrorActionPreference = $previousPreference

	if ($exitCode -ne 0)
	{
		throw "Docker Desktop is not running. Start Docker Desktop and run this script again."
	}
}

function Pull-DockerImage
{
	param(
		[string]$Image,
		[int]$MaximumAttempts = 3
	)

	$previousPreference = $ErrorActionPreference
	$ErrorActionPreference = "SilentlyContinue"
	docker image inspect $Image *> $null
	$imageExists = $LASTEXITCODE -eq 0
	$ErrorActionPreference = $previousPreference

	if ($imageExists)
	{
		Write-Host "Docker image already available: $Image"
		return
	}

	for ($attempt = 1; $attempt -le $MaximumAttempts; $attempt++)
	{
		Write-Host "Pulling $Image (attempt $attempt/$MaximumAttempts)..."
		$previousPreference = $ErrorActionPreference
		$ErrorActionPreference = "Continue"
		docker pull $Image
		$exitCode = $LASTEXITCODE
		$ErrorActionPreference = $previousPreference

		if ($exitCode -eq 0)
		{
			return
		}
	}

	throw "Could not download '$Image'. Check VPN/proxy/firewall access to mcr.microsoft.com, *.data.mcr.microsoft.com and Docker Hub, then run the script again."
}

$root = Split-Path -Parent $PSScriptRoot
$composeFile = Join-Path $root "docker\docker-compose.dependencies.yml"
$envFile = Join-Path $root "docker\.env"

if (Test-Path $envFile)
{
	Get-Content $envFile | Where-Object { $_ -and -not $_.StartsWith("#") } | ForEach-Object {
		$name, $value = $_ -split "=", 2
		[Environment]::SetEnvironmentVariable($name.Trim(), $value.Trim(), "Process")
	}
}

Assert-DockerIsRunning

Pull-DockerImage "mcr.microsoft.com/mssql/server:2022-latest"
Pull-DockerImage "rabbitmq:4-management"
Pull-DockerImage "adminer:latest"

if (Test-Path $envFile)
{
	docker compose --env-file $envFile -f $composeFile up -d --wait
}
else
{
	docker compose -f $composeFile up -d --wait
}
Assert-NativeCommandSucceeded "Could not start SQL Server and RabbitMQ containers. Check 'docker compose logs'."

Write-Host ""
Write-Host "Local dependencies are healthy:"
Write-Host "  SQL Server:          localhost:1433"
Write-Host "  Adminer:             http://localhost:8081"
Write-Host "  RabbitMQ:            localhost:5672"
Write-Host "  RabbitMQ Management: http://localhost:15672"
Write-Host ""
Write-Host "Open LedgerFlow.sln in Visual Studio and press F5 when you want to start LedgerFlow.API."