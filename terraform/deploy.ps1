#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Deploy MarvinDateMcp to Azure App Service
.DESCRIPTION
    Builds, publishes, and deploys the .NET application to Azure
.PARAMETER SkipBuild
    Skip the build step (use existing publish folder)
.PARAMETER SkipTerraform
    Skip Terraform infrastructure deployment
.EXAMPLE
    .\deploy.ps1
    .\deploy.ps1 -SkipBuild
    .\deploy.ps1 -SkipTerraform
#>
param(
    [switch]$SkipBuild,
    [switch]$SkipTerraform
)

$ErrorActionPreference = "Stop"

# Paths
$ScriptDir = $PSScriptRoot
$RepoRoot = Split-Path $ScriptDir -Parent
$SrcPath = Join-Path $RepoRoot "src\MarvinDateMcp.Api"
$PublishPath = Join-Path $RepoRoot "publish"
$ZipPath = Join-Path $RepoRoot "publish.zip"

Write-Host "=== MarvinDateMcp Deployment ===" -ForegroundColor Cyan

# Check Azure CLI login
Write-Host "`nChecking Azure CLI login..." -ForegroundColor Yellow
$account = az account show 2>$null | ConvertFrom-Json
if (-not $account) {
    Write-Host "Not logged in to Azure CLI. Please run 'az login' first." -ForegroundColor Red
    exit 1
}
Write-Host "Logged in as: $($account.user.name) in subscription: $($account.name)" -ForegroundColor Green

# Infrastructure deployment
if (-not $SkipTerraform) {
    Write-Host "`n=== Terraform Infrastructure ===" -ForegroundColor Cyan
    
    Push-Location $ScriptDir
    try {
        # Check for tfvars
        if (-not (Test-Path "terraform.tfvars")) {
            Write-Host "terraform.tfvars not found. Creating from .env.local..." -ForegroundColor Yellow
            
            $envFile = Join-Path $RepoRoot ".env.local"
            if (Test-Path $envFile) {
                $envContent = Get-Content $envFile -Raw
                if ($envContent -match 'GOOGLE_API_KEY=(.+)') {
                    $googleKey = $Matches[1].Trim()
                    "google_api_key = `"$googleKey`"" | Out-File "terraform.tfvars" -Encoding UTF8
                    Write-Host "Created terraform.tfvars from .env.local" -ForegroundColor Green
                }
            } else {
                Write-Host "Please create terraform.tfvars with your google_api_key" -ForegroundColor Red
                exit 1
            }
        }
        
        Write-Host "Initializing Terraform..." -ForegroundColor Yellow
        terraform init -upgrade
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
        
        Write-Host "Planning infrastructure..." -ForegroundColor Yellow
        terraform plan -out=tfplan
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
        
        Write-Host "Applying infrastructure..." -ForegroundColor Yellow
        terraform apply tfplan
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
        
        # Get outputs
        $ResourceGroup = terraform output -raw resource_group_name
        $AppName = terraform output -raw app_service_name
        $AppUrl = terraform output -raw app_url
    }
    finally {
        Pop-Location
    }
} else {
    Write-Host "`nSkipping Terraform (using existing infrastructure)" -ForegroundColor Yellow
    Push-Location $ScriptDir
    $ResourceGroup = terraform output -raw resource_group_name 2>$null
    $AppName = terraform output -raw app_service_name 2>$null
    $AppUrl = terraform output -raw app_url 2>$null
    Pop-Location
    
    if (-not $AppName) {
        Write-Host "Could not get Terraform outputs. Run without -SkipTerraform first." -ForegroundColor Red
        exit 1
    }
}

Write-Host "`nTarget: $AppName in $ResourceGroup" -ForegroundColor Cyan

# Build and publish
if (-not $SkipBuild) {
    Write-Host "`n=== Building Application ===" -ForegroundColor Cyan
    
    # Clean previous publish
    if (Test-Path $PublishPath) { Remove-Item $PublishPath -Recurse -Force }
    if (Test-Path $ZipPath) { Remove-Item $ZipPath -Force }
    
    Write-Host "Publishing .NET application..." -ForegroundColor Yellow
    dotnet publish $SrcPath -c Release -o $PublishPath --no-self-contained
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    
    Write-Host "Creating deployment package..." -ForegroundColor Yellow
    Compress-Archive -Path "$PublishPath\*" -DestinationPath $ZipPath -Force
}

# Deploy
Write-Host "`n=== Deploying to Azure ===" -ForegroundColor Cyan

if (-not (Test-Path $ZipPath)) {
    Write-Host "publish.zip not found. Run without -SkipBuild first." -ForegroundColor Red
    exit 1
}

Write-Host "Deploying to App Service..." -ForegroundColor Yellow
az webapp deploy --resource-group $ResourceGroup --name $AppName --src-path $ZipPath --type zip
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "`n=== Deployment Complete ===" -ForegroundColor Green
Write-Host "Application URL: $AppUrl" -ForegroundColor Cyan
Write-Host "`nTest the MCP endpoint:" -ForegroundColor Yellow
Write-Host "  curl $AppUrl/health" -ForegroundColor White
