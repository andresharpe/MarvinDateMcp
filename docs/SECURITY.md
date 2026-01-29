# Security Architecture - MarvinDateMcp

## Overview

This document describes the security controls implemented in the MarvinDateMcp Azure MCP Server to protect against unauthorized access and attacks.

## Security Status: ✅ PRODUCTION READY (with configuration)

The application now implements comprehensive security controls across all layers:
- ✅ API Key Authentication
- ✅ Rate Limiting
- ✅ Azure Key Vault for Secrets
- ✅ CORS Restrictions
- ✅ Security Headers
- ✅ Application Insights Logging
- ✅ IP Allowlisting (NSG)
- ✅ Managed Identity
- ✅ HTTPS/TLS 1.2+

---

## Security Controls

### 1. Authentication - API Key

**Implementation:** `src/MarvinDateMcp.Api/Security/ApiKeyAuthenticationHandler.cs`

All requests to the `/mcp` endpoint require an API key in the `X-API-Key` header.

**Configuration:**
```bash
# Generate a strong API key
openssl rand -base64 32

# Add to .env.local
MCP_API_KEY=your_generated_key_here
```

**Usage:**
```bash
curl -X POST https://YOUR_APP_SERVICE_URL/mcp \
  -H "X-API-Key: your_api_key_here" \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","id":1,"method":"initialize",...}'
```

**Security Features:**
- ✅ Constant-time comparison (prevents timing attacks)
- ✅ Logged authentication attempts (failed and successful)
- ✅ Returns 401 Unauthorized for missing/invalid keys
- ✅ Returns 403 Forbidden for authorization failures

---

### 2. Rate Limiting

**Implementation:** `src/MarvinDateMcp.Api/Program.cs:59-79`

Prevents abuse by limiting requests per IP address.

**Configuration:**
- **Limit:** 100 requests per minute per IP
- **Algorithm:** Fixed Window
- **Response:** 429 Too Many Requests when exceeded

**Customization:**
```csharp
// In Program.cs, adjust these values:
PermitLimit = 100,                    // Max requests
Window = TimeSpan.FromMinutes(1)      // Time window
```

**Logging:**
All rate limit violations are logged with source IP address.

---

### 3. Azure Key Vault Integration

**Implementation:** `terraform/main.tf:47-82`

Secrets are stored in Azure Key Vault, not in App Settings or environment variables.

**Resources Created:**
- Key Vault: `YOUR_KEY_VAULT_NAME`
- Secrets:
  - `google-api-key` - Google Geocoding/Timezone API key
  - `mcp-api-key` - MCP endpoint authentication key

**Access Control:**
- App Service uses Managed Identity
- Only `Get` and `List` permissions granted
- Deployer has `Set` and `Delete` for deployment

**Key Vault References:**
```terraform
# In App Settings (automatic retrieval via Managed Identity)
"Google__ApiKey" = "@Microsoft.KeyVault(VaultName=YOUR_KEY_VAULT_NAME;SecretName=google-api-key)"
```

---

### 4. CORS Restrictions

**Implementation:** `src/MarvinDateMcp.Api/Program.cs:81-93`

Replaces wildcard `AllowedHosts: *` with explicit origin allowlist.

**Configuration:**
```json
// appsettings.json
{
  "Security": {
    "AllowedOrigins": [
      "https://your-trusted-domain.com",
      "https://another-trusted-domain.com"
    ]
  }
}
```

**Default (Development):**
```json
"AllowedOrigins": ["https://localhost:5001"]
```

**Headers Exposed:**
- `Mcp-Session-Id` (required for MCP protocol)

---

### 5. Security Headers

**Implementation:** `src/MarvinDateMcp.Api/Program.cs:117-127`

All HTTP responses include comprehensive security headers.

**Headers Applied:**
| Header | Value | Purpose |
|--------|-------|---------|
| `X-Content-Type-Options` | `nosniff` | Prevent MIME sniffing |
| `X-Frame-Options` | `DENY` | Prevent clickjacking |
| `Strict-Transport-Security` | `max-age=31536000; includeSubDomains` | Force HTTPS for 1 year |
| `Content-Security-Policy` | `default-src 'self'` | Restrict resource loading |
| `X-XSS-Protection` | `1; mode=block` | Enable XSS filter |
| `Referrer-Policy` | `strict-origin-when-cross-origin` | Control referrer info |
| `Permissions-Policy` | `geolocation=(), microphone=(), camera=()` | Disable sensitive features |

---

### 6. Application Insights

**Implementation:** `terraform/main.tf:84-109`

Centralized logging and monitoring for security events.

**Resources Created:**
- Application Insights: `YOUR_APP_INSIGHTS_NAME`
- Log Analytics Workspace: `YOUR_LOG_ANALYTICS_NAME`
- Retention: 30 days

**Logged Events:**
- ✅ Authentication attempts (success/failure)
- ✅ Rate limit violations
- ✅ API errors and exceptions
- ✅ Request telemetry (response times, status codes)
- ✅ Dependency calls (Google APIs)

**Monitoring Queries:**
```kusto
// Failed authentication attempts
traces
| where message contains "Invalid API Key"
| project timestamp, client_IP, message

// Rate limit violations
traces
| where message contains "Rate limit exceeded"
| summarize count() by bin(timestamp, 5m), client_IP
```

---

### 7. Network Security - IP Allowlisting

**Implementation:** `terraform/main.tf:205-214`

IP-based access control at the App Service level.

**Configuration:**
```hcl
# terraform/terraform.tfvars
allowed_ip_addresses = [
  "203.0.113.0/24",      # Office network
  "198.51.100.42/32"     # VPN gateway
]
```

**Behavior:**
- If `allowed_ip_addresses` is empty: All IPs allowed (default)
- If `allowed_ip_addresses` is set: Only listed IPs can access the app

**Note:** For production, configure IP restrictions to limit access to known networks.

---

### 8. Managed Identity

**Implementation:** `terraform/main.tf:168-171, 229-241`

App Service uses System Assigned Managed Identity for Azure resource access.

**Benefits:**
- ✅ No credentials in code or configuration
- ✅ Automatic credential rotation by Azure
- ✅ Least privilege access (only Key Vault secrets)
- ✅ Audit trail in Azure AD

**Permissions Granted:**
- Key Vault: `Get`, `List` secrets only
- Application Insights: Automatic via connection string

**Identity Info:**
- Type: System Assigned
- Principal ID: Output in `terraform output managed_identity_principal_id`

---

## Deployment Security

### Prerequisites

1. **Generate MCP API Key**
```bash
openssl rand -base64 32
```

2. **Update .env.local**
```bash
GOOGLE_API_KEY=your_google_api_key
MCP_API_KEY=your_generated_mcp_key
```

3. **Configure IP Allowlist (Optional but Recommended)**
```hcl
# terraform/terraform.tfvars
allowed_ip_addresses = ["your.office.ip/32"]
```

### Deployment Steps

1. **Initialize Terraform**
```bash
cd terraform
terraform init
```

2. **Plan Deployment**
```bash
terraform plan -var="google_api_key=$env:GOOGLE_API_KEY" -var="mcp_api_key=$env:MCP_API_KEY"
```

3. **Apply Infrastructure**
```bash
terraform apply -var="google_api_key=$env:GOOGLE_API_KEY" -var="mcp_api_key=$env:MCP_API_KEY" -auto-approve
```

4. **Deploy Application**
```bash
.\scripts\deploy.ps1
```

### Post-Deployment Verification

**1. Check Authentication**
```bash
# Should return 401 Unauthorized
curl https://YOUR_APP_SERVICE_URL/mcp

# Should succeed (with valid API key)
curl -H "X-API-Key: your_api_key" https://YOUR_APP_SERVICE_URL/mcp
```

**2. Check Rate Limiting**
```bash
# Run 101 requests rapidly - last one should return 429
for i in 1..101; do curl -H "X-API-Key: key" https://YOUR_APP_SERVICE_URL/health; done
```

**3. Check Key Vault Integration**
```bash
# View Key Vault secrets (should show references, not actual values)
az webapp config appsettings list --name YOUR_APP_SERVICE_NAME --resource-group YOUR_RESOURCE_GROUP_NAME
```

**4. Check Application Insights**
```bash
# View logs in Azure Portal
https://portal.azure.com > Application Insights > YOUR_APP_INSIGHTS_NAME > Logs
```

---

## Security Recommendations

### Critical (Implement Before Production)

1. ✅ **COMPLETED:** API Key authentication
2. ✅ **COMPLETED:** Rate limiting
3. ✅ **COMPLETED:** Azure Key Vault for secrets
4. ✅ **COMPLETED:** CORS restrictions
5. ⚠️ **TODO:** Configure IP allowlist in `terraform.tfvars`
6. ⚠️ **TODO:** Restrict Google API key to Azure IP ranges in Google Cloud Console

### High Priority

7. ✅ **COMPLETED:** Security headers
8. ✅ **COMPLETED:** Application Insights logging
9. ✅ **COMPLETED:** Managed Identity
10. ⚠️ **TODO:** Set up Application Insights alerts for:
    - Failed authentication attempts (>10 per hour)
    - Rate limit violations (>100 per hour)
    - HTTP 5xx errors (>5 per hour)

### Medium Priority

11. ⚠️ **TODO:** Implement session timeout (MCP sessions)
12. ⚠️ **TODO:** Add audit logging for all tool invocations
13. ⚠️ **TODO:** Upgrade MCP library from preview to stable (when available)
14. ⚠️ **TODO:** Implement API key rotation strategy

### Low Priority (Future Improvements)

15. Azure Front Door for DDoS protection
16. Web Application Firewall (WAF)
17. VNET integration for private endpoints
18. API versioning strategy
19. Penetration testing

---

## Security Incident Response

### Failed Authentication Alerts

**Symptom:** Multiple 401 responses in Application Insights

**Investigation:**
```kusto
traces
| where message contains "Invalid API Key"
| project timestamp, client_IP, message
| summarize count() by client_IP
| order by count_ desc
```

**Action:**
1. Check if source IPs are legitimate users with wrong key
2. If attack pattern detected, add IP to NSG block list
3. Consider rotating MCP API key if compromised

### Rate Limit Violations

**Symptom:** 429 responses in logs

**Investigation:**
```kusto
traces
| where message contains "Rate limit exceeded"
| project timestamp, client_IP
```

**Action:**
1. Verify if legitimate user needs higher quota
2. If abuse detected, block IP via NSG
3. Consider implementing per-user quotas

### Key Vault Access Issues

**Symptom:** App cannot retrieve secrets from Key Vault

**Investigation:**
```bash
# Check Managed Identity permissions
az keyvault show --name YOUR_KEY_VAULT_NAME
az role assignment list --assignee <managed-identity-principal-id>
```

**Action:**
1. Verify Managed Identity is enabled
2. Check Key Vault access policy
3. Ensure App Service has restarted after identity creation

---

## Compliance

### Data Classification
**Internal** - No PII or sensitive data stored

### Encryption
- ✅ HTTPS/TLS 1.2+ in transit
- ✅ Azure Storage encryption at rest (Key Vault secrets)
- ✅ Application Insights data encrypted at rest

### Access Control
- ✅ API Key authentication
- ✅ IP allowlisting capability
- ✅ Azure RBAC for infrastructure
- ✅ Key Vault RBAC for secrets

### Logging & Auditing
- ✅ Application Insights (30-day retention)
- ✅ Azure Activity Logs (infrastructure changes)
- ✅ Key Vault audit logs (secret access)

---

## Security Contact

**Security Issues:** Report to andre.sharpe@example.com
**Azure Security Center:** Monitor recommendations in Azure Portal

---

## Changelog

### 2026-01-29 - Security Hardening Release
- ✅ Added API Key authentication
- ✅ Implemented rate limiting (100 req/min per IP)
- ✅ Migrated secrets to Azure Key Vault
- ✅ Added CORS restrictions
- ✅ Implemented security headers (HSTS, CSP, etc.)
- ✅ Enabled Application Insights logging
- ✅ Added IP allowlisting capability via NSG
- ✅ Enabled System Assigned Managed Identity
- ✅ Configured Key Vault references for secrets

**Previous Status:** 🔴 HIGH RISK - No authentication, no authorization, no rate limiting
**Current Status:** ✅ PRODUCTION READY (with proper configuration)
