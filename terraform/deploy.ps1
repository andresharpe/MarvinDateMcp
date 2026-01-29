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
                $googleKey = $null
                $mcpKey = $null

                if ($envContent -match 'GOOGLE_API_KEY=(.+)') {
                    $googleKey = $Matches[1].Trim()
                }
                if ($envContent -match 'MCP_API_KEY=(.+)') {
                    $mcpKey = $Matches[1].Trim()
                }

                if ($googleKey -and $mcpKey) {
                    @"
google_api_key = "$googleKey"
mcp_api_key = "$mcpKey"
"@ | Out-File "terraform.tfvars" -Encoding UTF8
                    Write-Host "Created terraform.tfvars from .env.local" -ForegroundColor Green
                } else {
                    Write-Host "Missing required keys in .env.local:" -ForegroundColor Red
                    if (-not $googleKey) { Write-Host "  - GOOGLE_API_KEY" -ForegroundColor Red }
                    if (-not $mcpKey) { Write-Host "  - MCP_API_KEY" -ForegroundColor Red }
                    exit 1
                }
            } else {
                Write-Host "Please create .env.local with GOOGLE_API_KEY and MCP_API_KEY" -ForegroundColor Red
                exit 1
            }
        }
        
        Write-Host "Initializing Terraform..." -ForegroundColor Yellow
        terraform init -upgrade
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
        
        # Check if web app managed identity exists (determines if two-step deploy needed)
        $webAppIdentityExists = $false
        try {
            $existingIdentity = terraform state show azurerm_windows_web_app.api 2>$null | Select-String "principal_id"
            if ($existingIdentity) {
                $webAppIdentityExists = $true
            }
        } catch {
            # State doesn't exist yet, that's fine
        }
        
        if (-not $webAppIdentityExists) {
            # Two-step deployment: First create all resources except app_identity policy
            # This is needed because the Key Vault access policy references the web app's
            # managed identity, which doesn't exist until the web app is created
            Write-Host "First-time deployment: Using two-step Terraform apply..." -ForegroundColor Yellow
            
            Write-Host "Step 1: Creating infrastructure (excluding Key Vault app identity policy)..." -ForegroundColor Yellow
            $terraformArgs = @(
                'apply',
                '-target=azurerm_resource_group.main',
                '-target=azurerm_service_plan.main',
                '-target=azurerm_windows_web_app.api',
                '-target=azurerm_key_vault.main',
                '-target=azurerm_key_vault_access_policy.deployer',
                '-target=azurerm_key_vault_secret.google_api_key',
                '-target=azurerm_key_vault_secret.mcp_api_key',
                '-target=azurerm_application_insights.main',
                '-target=azurerm_log_analytics_workspace.main',
                '-auto-approve'
            )
            & terraform $terraformArgs
            if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
            
            Write-Host "Step 2: Creating Key Vault access policy for app identity..." -ForegroundColor Yellow
            terraform apply -target="azurerm_key_vault_access_policy.app_identity" -auto-approve
            if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
        } else {
            # Normal deployment: All resources can be applied together
            Write-Host "Subsequent deployment: Using standard Terraform apply..." -ForegroundColor Yellow
            
            Write-Host "Planning infrastructure..." -ForegroundColor Yellow
            terraform plan -out=tfplan
            if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
            
            Write-Host "Applying infrastructure..." -ForegroundColor Yellow
            terraform apply tfplan
            if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
        }
        
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

# Verification Testing
Write-Host "`n=== Verifying Deployment ===" -ForegroundColor Cyan

# Wait for app startup
Write-Host "Waiting for application startup (30s)..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Test 1: Health endpoint
Write-Host "`nTesting health endpoint..." -ForegroundColor Yellow
try {
    $healthResponse = Invoke-WebRequest -Uri "$AppUrl/health" -UseBasicParsing -TimeoutSec 10
    if ($healthResponse.StatusCode -eq 200 -and $healthResponse.Content -eq "Healthy") {
        Write-Host "✓ Health endpoint OK" -ForegroundColor Green
    } else {
        Write-Host "✗ Health endpoint returned unexpected response" -ForegroundColor Red
        Write-Host "  Status: $($healthResponse.StatusCode)" -ForegroundColor Red
        Write-Host "  Body: $($healthResponse.Content)" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Health endpoint failed: $_" -ForegroundColor Red
}

# Test 2: MCP endpoint initialization
Write-Host "`nTesting MCP endpoint..." -ForegroundColor Yellow

# Get MCP API Key from .env.local
$mcpApiKey = $null
$envFile = Join-Path $RepoRoot ".env.local"
if (Test-Path $envFile) {
    $envContent = Get-Content $envFile -Raw
    if ($envContent -match 'MCP_API_KEY=(.+)') {
        $mcpApiKey = $Matches[1].Trim()
    }
}

if (-not $mcpApiKey) {
    Write-Host "⚠ MCP_API_KEY not found in .env.local - skipping MCP tests" -ForegroundColor Yellow
    Write-Host "  Set MCP_API_KEY in .env.local to enable MCP endpoint tests" -ForegroundColor Yellow
} else {
    # MCP requires Accept header with both application/json and text/event-stream
    $mcpAcceptHeader = "application/json, text/event-stream"
    
    try {
        # Test 1: Verify authentication is required (no API key)
        Write-Host "Testing authentication (without API key)..." -ForegroundColor Yellow
        try {
            $unauthResponse = Invoke-WebRequest `
                -Uri "$AppUrl/mcp" `
                -Method POST `
                -ContentType "application/json" `
                -Headers @{ "Accept" = $mcpAcceptHeader } `
                -Body '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test","version":"1.0.0"}}}' `
                -UseBasicParsing `
                -TimeoutSec 10 `
                -SkipHttpErrorCheck

            if ($unauthResponse.StatusCode -eq 401) {
                Write-Host "✓ Authentication required (401 Unauthorized)" -ForegroundColor Green
            } else {
                Write-Host "✗ Expected 401 Unauthorized, got $($unauthResponse.StatusCode)" -ForegroundColor Red
                Write-Host "  Response: $($unauthResponse.Content)" -ForegroundColor Red
            }
        } catch {
            if ($_.Exception.Response.StatusCode.Value__ -eq 401) {
                Write-Host "✓ Authentication required (401 Unauthorized)" -ForegroundColor Green
            } else {
                Write-Host "✗ Authentication test failed: $_" -ForegroundColor Red
            }
        }

        # Test 2: Initialize MCP session with API key
        Write-Host "Testing MCP initialize (with API key)..." -ForegroundColor Yellow
        $initPayload = @{
            jsonrpc = "2.0"
            id = 1
            method = "initialize"
            params = @{
                protocolVersion = "2024-11-05"
                capabilities = @{}
                clientInfo = @{
                    name = "deploy-test"
                    version = "1.0.0"
                }
            }
        } | ConvertTo-Json -Depth 10

        $initResponse = Invoke-WebRequest `
            -Uri "$AppUrl/mcp" `
            -Method POST `
            -ContentType "application/json" `
            -Headers @{ 
                "X-API-Key" = $mcpApiKey
                "Accept" = $mcpAcceptHeader
            } `
            -Body $initPayload `
            -UseBasicParsing `
            -TimeoutSec 10

        if ($initResponse.StatusCode -ne 200) {
            Write-Host "✗ MCP initialize failed with status $($initResponse.StatusCode)" -ForegroundColor Red
            Write-Host "  Response: $($initResponse.Content)" -ForegroundColor Red
            exit 1
        }

        # Extract session ID from response headers (may be returned as array)
        $sessionIdRaw = $initResponse.Headers['mcp-session-id']
        if (-not $sessionIdRaw) {
            Write-Host "✗ MCP initialize did not return mcp-session-id header" -ForegroundColor Red
            Write-Host "  Headers: $($initResponse.Headers | ConvertTo-Json)" -ForegroundColor Red
            exit 1
        }
        # Handle array or string
        if ($sessionIdRaw -is [array]) {
            $sessionId = $sessionIdRaw[0]
        } else {
            $sessionId = $sessionIdRaw
        }

        Write-Host "✓ MCP session initialized (ID: $sessionId)" -ForegroundColor Green

        # Test 3: Test tools/list with session ID
        Write-Host "Testing MCP tools/list..." -ForegroundColor Yellow
        $toolsPayload = @{
            jsonrpc = "2.0"
            id = 2
            method = "tools/list"
            params = @{}
        } | ConvertTo-Json -Depth 10

        $toolsResponse = Invoke-WebRequest `
            -Uri "$AppUrl/mcp" `
            -Method POST `
            -ContentType "application/json" `
            -Headers @{
                "X-API-Key" = $mcpApiKey
                "Accept" = $mcpAcceptHeader
                "Mcp-Session-Id" = $sessionId
            } `
            -Body $toolsPayload `
            -UseBasicParsing `
            -TimeoutSec 10

        if ($toolsResponse.StatusCode -ne 200) {
            Write-Host "✗ MCP tools/list failed with status $($toolsResponse.StatusCode)" -ForegroundColor Red
            Write-Host "  Response: $($toolsResponse.Content)" -ForegroundColor Red
            exit 1
        }

        # Parse and validate tools response (handle SSE format)
        $toolsContent = $toolsResponse.Content
        # Extract JSON from SSE format if present (event: message\ndata: {...})
        if ($toolsContent -match 'data:\s*(.+)') {
            $toolsJson = $Matches[1]
        } else {
            $toolsJson = $toolsContent
        }
        
        $toolsResult = $toolsJson | ConvertFrom-Json
        if ($toolsResult.result.tools -and $toolsResult.result.tools.Count -gt 0) {
            $toolNames = $toolsResult.result.tools | ForEach-Object { $_.name }
            Write-Host "✓ MCP tools/list OK - Found tools: $($toolNames -join ', ')" -ForegroundColor Green

            # Verify expected tool exists
            if ($toolNames -contains "analyze_date_context") {
                Write-Host "✓ analyze_date_context tool registered" -ForegroundColor Green
            } else {
                Write-Host "⚠ analyze_date_context tool not found in: $toolNames" -ForegroundColor Yellow
            }
        } else {
            Write-Host "✗ MCP tools/list returned no tools" -ForegroundColor Red
            Write-Host "  Response: $toolsContent" -ForegroundColor Red
        }

    } catch {
        Write-Host "✗ MCP endpoint test failed: $_" -ForegroundColor Red
        Write-Host "  Exception: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n=== Deployment Verification Complete ===" -ForegroundColor Green
Write-Host "Application URL: $AppUrl" -ForegroundColor Cyan
