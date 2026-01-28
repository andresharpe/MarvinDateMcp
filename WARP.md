# Project Rules

## File Organization
- **Never** create files in project root - use appropriate directories
- Logs/downloads → `./logs/` (gitignored)
- Temp scripts/test files → `./temp-scripts/` (gitignored)
- Working notes/analysis → `./workspace/` (gitignored)
- Terraform files → `./terraform/`

## Azure Deployment
- Subscription: `<subscription-name>` (<subscription-id>)
- Resource naming: `RG_WE_APPS_{app}_TEST`, `app-{name}-test`
- IWG tagging policy: 12+ mandatory tags (see `terraform/variables.tf`)
- Deploy via: `./terraform/deploy.ps1` or `az webapp deploy`

## Code Standards
- Target: .NET 9 (Azure App Service compatible)
- `TreatWarningsAsErrors` enabled
- Secrets: App Settings, not Key Vault (for now)

## Known Issues
- Linux App Service crashes with exit code 134 - investigation ongoing
