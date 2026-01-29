variable "subscription_id" {
  description = "Azure subscription ID"
  type        = string
  # No default - must be provided via terraform.tfvars or -var flag
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

variable "mcp_api_key" {
  description = "API Key for MCP endpoint authentication"
  type        = string
  sensitive   = true
}

# Security settings
variable "allowed_ip_addresses" {
  description = "List of allowed IP addresses/CIDR blocks for NSG rules"
  type        = list(string)
  default     = []
}

# Rate Limiting
variable "rate_limit_permits" {
  description = "Max requests per window per IP (100 for dev, 5000+ for prod with ElevenLabs)"
  type        = number
  default     = 100
}

variable "rate_limit_window_minutes" {
  description = "Rate limit window in minutes"
  type        = number
  default     = 1
}

# Retry Policy
variable "retry_count" {
  description = "Number of HTTP retry attempts for transient errors"
  type        = number
  default     = 3
}

variable "retry_base_delay_seconds" {
  description = "Base delay in seconds for exponential backoff"
  type        = number
  default     = 2
}

# Resource tags
variable "tags" {
  description = "Azure resource tags"
  type        = map(string)
  default = {
    Application          = "MarvinDateMcp"
    Application_Owner    = "owner@example.com"
    Application_Type     = "PaaS"
    Business_Criticality = "NoBC"
    DR_Tag               = "NoDR"
    Data_Classification  = "Internal"
    Deployed_By          = "Infra_terraform"
    Environment          = "TEST"
    Incident_Severity    = "n/a"
    Managed_By           = "IWG"
    Purpose              = "MCP_Date_Context_Server"
    SLA_Tier             = "NoSLA"
    Status               = "PoC"
    System_Owner         = "owner@example.com"
    Take_On_Stream       = "MP"
  }
}
