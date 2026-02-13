# Deployment Configuration Guide

This document lists all the variables and setup required to deploy Anime Feed Manager to Azure.

## GitHub Secrets (Required)

These must be configured as **Repository Secrets** in GitHub (Settings > Secrets and variables > Actions > Secrets).

### Azure Authentication (OIDC)

| Secret | Description |
|--------|-------------|
| `AZURE_CLIENT_ID` | Azure AD application (service principal) client ID |
| `AZURE_TENANT_ID` | Azure AD tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |

For detailed OIDC setup instructions, see [OIDC-SETUP.md](OIDC-SETUP.md).

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

### Chrome (Browserless)

| Secret | Description |
|--------|-------------|
| `CHROME_TOKEN` | Authentication token for browserless Chrome container |

To generate a secure token:
```bash
openssl rand -base64 32
```

## Optional Parameters (Hardcoded Defaults)

These values are hardcoded in the infrastructure workflow (`.github/workflows/amf-infrastructure.yml`). Edit the workflow file to change them.

| Parameter | Default | Description |
|-----------|---------|-------------|
| `gmailFromName` | `Anime Feed Manager` | Email sender display name |
| `feedNotificationSchedule` | `0 0 * * * *` | Cron schedule for feed notifications (hourly) |
| `scrapingSchedule` | `0 0 4 * * 6` | Cron schedule for full library scraping (weekly Saturday 4 AM UTC) |
| `feedTitlesUpdateSchedule` | `0 0 0 1 1 *` | Cron schedule for feed titles update (disabled - once per year) |

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
- `0 0 0 * * *` - Every day at midnight UTC
- `0 0 4 * * 6` - Every Saturday at 4 AM UTC (default for scraping)
- `0 30 8 * * *` - Every day at 8:30 AM UTC
- `0 0 */6 * * *` - Every 6 hours

### Season Rush Mode

During anime season transitions (~1 day before to ~20 days after season start), enable "rush mode" to catch new series faster.

**Season start dates:** Winter (Jan 1), Spring (Apr 1), Summer (Jul 1), Fall (Oct 1)

#### Schedule Presets

| Mode | `scrapingSchedule` | `feedTitlesUpdateSchedule` | Description |
|------|-------------------|---------------------------|-------------|
| **Normal** | `0 0 4 * * 6` | `0 0 0 1 1 *` | Weekly scraping, titles update disabled |
| **Rush** | `0 0 4 * * *` | `0 0 */4 * * *` | Daily scraping (4 AM UTC), titles every 4 hours |

#### CLI Commands

**Switch to Rush Mode:**

```bash
az functionapp config appsettings set \
  --name <FUNCTION_APP_NAME> \
  --resource-group <RESOURCE_GROUP> \
  --settings \
    "ScrapingSchedule=0 0 4 * * *" \
    "FeedTitlesUpdateSchedule=0 0 */4 * * *"
```

**Switch to Normal Mode:**

```bash
az functionapp config appsettings set \
  --name <FUNCTION_APP_NAME> \
  --resource-group <RESOURCE_GROUP> \
  --settings \
    "ScrapingSchedule=0 0 4 * * 6" \
    "FeedTitlesUpdateSchedule=0 0 0 1 1 *"
```

**Check Current Settings:**

```bash
az functionapp config appsettings list \
  --name <FUNCTION_APP_NAME> \
  --resource-group <RESOURCE_GROUP> \
  --query "[?name=='ScrapingSchedule' || name=='FeedTitlesUpdateSchedule' || name=='FeedNotificationSchedule'].{name:name, value:value}" \
  --output table
```

**Notes:**
- Feed titles update is lighter than full scraping (only hits SubsPlease schedule page)
- Both still use Chrome container - avoid overlapping schedules too closely
- App settings changes trigger automatic Function App restart (~30 seconds)
- Schedule changes take effect immediately after restart

## Deployment Order

1. **Configure GitHub Secrets** - Add all required secrets listed above
2. **Run Infrastructure Workflow** - Deploy Azure resources
   - This creates the storage account and other resources
   - **Note:** This run will fail at the "Seed Admin User" step because RBAC roles aren't set yet
3. **Grant RBAC to OIDC Principal** - Required for admin seeding and Functions deployment (see [OIDC-SETUP.md](OIDC-SETUP.md))
   - Storage Table Data Contributor (for admin seeding)
   - Storage Blob Data Contributor (for Functions deployment)
4. **Wait 5 minutes** - RBAC role propagation takes time
5. **Re-run Infrastructure Workflow** - This time admin user seeding will succeed
6. **Deploy Functions** - Run the Functions workflow manually or push changes to trigger
7. **Deploy Web App** - Run the Blazor workflow manually or push changes to trigger

## RBAC Setup for Deployer

The OIDC service principal (deployer) needs these Azure RBAC roles:

### Subscription/Resource Group Level (required before first deployment)

| Role | Purpose |
|------|---------|
| Contributor | Create and manage Azure resources |
| User Access Administrator | Assign RBAC roles to Function App and Web App managed identities |

### Storage Account Level (required after first deployment)

After the first infrastructure deployment creates the storage account, grant these additional roles:

#### Storage Table Data Contributor (for admin seeding)

```bash
az role assignment create \
  --assignee <AZURE_CLIENT_ID> \
  --role "Storage Table Data Contributor" \
  --scope /subscriptions/<SUBSCRIPTION_ID>/resourceGroups/<RESOURCE_GROUP>/providers/Microsoft.Storage/storageAccounts/<STORAGE_ACCOUNT>
```

#### Storage Blob Data Contributor (for Functions deployment)

The deployment uses Managed Identity (RBAC) instead of storage account keys. This means the deployer needs blob access to upload deployment packages:

```bash
az role assignment create \
  --assignee <AZURE_CLIENT_ID> \
  --role "Storage Blob Data Contributor" \
  --scope /subscriptions/<SUBSCRIPTION_ID>/resourceGroups/<RESOURCE_GROUP>/providers/Microsoft.Storage/storageAccounts/<STORAGE_ACCOUNT>
```

Wait 5 minutes for RBAC propagation before running the workflows.

## Resources Created

The Bicep templates create the following Azure resources:

| Resource | Description | SKU |
|----------|-------------|-----|
| Resource Group | Container for all resources | - |
| Storage Account | Tables, queues, and blobs | Standard_LRS |
| Function App Plan | Hosting plan for Functions | Flex Consumption (FC1) |
| Function App | Backend processing | - |
| Web App Plan | Hosting plan for Web App | Basic (B2) |
| Web App | Blazor SSR frontend | - |
| Chrome Web App | Browserless Chrome container | (shared plan) |
| SignalR Service | Real-time notifications | Free_F1 |
| Application Insights | Monitoring and diagnostics | - |

**Notes:**
- The Web App Plan uses Basic B2 (2 cores, 3.5 GB RAM) to support Chrome container concurrency needs. Both the web app and Chrome container share the same plan.
- Web App has health checks enabled at `/health` endpoint for Azure monitoring.
- Chrome container is configured with `CONCURRENT=3`, `QUEUED=5`, and health checks (`HEALTH=true`, `MAX_MEMORY_PERCENT=80`, `MAX_CPU_PERCENT=80`).
- Storage account has `allowBlobPublicAccess: true` to serve images publicly. The `images` container is set to `Blob` access (anonymous read, no listing).

### Manual: Enable Public Blob Access

If images aren't loading (409 Public access not permitted), run these commands:

```bash
# Step 1: Enable public access on storage account
az storage account update --name <STORAGE_ACCOUNT> --resource-group <RESOURCE_GROUP> --allow-blob-public-access true

# Step 2: Set images container to public blob access
az storage container set-permission --name images --account-name <STORAGE_ACCOUNT> --public-access blob --account-key $(az storage account keys list --account-name <STORAGE_ACCOUNT> --resource-group <RESOURCE_GROUP> --query "[0].value" -o tsv)
```

## Workflows

| Workflow | Trigger | Description |
|----------|---------|-------------|
| `amf-infrastructure.yml` | `deployment/**` changes | Deploys Bicep templates and seeds admin user |
| `amf-functions.yml` | Functions/Features code changes | Builds and deploys Azure Functions |
| `amf-blazor.yml` | Web/Features code changes | Builds CSS, deploys Blazor Web App, and sets version info |
| `amf-chrome-refresh.yml` | Manual only | Restarts Chrome App Service to pull latest container image |

All workflows:
- Support `workflow_dispatch` for manual triggering via GitHub Actions UI
- Infrastructure, Functions, and Blazor workflows also trigger automatically on push to `main` when relevant paths change

---

## Azure Functions Flex Consumption Plan

This deployment uses the **Flex Consumption** plan for Azure Functions.

### Why Flex Consumption?

| Feature | Linux Consumption (Legacy) | Flex Consumption |
|---------|---------------------------|------------------|
| **.NET 10 on Linux** | Not supported | Supported |
| **Cold starts** | Slower | Faster (Always Ready instances) |
| **Deployment** | WEBSITE_RUN_FROM_PACKAGE | One Deploy to blob container |
| **EOL** | September 30, 2028 | Current/recommended |

### Function App Managed Identity Roles

The Function App's system-assigned managed identity needs:
- Storage Blob Data Contributor (for deployments and blob access)
- Storage Queue Data Contributor (for queue triggers)
- Storage Table Data Contributor (for table storage)
- Storage Account Contributor (for deployment container access)

These are automatically assigned by the Bicep templates.

---

## Storage Authentication

The deployment uses Managed Identity with URI-based connections (no storage keys):

```bicep
{
  name: 'AzureWebJobsStorage__accountName'
  value: storageAccountName
}
{
  name: 'ConnectionStrings__BlobConnection'
  value: 'https://${storageAccountName}.blob.${environment().suffixes.storage}'
}
```

Benefits:
- No keys stored in app settings (more secure)
- Managed Identity handles authentication automatically

The `azure/functions-action` detects RBAC mode when it sees `AzureWebJobsStorage__accountName` and uses the workflow's OIDC credentials to upload deployment packages.

---

## Legacy Secrets (Can Be Deleted)

These secrets were used by previous deployments and are no longer needed:

| Secret | Reason No Longer Needed |
|--------|------------------------|
| `AZURE_FUNCTIONAPP_PUBLISH_PROFILE` | Uses OIDC authentication instead of publish profiles |
| `AZURE_RG` | Resource group name is hardcoded in workflow |
| `AZURE_STATIC_WEB_APPS_API_TOKEN_*` | Uses App Service instead of Static Web Apps |
| `REPO_URL` | Legacy deployment configuration |
| `SENDER_EMAIL` | Replaced by `GMAIL_FROM_EMAIL` |
