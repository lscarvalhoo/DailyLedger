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
$composeFile = Join-Path $root "docker\docker-compose.yml"
$envFile = Join-Path $root "docker\.env"

Assert-DockerIsRunning

Pull-DockerImage "mcr.microsoft.com/mssql/server:2022-latest"
Pull-DockerImage "rabbitmq:4-management"
Pull-DockerImage "adminer:latest"
Pull-DockerImage "mcr.microsoft.com/dotnet/sdk:10.0"
Pull-DockerImage "mcr.microsoft.com/dotnet/aspnet:10.0"

if (Test-Path $envFile)
{
	docker compose --env-file $envFile -f $composeFile up -d --build --wait
	Assert-NativeCommandSucceeded "Could not build or start the LedgerFlow Docker stack."
	docker compose --env-file $envFile -f $composeFile ps
}
else
{
	docker compose -f $composeFile up -d --build --wait
	Assert-NativeCommandSucceeded "Could not build or start the LedgerFlow Docker stack."
	docker compose -f $composeFile ps
}
Assert-NativeCommandSucceeded "Could not retrieve the LedgerFlow Docker stack status."