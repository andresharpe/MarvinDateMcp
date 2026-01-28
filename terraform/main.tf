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
  features {}
  subscription_id = var.subscription_id
}

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
  }

  app_settings = {
    # Application settings
    "Google__ApiKey"                    = var.google_api_key
    "DateService__HolidayCacheTtlDays"  = "30"
    "DateService__GeocodeCacheTtlDays"  = "7"
    "DateService__HolidayLookaheadDays" = "90"

    # .NET settings
    "ASPNETCORE_ENVIRONMENT" = var.environment == "prod" ? "Production" : "Development"
  }

  lifecycle {
    ignore_changes = [tags["Created_On_Date"]]
  }
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

output "deployment_command" {
  description = "Command to deploy the app using az cli"
  value       = "az webapp deploy --resource-group ${azurerm_resource_group.main.name} --name ${azurerm_windows_web_app.api.name} --src-path <path-to-zip>"
}
