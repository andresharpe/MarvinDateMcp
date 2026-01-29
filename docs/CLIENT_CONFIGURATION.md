# Client Configuration Guide

This guide shows how to configure various MCP clients to connect to the deployed MarvinDateMcp server.

## Prerequisites

- Deployed MarvinDateMcp server (see [DEPLOYMENT.md](DEPLOYMENT.md))
- MCP API Key (stored in Azure Key Vault)
- Azure App Service URL (obtained after deployment)

---

## Getting Your Configuration Values

### Server URL

After deployment, get your server URL:

```bash
# From terraform directory
cd terraform
terraform output app_service_url
```

Or from Azure Portal: App Service → Overview → URL

### MCP API Key

Retrieve the MCP API Key from Azure Key Vault:

```bash
# Get the key from Key Vault
az keyvault secret show \
  --vault-name YOUR_KEY_VAULT_NAME \
  --name mcp-api-key \
  --query value \
  --output tsv
```

Or from Azure Portal: Key Vault → Secrets → mcp-api-key → Current Version → Show Secret Value

**Security Note:** Keep this key secure and never commit it to version control or share it publicly.

---

## Configuration by Client

### Claude Desktop (Claude Code)

**Location:** `%APPDATA%\Claude\claude_desktop_config.json` (Windows) or `~/Library/Application Support/Claude/claude_desktop_config.json` (macOS)

```json
{
  "mcpServers": {
    "MarvinDateMcp": {
      "command": "node",
      "args": [
        "/path/to/mcp-http-proxy.js",
        "https://YOUR_APP_SERVICE_URL/mcp"
      ],
      "env": {
        "MCP_API_KEY": "YOUR_MCP_API_KEY"
      }
    }
  }
}
```

**Note:** Claude Desktop requires a local proxy for HTTP-based MCP servers. See the [MCP HTTP Proxy setup](https://github.com/modelcontextprotocol/docs/blob/main/http-transport.md) for details.

Alternatively, if using a direct HTTP MCP client library:

```json
{
  "mcpServers": {
    "MarvinDateMcp": {
      "url": "https://YOUR_APP_SERVICE_URL/mcp",
      "headers": {
        "X-API-Key": "YOUR_MCP_API_KEY"
      },
      "description": "Date context analysis for location-aware AI applications"
    }
  }
}
```

---

### ChatGPT Desktop

**Location:** `%APPDATA%\OpenAI\ChatGPT\mcp_config.json` (Windows) or `~/Library/Application Support/OpenAI/ChatGPT/mcp_config.json` (macOS)

```json
{
  "mcpServers": {
    "MarvinDateMcp": {
      "url": "https://YOUR_APP_SERVICE_URL/mcp",
      "headers": {
        "X-API-Key": "YOUR_MCP_API_KEY"
      },
      "description": "Date context analysis for location-aware AI applications"
    }
  }
}
```

**Steps:**
1. Open ChatGPT Desktop
2. Go to Settings → Integrations → MCP Servers
3. Click "Add Server" or "Edit Configuration"
4. Add the configuration above
5. Replace `YOUR_APP_SERVICE_URL` with your Azure App Service URL
6. Replace `YOUR_MCP_API_KEY` with the key from Azure Key Vault
7. Save and restart ChatGPT Desktop

---

### Warp AI Agent Mode

**Location:** Warp Settings → MCP Servers → Add Server

Add the following configuration:

```json
{
  "mcpServers": {
    "MarvinDateMcp": {
      "url": "https://YOUR_APP_SERVICE_URL/mcp",
      "headers": {
        "X-API-Key": "YOUR_MCP_API_KEY"
      },
      "description": "Date context analysis for location-aware AI applications"
    }
  }
}
```

**Steps:**
1. Open Warp Settings
2. Navigate to MCP Servers section
3. Click "Add Server"
4. Paste the configuration above
5. Replace `YOUR_APP_SERVICE_URL` with your Azure App Service URL
6. Replace `YOUR_MCP_API_KEY` with the key from Azure Key Vault
7. Save and restart Warp

---

### ElevenLabs Agent

**Location:** ElevenLabs Platform → Agent Configuration → MCP Integration

Add the MCP server configuration:

```json
{
  "name": "MarvinDateMcp",
  "url": "https://YOUR_APP_SERVICE_URL/mcp",
  "headers": {
    "X-API-Key": "YOUR_MCP_API_KEY"
  },
  "description": "Date context analysis for location-aware AI applications"
}
```

**Steps:**
1. Log in to ElevenLabs Platform
2. Navigate to your Agent configuration
3. Go to MCP Integration settings
4. Add a new MCP server
5. Fill in the details from the configuration above
6. Replace `YOUR_APP_SERVICE_URL` with your Azure App Service URL
7. Replace `YOUR_MCP_API_KEY` with the key from Azure Key Vault
8. Save configuration

---

## Testing the Connection

After configuring your client, test the connection:

### Test with curl

```bash
# Replace YOUR_APP_SERVICE_URL and YOUR_MCP_API_KEY with actual values
curl -X POST https://YOUR_APP_SERVICE_URL/mcp \
  -H "X-API-Key: YOUR_MCP_API_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": 1,
    "method": "tools/list",
    "params": {}
  }'
```

**Expected Response:**
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "tools": [
      {
        "name": "analyze_date_context",
        "description": "Analyzes comprehensive date context for a location...",
        "inputSchema": { ... }
      }
    ]
  }
}
```

### Test the analyze_date_context tool

```bash
curl -X POST https://YOUR_APP_SERVICE_URL/mcp \
  -H "X-API-Key: YOUR_MCP_API_KEY" \
  -H "Content-Type: application/json" \
  -H "Mcp-Session-Id: test-session-123" \
  -d '{
    "jsonrpc": "2.0",
    "id": 2,
    "method": "tools/call",
    "params": {
      "name": "analyze_date_context",
      "arguments": {
        "location": "Dubai"
      }
    }
  }'
```

---

## Configuration Values Summary

| Setting | Value |
|---------|-------|
| **Server URL** | `https://YOUR_APP_SERVICE_URL/mcp` (get from `terraform output app_service_url`) |
| **Authentication Header** | `X-API-Key` |
| **API Key Location** | Azure Key Vault → `YOUR_KEY_VAULT_NAME` → `mcp-api-key` |
| **Transport Protocol** | HTTP with JSON-RPC 2.0 |
| **Available Tools** | `analyze_date_context` |

---

## Troubleshooting

### 401 Unauthorized Error

**Cause:** Missing or invalid API key

**Solution:**
1. Verify the API key matches the value in Azure Key Vault (`mcp-api-key` secret)
2. Ensure the header name is exactly `X-API-Key` (case-sensitive)
3. Check that the key hasn't been rotated recently

### 429 Too Many Requests

**Cause:** Rate limit exceeded (100 requests/minute per IP)

**Solution:** Wait 60 seconds before retrying

### Connection Timeout

**Cause:** App Service might be in cold start or network issues

**Solution:**
1. Check App Service status in Azure Portal
2. Verify the URL is correct
3. Test with curl to isolate client-specific issues
4. Check if IP allowlisting is configured and your IP is included

### No Tools Available

**Cause:** MCP initialization failed or wrong endpoint

**Solution:**
1. Ensure you're using the `/mcp` endpoint (not just the base URL)
2. Check App Service logs for errors: `az webapp log tail --resource-group YOUR_RESOURCE_GROUP_NAME --name YOUR_APP_SERVICE_NAME`
3. Verify the server is running: `curl https://YOUR_APP_SERVICE_URL/health`

---

## Security Best Practices

1. **Never hardcode API keys** in configuration files committed to version control
2. **Use environment variables** or secure secret management for API keys
3. **Rotate API keys regularly** (see [DEPLOYMENT.md](DEPLOYMENT.md#updating-secrets))
4. **Monitor usage** via Application Insights for suspicious activity
5. **Restrict IP addresses** if possible (see [DEPLOYMENT.md](DEPLOYMENT.md#security-configuration))
6. **Keep server URLs private** - do not share in public repositories or documentation
7. **Use Azure Key Vault** as the source of truth for all secrets

---

## Related Documentation

- [DEPLOYMENT.md](DEPLOYMENT.md) - Deployment and infrastructure setup
- [SECURITY.md](SECURITY.md) - Security architecture and controls
- [README.md](../README.md) - Project overview and features
