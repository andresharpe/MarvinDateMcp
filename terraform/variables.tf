variable "subscription_id" {
  description = "Azure subscription ID (<subscription-name>)"
  type        = string
  default     = "<subscription-id>"
}

variable "environment" {
  description = "Environment name (test/prod)"
  type        = string
  default     = "test"
}

variable "location" {
  description = "Azure region"
  type        = string
  default     = "westeurope"
}

variable "app_name" {
  description = "Application name (lowercase, used in resource naming)"
  type        = string
  default     = "marvindatemcp"
}

variable "resource_group_name" {
  description = "Resource group name"
  type        = string
  default     = "YOUR_RESOURCE_GROUP_NAME"
}

variable "app_service_plan_sku" {
  description = "App Service Plan SKU (F1=Free, B1=Basic, S1=Standard)"
  type        = string
  default     = "B1"
}

# Application secrets
variable "google_api_key" {
  description = "Google API Key for Geocoding and Time Zone APIs"
  type        = string
  sensitive   = true
}

# Resource tags
variable "tags" {
  description = "Azure resource tags"
  type        = map(string)
  default = {
    Application          = "MarvinDateMcp"
    Application_Owner    = "andre.sharpe@example.com"
    Application_Type     = "PaaS"
    Business_Criticality = "NoBC"
    DR_Tag               = "NoDR"
    Data_Classification  = "Internal"
    Deployed_By          = "Infra_terraform"
    Environment          = "TEST"
    Incident_Severity    = "n/a"
    Managed_By           = "Terraform"
    Purpose              = "MCP_Date_Context_Server"
    SLA_Tier             = "NoSLA"
    Status               = "PoC"
    System_Owner         = "andre.sharpe@example.com"
    Take_On_Stream       = "MP"
  }
}
