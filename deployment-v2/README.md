# Deployment V2 - Configuration Guide

This document lists all the variables required to deploy Anime Feed Manager to Azure.

## GitHub Secrets (Required)

These must be configured as **Repository Secrets** in GitHub (Settings > Secrets and variables > Actions > Secrets).

### Azure Authentication (OIDC)

| Secret | Description |
|--------|-------------|
| `AZURE_CLIENT_ID` | Azure AD application (service principal) client ID |
| `AZURE_TENANT_ID` | Azure AD tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |

### Passwordless Authentication

| Secret | Description |
|--------|-------------|
| `PASSWORDLESS_API_KEY` | Passwordless.dev API key (public) |
| `PASSWORDLESS_API_SECRET` | Passwordless.dev API secret |

### Gmail SMTP

| Secret | Description |
|--------|-------------|
| `GMAIL_FROM_EMAIL` | Gmail sender email address |
| `GMAIL_APP_PASSWORD` | Gmail app-specific password (not your account password) |

To create a Gmail app password:
1. Enable 2-Step Verification on your Google account
2. Go to Google Account > Security > App passwords
3. Create a new app password for "Mail"

### Admin User Seeding

| Secret | Description |
|--------|-------------|
| `ADMIN_USER_ID` | Passwordless user ID for the admin user |
| `ADMIN_EMAIL` | Admin user email address |

## GitHub Variables (Optional)

These can be configured as **Repository Variables** in GitHub (Settings > Secrets and variables > Actions > Variables).

If not set, default values will be used.

| Variable | Default | Description |
|----------|---------|-------------|
| `GMAIL_FROM_NAME` | `Anime Feed Manager` | Email sender display name |
| `FEED_NOTIFICATION_SCHEDULE` | `0 0 * * * *` | Cron schedule for feed notifications |
| `SCRAPING_SCHEDULE` | `0 0 0 * * *` | Cron schedule for library scraping |

### Cron Schedule Format

Schedules use Azure Functions NCrontab format with **6 fields**:

```
┌───────────── second (0-59)
│ ┌───────────── minute (0-59)
│ │ ┌───────────── hour (0-23)
│ │ │ ┌───────────── day of month (1-31)
│ │ │ │ ┌───────────── month (1-12)
│ │ │ │ │ ┌───────────── day of week (0-6, Sunday=0)
│ │ │ │ │ │
* * * * * *
```

**Examples:**
- `0 0 * * * *` - Every hour at minute 0 (default for notifications)
- `0 0 0 * * *` - Every day at midnight UTC (default for scraping)
- `0 30 8 * * *` - Every day at 8:30 AM UTC
- `0 0 */6 * * *` - Every 6 hours

## Deployment Order

1. **Configure GitHub Secrets** - Add all required secrets listed above
2. **Configure GitHub Variables** (optional) - Override defaults if needed
3. **Run Infrastructure Workflow** - Deploy Azure resources (see [Manual Triggering](#manual-workflow-triggering))
4. **Grant RBAC to OIDC Principal** - Required for admin seeding and Functions deployment (see below)
5. **Re-run Infrastructure Workflow** - Seeds admin user
6. **Deploy Functions** - Run the Functions workflow manually
7. **Deploy Web App** - Run the Blazor workflow manually

## RBAC Setup for Deployer

After the first infrastructure deployment, grant the OIDC service principal (deployer) these storage roles:

### Storage Table Data Contributor (for admin seeding)

```bash
az role assignment create \
  --assignee <AZURE_CLIENT_ID> \
  --role "Storage Table Data Contributor" \
  --scope /subscriptions/<SUBSCRIPTION_ID>/resourceGroups/<RESOURCE_GROUP>/providers/Microsoft.Storage/storageAccounts/<STORAGE_ACCOUNT>
```

### Storage Blob Data Contributor (for Functions deployment)

The V2 deployment uses Managed Identity (RBAC) instead of storage account keys. This means the deployer needs blob access to upload deployment packages:

```bash
az role assignment create \
  --assignee <AZURE_CLIENT_ID> \
  --role "Storage Blob Data Contributor" \
  --scope /subscriptions/<SUBSCRIPTION_ID>/resourceGroups/<RESOURCE_GROUP>/providers/Microsoft.Storage/storageAccounts/<STORAGE_ACCOUNT>
```

Wait 5 minutes for RBAC propagation before running the workflows.

## Resources Created

| Resource | Name | SKU |
|----------|------|-----|
| Resource Group | `amf-rg` | - |
| Storage Account | `amfstorage` | Standard_LRS |
| Function App | `amf-functions` | Consumption (Y1) |
| Web App | `amf-web` | Free (F1) |
| SignalR Service | `amf-signalr` | Free_F1 |
| Application Insights | `amf-insights` | - |

## Workflows

| Workflow | Description |
|----------|-------------|
| `amf-v2-infrastructure.yml` | Deploys Bicep templates and seeds admin user |
| `amf-v2-functions.yml` | Builds and deploys Azure Functions |
| `amf-v2-blazor.yml` | Builds CSS and deploys Blazor Web App |

All workflows support `workflow_dispatch` for manual triggering.

## Manual Workflow Triggering

When workflows exist only on a non-default branch (e.g., `rewrite`), they won't appear in the GitHub Actions UI dropdown. Use the GitHub REST API to trigger them manually.

### Using curl

```bash
curl -X POST \
  -H "Accept: application/vnd.github+json" \
  -H "Authorization: Bearer <GITHUB_PAT>" \
  https://api.github.com/repos/<OWNER>/<REPO>/actions/workflows/<WORKFLOW_FILE>/dispatches \
  -d '{"ref":"<BRANCH>"}'
```

Replace:
- `<GITHUB_PAT>` - Your GitHub Personal Access Token with `repo` scope
- `<OWNER>/<REPO>` - Repository owner and name
- `<WORKFLOW_FILE>` - Workflow filename (e.g., `amf-v2-infrastructure.yml`)
- `<BRANCH>` - Target branch (e.g., `rewrite`)

A successful response returns **HTTP 204 No Content**.

For detailed instructions on creating a GitHub PAT and additional reference information, see `.claude/azure-github-deployment-reference.md`.
