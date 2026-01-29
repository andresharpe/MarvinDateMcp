# Deployment Guide - MarvinDateMcp (Security Hardened)

## Prerequisites

### Required Software
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Terraform](https://www.terraform.io/downloads) >= 1.6
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- PowerShell 7+ (for deployment script)

### Azure Requirements
- Azure subscription with appropriate permissions
- Ability to create:
  - Resource Groups
  - App Service Plans and App Services
  - Key Vaults
  - Application Insights
  - Log Analytics Workspaces

---

## Security Configuration

### 1. Generate API Keys

**MCP API Key** (Required for authentication):
```bash
# Generate a secure random API key
openssl rand -base64 32

# Or on Windows PowerShell:
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
```

**Google API Key** (Required for geocoding/timezone):
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create or select a project
3. Enable these APIs:
   - Geocoding API
   - Time Zone API
4. Create API Key (API & Services > Credentials)
5. **IMPORTANT:** Restrict the key:
   - Application restrictions: IP addresses (add Azure outbound IPs)
   - API restrictions: Only Geocoding API and Time Zone API

### 2. Configure Environment Variables

Create `.env.local` in the repository root:

```bash
# Copy from example
cp .env.example .env.local

# Edit and add your keys
GOOGLE_API_KEY=your_google_api_key_here
MCP_API_KEY=your_generated_mcp_api_key_here
```

**IMPORTANT:** Never commit `.env.local` to git (already in .gitignore)

### 3. Configure Network Security (Optional but Recommended)

Edit `terraform/terraform.tfvars` to add IP allowlisting:

```hcl
allowed_ip_addresses = [
  "203.0.113.0/24",      # Your office network CIDR
  "198.51.100.42/32",    # Your VPN gateway IP
]
```

To find your IP:
```bash
curl ifconfig.me
```

**Note:** If you don't configure this, the app will be accessible from any IP (not recommended for production).

---

## Deployment Steps

### Step 1: Azure Login

```bash
az login
az account list --output table  # Find your subscription
az account set --subscription "<your-subscription-id>"
az account show
```

### Step 2: Deploy Infrastructure and Application

```bash
# From repository root
cd terraform
.\deploy.ps1
```

This script will:
1. ✅ Create Terraform infrastructure (Key Vault, App Service, Application Insights)
2. ✅ Build and publish the .NET application
3. ✅ Deploy to Azure App Service
4. ✅ Run verification tests

**Note on First-Time Deployment:**

The script automatically handles a two-step Terraform deployment for fresh environments. This is required because:
- The Key Vault access policy for the App Service needs the managed identity's principal ID
- The managed identity doesn't exist until the App Service is created
- Terraform cannot reference values that don't exist yet

The script detects if this is a first-time deployment and:
1. **Step 1:** Creates all resources *except* the `app_identity` Key Vault access policy
2. **Step 2:** Creates the Key Vault access policy using the now-available managed identity

Subsequent deployments use a standard single-step `terraform apply`.

**Expected Output (First-Time Deployment):**
```
=== MarvinDateMcp Deployment ===
Logged in as: user@example.com in subscription: <your-subscription-name>

=== Terraform Infrastructure ===
Initializing Terraform...
First-time deployment: Using two-step Terraform apply...
Step 1: Creating infrastructure (excluding Key Vault app identity policy)...
Apply complete! Resources: 9 added, 0 changed, 0 destroyed.
Step 2: Creating Key Vault access policy for app identity...
Apply complete! Resources: 1 added, 0 changed, 0 destroyed.

=== Building Application ===
Publishing .NET application...
Creating deployment package...

=== Deploying to Azure ===
Deploying to App Service...
Deployment successful.

=== Verifying Deployment ===
✓ Health endpoint OK
✓ Authentication required (401 Unauthorized)
✓ MCP session initialized (ID: abc123)
✓ MCP tools/list OK - Found tools: analyze_date_context
✓ analyze_date_context tool registered

Application URL: https://YOUR_APP_SERVICE_URL
```

**Expected Output (Subsequent Deployments):**
```
=== Terraform Infrastructure ===
Initializing Terraform...
Subsequent deployment: Using standard Terraform apply...
Planning infrastructure...
Applying infrastructure...
Apply complete! Resources: 0 added, 1 changed, 0 destroyed.
```

### Step 3: Verify Deployment

**Test Health Endpoint:**
```bash
curl https://YOUR_APP_SERVICE_URL/health
# Expected: "Healthy"
```

**Test Authentication (Should Fail):**
```bash
curl https://YOUR_APP_SERVICE_URL/mcp
# Expected: 401 Unauthorized
```

**Test Authenticated MCP Request:**
```bash
curl -X POST https://YOUR_APP_SERVICE_URL/mcp \
  -H "X-API-Key: your_mcp_api_key_here" \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": 1,
    "method": "initialize",
    "params": {
      "protocolVersion": "2024-11-05",
      "capabilities": {},
      "clientInfo": {
        "name": "test-client",
        "version": "1.0.0"
      }
    }
  }'
# Expected: JSON response with mcp-session-id header
```

---

## Deployment Options

### Skip Build (Reuse Existing Package)
```bash
.\deploy.ps1 -SkipBuild
```

### Skip Terraform (Deploy Code Only)
```bash
.\deploy.ps1 -SkipTerraform
```

### Manual Terraform Steps

**For subsequent deployments** (infrastructure already exists):
```bash
cd terraform
terraform init
terraform plan
terraform apply
```

**For first-time deployment** (two-step process required):
```powershell
cd terraform
terraform init

# Step 1: Create all resources except Key Vault app identity policy
terraform apply `
  -target="azurerm_resource_group.main" `
  -target="azurerm_service_plan.main" `
  -target="azurerm_windows_web_app.api" `
  -target="azurerm_key_vault.main" `
  -target="azurerm_key_vault_access_policy.deployer" `
  -target="azurerm_key_vault_secret.google_api_key" `
  -target="azurerm_key_vault_secret.mcp_api_key" `
  -target="azurerm_application_insights.main" `
  -target="azurerm_log_analytics_workspace.main"

# Step 2: Create Key Vault access policy for app managed identity
terraform apply -target="azurerm_key_vault_access_policy.app_identity"
```

The deploy script handles this automatically - manual steps are only needed for troubleshooting.

---

## Post-Deployment Configuration

### 1. Configure Application Insights Alerts

Navigate to Azure Portal > Application Insights > `YOUR_APP_INSIGHTS_NAME` > Alerts

**Recommended Alerts:**

**Failed Authentication Alert:**
```kusto
traces
| where message contains "Invalid API Key"
| summarize count() by bin(timestamp, 1h)
| where count_ > 10
```
Alert when > 10 failed attempts per hour

**Rate Limit Violations:**
```kusto
traces
| where message contains "Rate limit exceeded"
| summarize count() by bin(timestamp, 1h)
| where count_ > 100
```
Alert when > 100 rate limits per hour

**Application Errors:**
```kusto
exceptions
| summarize count() by bin(timestamp, 1h)
| where count_ > 5
```
Alert when > 5 exceptions per hour

### 2. Restrict Google API Key

1. Get App Service outbound IPs:
```bash
az webapp show \
  --resource-group YOUR_RESOURCE_GROUP_NAME \
  --name YOUR_APP_SERVICE_NAME \
  --query outboundIpAddresses \
  --output tsv
```

2. In Google Cloud Console:
   - API & Services > Credentials > [Your API Key]
   - Application restrictions: IP addresses
   - Add all outbound IPs from step 1

### 3. Review Key Vault Access

```bash
# View Key Vault details
az keyvault show --name YOUR_KEY_VAULT_NAME

# List access policies
az keyvault show --name YOUR_KEY_VAULT_NAME \
  --query properties.accessPolicies
```

---

## Updating Secrets

### Update MCP API Key

**Option 1: Via Azure Portal**
1. Navigate to Key Vault > `YOUR_KEY_VAULT_NAME`
2. Secrets > `mcp-api-key`
3. New Version > Add new value
4. Restart App Service

**Option 2: Via Azure CLI**
```bash
# Generate new key
$newKey = [Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))

# Update Key Vault
az keyvault secret set \
  --vault-name YOUR_KEY_VAULT_NAME \
  --name mcp-api-key \
  --value "$newKey"

# Update App Setting (for direct reference)
az webapp config appsettings set \
  --resource-group YOUR_RESOURCE_GROUP_NAME \
  --name YOUR_APP_SERVICE_NAME \
  --settings "MCP_API_KEY=$newKey"

# Restart app
az webapp restart \
  --resource-group YOUR_RESOURCE_GROUP_NAME \
  --name YOUR_APP_SERVICE_NAME
```

### Update Google API Key

```bash
# Update Key Vault
az keyvault secret set \
  --vault-name YOUR_KEY_VAULT_NAME \
  --name google-api-key \
  --value "your_new_google_api_key"

# Restart app (to pick up new secret)
az webapp restart \
  --resource-group YOUR_RESOURCE_GROUP_NAME \
  --name YOUR_APP_SERVICE_NAME
```

---

## Monitoring

### View Application Logs

**Azure Portal:**
1. App Service > `YOUR_APP_SERVICE_NAME`
2. Log stream (real-time)
3. Or: Application Insights > Logs

**Azure CLI:**
```bash
# Stream logs
az webapp log tail \
  --resource-group YOUR_RESOURCE_GROUP_NAME \
  --name YOUR_APP_SERVICE_NAME

# Download logs
az webapp log download \
  --resource-group YOUR_RESOURCE_GROUP_NAME \
  --name YOUR_APP_SERVICE_NAME
```

### Application Insights Queries

**Authentication Events:**
```kusto
traces
| where message contains "authentication" or message contains "API Key"
| project timestamp, message, customDimensions.RemoteIp
| order by timestamp desc
```

**Rate Limiting:**
```kusto
traces
| where message contains "Rate limit"
| summarize count() by bin(timestamp, 5m), tostring(customDimensions.RemoteIp)
| render timechart
```

**Performance:**
```kusto
requests
| summarize avg(duration), percentile(duration, 95) by bin(timestamp, 5m), name
| render timechart
```

---

## Troubleshooting

### Issue: 401 Unauthorized (Expected)

**Cause:** MCP endpoint requires authentication
**Solution:** Add `X-API-Key` header with your MCP API key

### Issue: 429 Too Many Requests

**Cause:** Rate limit exceeded (100 requests/minute)
**Solution:** Wait 1 minute or implement backoff in client

### Issue: Key Vault Access Denied

**Symptoms:**
- App logs show "Failed to configure Azure Key Vault"
- Secrets not loading

**Resolution:**
```bash
# Check Managed Identity
az webapp identity show \
  --resource-group YOUR_RESOURCE_GROUP_NAME \
  --name YOUR_APP_SERVICE_NAME

# Verify Key Vault access policy
az keyvault show --name YOUR_KEY_VAULT_NAME \
  --query properties.accessPolicies

# If missing, re-run Terraform
cd terraform
terraform apply
```

### Issue: Application Not Starting

**Check logs:**
```bash
az webapp log tail \
  --resource-group YOUR_RESOURCE_GROUP_NAME \
  --name YOUR_APP_SERVICE_NAME
```

**Common causes:**
1. Missing environment variables
2. Key Vault access issues
3. .NET runtime mismatch

**Solution:**
```bash
# Verify App Settings
az webapp config appsettings list \
  --resource-group YOUR_RESOURCE_GROUP_NAME \
  --name YOUR_APP_SERVICE_NAME

# Restart app
az webapp restart \
  --resource-group YOUR_RESOURCE_GROUP_NAME \
  --name YOUR_APP_SERVICE_NAME
```

---

## Rollback

### Rollback Application Code

```bash
# List deployment history
az webapp deployment list \
  --resource-group YOUR_RESOURCE_GROUP_NAME \
  --name YOUR_APP_SERVICE_NAME

# Restore previous deployment
az webapp deployment source show \
  --resource-group YOUR_RESOURCE_GROUP_NAME \
  --name YOUR_APP_SERVICE_NAME
```

### Rollback Infrastructure

```bash
cd terraform

# Review Terraform state
terraform show

# Destroy and recreate (WARNING: deletes all resources)
terraform destroy
terraform apply
```

---

## Cleanup

### Delete Test Environment

```bash
cd terraform
terraform destroy -auto-approve
```

**Note:** This will delete:
- App Service and App Service Plan
- Key Vault (soft-deleted, can be recovered for 7 days)
- Application Insights
- Log Analytics Workspace
- Resource Group (if empty)

### Purge Soft-Deleted Key Vault (Optional)

```bash
az keyvault purge \
  --name YOUR_KEY_VAULT_NAME \
  --location westeurope
```

---

## Production Deployment Checklist

Before deploying to production:

- [ ] Generate strong MCP API key (`openssl rand -base64 32`)
- [ ] Configure `allowed_ip_addresses` in `terraform.tfvars`
- [ ] Restrict Google API key to Azure IPs in Google Cloud Console
- [ ] Change `environment = "prod"` in `terraform.tfvars`
- [ ] Update App Service Plan to production tier (S1 or P1V2)
- [ ] Configure Application Insights alerts
- [ ] Set up Key Vault monitoring alerts
- [ ] Enable Key Vault purge protection (`purge_protection_enabled = true`)
- [ ] Increase Key Vault retention (`soft_delete_retention_days = 90`)
- [ ] Document API key rotation schedule
- [ ] Set up backup/disaster recovery plan
- [ ] Review CORS allowed origins
- [ ] Test failover procedures

---

## Security Compliance

### Security Controls Implemented

✅ **Authentication:** API Key required for all MCP endpoints
✅ **Rate Limiting:** 100 requests/minute per IP
✅ **Secrets Management:** Azure Key Vault with Managed Identity
✅ **Network Security:** IP allowlisting via NSG (configurable)
✅ **Transport Security:** HTTPS/TLS 1.2+ enforced
✅ **Security Headers:** HSTS, CSP, X-Frame-Options, etc.
✅ **Monitoring:** Application Insights with 30-day retention
✅ **CORS:** Configured allowed origins (no wildcard)

### Regular Security Tasks

**Weekly:**
- Review Application Insights for suspicious activity
- Check failed authentication attempts

**Monthly:**
- Review Key Vault access logs
- Update dependencies (`dotnet outdated`)
- Review Azure Security Center recommendations

**Quarterly:**
- Rotate MCP API Key
- Review and update IP allowlist
- Security scan with OWASP ZAP or similar

**Annually:**
- Penetration testing
- Security architecture review
- Disaster recovery drill

---

## Support

**Issues:** https://github.com/your-org/MarvinDateMcp/issues
**Security:** andre.sharpe@example.com
**Azure Support:** https://portal.azure.com > Help + support
