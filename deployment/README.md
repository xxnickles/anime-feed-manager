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
| `feedNotificationSchedule` | `0 0 * * * *` | Cron schedule for feed notifications |
| `scrapingSchedule` | `0 0 0 * * *` | Cron schedule for library scraping |

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
2. **Run Infrastructure Workflow** - Deploy Azure resources
   - This creates the storage account and other resources
   - **Note:** This run will fail at the "Seed Admin User" step because RBAC roles aren't set yet
3. **Grant RBAC to OIDC Principal** - Required for admin seeding and Functions deployment (see below)
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

| Resource | Name | SKU |
|----------|------|-----|
| Resource Group | `amf-rg` | - |
| Storage Account | `amfstorage` | Standard_LRS |
| Function App Plan | `amf-functions-plan` | Flex Consumption (FC1) |
| Function App | `amf-functions` | - |
| Web App Plan | `amf-web-manager-plan` | Basic (B1) |
| Web App | `amf-web-manager` | - |
| Chrome Web App | `amf-chrome` | (shared plan) |
| SignalR Service | `amf-signalr` | Free_F1 |
| Application Insights | `amf-insights` | - |

**Note:** The Web App Plan was upgraded from Free (F1) to Basic (B1) to support the containerized Chrome app. Both `amf-web-manager` and `amf-chrome` share the same plan.

## Workflows

| Workflow | Trigger | Description |
|----------|---------|-------------|
| `amf-infrastructure.yml` | `deployment/**` changes | Deploys Bicep templates and seeds admin user |
| `amf-functions.yml` | Functions/Features code changes | Builds and deploys Azure Functions |
| `amf-blazor.yml` | Web/Features code changes | Builds CSS and deploys Blazor Web App |

All workflows:
- Trigger automatically on push to `main` when relevant paths change
- Support `workflow_dispatch` for manual triggering via GitHub Actions UI

---

## OIDC Authentication Setup

### Overview

GitHub Actions authenticate to Azure using OpenID Connect (OIDC) with federated credentials. This approach:
- Requires no secrets to store or rotate
- GitHub provides a short-lived token for each workflow run
- Azure validates the token against configured trust conditions

### Federated Credential Configuration

Federated credentials are configured on the **App Registration** in Azure.

**Location:** Azure Portal > Microsoft Entra ID > App registrations > `amf-deployer` > Certificates & secrets > Federated credentials

#### Branch-Based Credential

| Field | Value |
|-------|-------|
| Federated credential scenario | GitHub Actions deploying Azure resources |
| Organization | `xxnickles` |
| Repository | `anime-feed-manager` |
| Entity type | Branch |
| Branch name | `main` |
| Name | `github-main-branch` |

**Subject identifier generated:** `repo:xxnickles/anime-feed-manager:ref:refs/heads/main`

### Required RBAC Roles Summary

| Role | Scope | Purpose |
|------|-------|---------|
| Contributor | Subscription or Resource Group | Create/manage Azure resources |
| User Access Administrator | Subscription or Resource Group | Assign RBAC roles to managed identities |
| Storage Blob Data Contributor | Storage Account | Upload deployment packages for Functions |
| Storage Table Data Contributor | Storage Account | Seed admin user during infrastructure deployment |

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
