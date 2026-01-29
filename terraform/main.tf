terraform {
  required_version = ">= 1.6"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}

provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy    = true
      recover_soft_deleted_key_vaults = true
    }
  }
  subscription_id = var.subscription_id
}

# Get current Azure client configuration
data "azurerm_client_config" "current" {}

# Computed tags with Created_On_Date
locals {
  tags = merge(
    var.tags,
    {
      Created_On_Date = timestamp()
    }
  )
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location
  tags     = local.tags

  lifecycle {
    ignore_changes = [tags["Created_On_Date"]]
  }
}

# Azure Key Vault
resource "azurerm_key_vault" "main" {
  name                       = "kv-${var.app_name}-${var.environment}"
  location                   = azurerm_resource_group.main.location
  resource_group_name        = azurerm_resource_group.main.name
  tenant_id                  = data.azurerm_client_config.current.tenant_id
  sku_name                   = "standard"
  soft_delete_retention_days = 7
  purge_protection_enabled   = true
  rbac_authorization_enabled = false

  network_acls {
    default_action = "Allow"
    bypass         = "AzureServices"
  }

  tags = local.tags

  lifecycle {
    ignore_changes = [tags["Created_On_Date"]]
  }
}

# Key Vault Access Policy for current user (for deployment)
resource "azurerm_key_vault_access_policy" "deployer" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azurerm_client_config.current.object_id

  secret_permissions = [
    "Get",
    "List",
    "Set",
    "Delete",
    "Purge",
    "Recover"
  ]
}

# Store Google API Key in Key Vault
resource "azurerm_key_vault_secret" "google_api_key" {
  name         = "google-api-key"
  value        = var.google_api_key
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [azurerm_key_vault_access_policy.deployer]
}

# Store MCP API Key in Key Vault
resource "azurerm_key_vault_secret" "mcp_api_key" {
  name         = "mcp-api-key"
  value        = var.mcp_api_key
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [azurerm_key_vault_access_policy.deployer]
}

# Application Insights
resource "azurerm_application_insights" "main" {
  name                = "ai-${var.app_name}-${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  application_type    = "web"
  retention_in_days   = 30

  tags = local.tags

  lifecycle {
    ignore_changes = [tags["Created_On_Date"]]
  }
}

# Log Analytics Workspace for Application Insights
resource "azurerm_log_analytics_workspace" "main" {
  name                = "law-${var.app_name}-${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "PerGB2018"
  retention_in_days   = 30

  tags = local.tags

  lifecycle {
    ignore_changes = [tags["Created_On_Date"]]
  }
}

# App Service Plan (Windows, Basic tier for simple deployment)
resource "azurerm_service_plan" "main" {
  name                = "asp-${var.app_name}-${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  os_type             = "Windows"
  sku_name            = var.app_service_plan_sku
  tags                = local.tags

  lifecycle {
    ignore_changes = [tags["Created_On_Date"]]
  }
}

# Windows Web App (.NET 9)
resource "azurerm_windows_web_app" "api" {
  name                = "app-${var.app_name}-${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  service_plan_id     = azurerm_service_plan.main.id
  https_only          = true
  tags                = local.tags

  # Enable System Assigned Managed Identity
  identity {
    type = "SystemAssigned"
  }

  site_config {
    always_on           = var.app_service_plan_sku != "F1"
    ftps_state          = "Disabled"
    minimum_tls_version = "1.2"
    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5

    application_stack {
      current_stack  = "dotnet"
      dotnet_version = "v9.0"
    }

    scm_minimum_tls_version = "1.2"

    # IP restrictions (NSG at application level)
    dynamic "ip_restriction" {
      for_each = length(var.allowed_ip_addresses) > 0 ? var.allowed_ip_addresses : []
      content {
        ip_address = ip_restriction.value
        action     = "Allow"
        priority   = 100 + ip_restriction.key
        name       = "AllowedIP_${ip_restriction.key}"
      }
    }
  }

  app_settings = {
    # Security settings
    "MCP_API_KEY"    = var.mcp_api_key
    "KEY_VAULT_URI"  = azurerm_key_vault.main.vault_uri

    # Application Insights
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = azurerm_application_insights.main.connection_string
    "ApplicationInsightsAgent_EXTENSION_VERSION" = "~3"

    # Application settings (using Key Vault references)
    "Google__ApiKey"                    = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.main.name};SecretName=${azurerm_key_vault_secret.google_api_key.name})"
    "DateService__HolidayCacheTtlDays"  = "30"
    "DateService__GeocodeCacheTtlDays"  = "7"
    "DateService__HolidayLookaheadDays" = "90"

    # Rate Limiting (100 for dev, 5000+ for prod with ElevenLabs)
    "RateLimiting__PermitLimit"    = tostring(var.rate_limit_permits)
    "RateLimiting__WindowMinutes"  = tostring(var.rate_limit_window_minutes)

    # Retry Policy
    "RetryPolicy__RetryCount"        = tostring(var.retry_count)
    "RetryPolicy__BaseDelaySeconds"  = tostring(var.retry_base_delay_seconds)

    # .NET settings
    "ASPNETCORE_ENVIRONMENT" = var.environment == "prod" ? "Production" : "Development"
  }

  lifecycle {
    ignore_changes = [tags["Created_On_Date"]]
  }
}

# Key Vault Access Policy for App Service Managed Identity
# NOTE: This must be applied AFTER the web app is created (run terraform apply twice)
# First run: Creates web app with managed identity
# Second run: Creates this access policy using the identity
resource "azurerm_key_vault_access_policy" "app_identity" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_windows_web_app.api.identity[0].principal_id

  secret_permissions = [
    "Get",
    "List"
  ]

  depends_on = [
    azurerm_key_vault_access_policy.deployer,
    azurerm_windows_web_app.api
  ]
}

# Output
output "resource_group_name" {
  description = "Resource group name"
  value       = azurerm_resource_group.main.name
}

output "app_service_name" {
  description = "App Service name"
  value       = azurerm_windows_web_app.api.name
}

output "app_url" {
  description = "Application URL"
  value       = "https://${azurerm_windows_web_app.api.default_hostname}"
}

output "key_vault_uri" {
  description = "Key Vault URI"
  value       = azurerm_key_vault.main.vault_uri
}

output "application_insights_instrumentation_key" {
  description = "Application Insights Instrumentation Key"
  value       = azurerm_application_insights.main.instrumentation_key
  sensitive   = true
}

output "managed_identity_principal_id" {
  description = "App Service Managed Identity Principal ID"
  value       = azurerm_windows_web_app.api.identity[0].principal_id
}

output "deployment_command" {
  description = "Command to deploy the app using az cli"
  value       = "az webapp deploy --resource-group ${azurerm_resource_group.main.name} --name ${azurerm_windows_web_app.api.name} --src-path <path-to-zip>"
}
