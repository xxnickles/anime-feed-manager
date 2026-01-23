# Azure OIDC Setup for GitHub Actions

This guide explains how to configure secretless authentication between GitHub Actions and Azure using OpenID Connect (OIDC) with federated credentials.

## Overview

GitHub Actions authenticate to Azure using OIDC with federated credentials. This approach:
- Requires no secrets to store or rotate
- GitHub provides a short-lived token for each workflow run
- Azure validates the token against configured trust conditions

## Step 1: Create the App Registration

### Azure Portal

1. Go to **Azure Portal** > **Microsoft Entra ID** > **App registrations**
2. Click **New registration**
3. Configure:
   - **Name:** Choose a descriptive name (e.g., `myapp-deployer`)
   - **Supported account types:** Accounts in this organizational directory only
   - **Redirect URI:** Leave blank
4. Click **Register**
5. Copy the **Application (client) ID** and **Directory (tenant) ID**

### CLI

```bash
# Create the app registration
az ad app create --display-name "myapp-deployer"

# Get the Application (client) ID
az ad app list --display-name "myapp-deployer" --query "[0].appId" -o tsv

# Get the Tenant ID
az account show --query tenantId -o tsv
```

## Step 2: Create the Service Principal

### Azure Portal

The service principal is created automatically when you assign roles (Step 4).

### CLI

```bash
az ad sp create --id <APPLICATION_CLIENT_ID>
```

## Step 3: Add Federated Credential

### Azure Portal

1. Go to your App Registration > **Certificates & secrets** > **Federated credentials**
2. Click **Add credential**
3. Select **GitHub Actions deploying Azure resources**
4. Fill in:
   - **Organization:** Your GitHub organization or username
   - **Repository:** Your repository name
   - **Entity type:** Branch
   - **Branch name:** `main` (or your deployment branch)
   - **Name:** A descriptive name (e.g., `github-main-branch`)
5. Click **Add**

### CLI

```bash
# Create federated credential JSON
cat > federated-credential.json << 'EOF'
{
  "name": "github-main-branch",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:<OWNER>/<REPO>:ref:refs/heads/main",
  "description": "GitHub Actions deployment from main branch",
  "audiences": ["api://AzureADTokenExchange"]
}
EOF

# Add federated credential to app
az ad app federated-credential create \
  --id <APPLICATION_CLIENT_ID> \
  --parameters federated-credential.json

# Clean up
rm federated-credential.json
```

Replace `<OWNER>/<REPO>` with your GitHub organization/username and repository name.

## Step 4: Assign RBAC Roles

### Subscription/Resource Group Level

These roles are required for creating and managing Azure resources.

#### Azure Portal

1. Go to **Subscriptions** > Your subscription > **Access control (IAM)**
2. Click **Add** > **Add role assignment**
3. Assign these roles to the app registration:
   - **Contributor** - Create and manage resources
   - **User Access Administrator** - Assign RBAC roles to managed identities

#### CLI

```bash
SUBSCRIPTION_ID=$(az account show --query id -o tsv)

# Assign Contributor role
az role assignment create \
  --assignee <APPLICATION_CLIENT_ID> \
  --role "Contributor" \
  --scope /subscriptions/$SUBSCRIPTION_ID

# Assign User Access Administrator role
az role assignment create \
  --assignee <APPLICATION_CLIENT_ID> \
  --role "User Access Administrator" \
  --scope /subscriptions/$SUBSCRIPTION_ID
```

### Storage Account Level (After First Deployment)

After your storage account is created, add these additional roles:

```bash
SUBSCRIPTION_ID=$(az account show --query id -o tsv)
RESOURCE_GROUP="<your-resource-group>"
STORAGE_ACCOUNT="<your-storage-account>"

# Storage Table Data Contributor (for table operations)
az role assignment create \
  --assignee <APPLICATION_CLIENT_ID> \
  --role "Storage Table Data Contributor" \
  --scope /subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.Storage/storageAccounts/$STORAGE_ACCOUNT

# Storage Blob Data Contributor (for blob operations and deployments)
az role assignment create \
  --assignee <APPLICATION_CLIENT_ID> \
  --role "Storage Blob Data Contributor" \
  --scope /subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.Storage/storageAccounts/$STORAGE_ACCOUNT
```

## Step 5: Configure GitHub Secrets

Add these secrets to your GitHub repository (Settings > Secrets and variables > Actions > Secrets):

| Secret | Description |
|--------|-------------|
| `AZURE_CLIENT_ID` | Application (client) ID from Step 1 |
| `AZURE_TENANT_ID` | Directory (tenant) ID from Step 1 |
| `AZURE_SUBSCRIPTION_ID` | Your Azure subscription ID |

## Step 6: Configure GitHub Workflow

Your workflow needs these permissions and login configuration:

```yaml
permissions:
  id-token: write
  contents: read

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Azure Login (OIDC)
        uses: azure/login@v2
        with:
          client-id: ${{ secrets.AZURE_CLIENT_ID }}
          tenant-id: ${{ secrets.AZURE_TENANT_ID }}
          subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
```

## Required RBAC Roles Summary

| Role | Scope | Purpose |
|------|-------|---------|
| Contributor | Subscription/Resource Group | Create and manage Azure resources |
| User Access Administrator | Subscription/Resource Group | Assign RBAC roles to managed identities |
| Storage Blob Data Contributor | Storage Account | Upload deployment packages, access blobs |
| Storage Table Data Contributor | Storage Account | Access table storage |

## Verification

```bash
# Verify app registration exists
az ad app show --id <APPLICATION_CLIENT_ID>

# Verify federated credentials
az ad app federated-credential list --id <APPLICATION_CLIENT_ID>

# Verify role assignments
az role assignment list --assignee <APPLICATION_CLIENT_ID> --output table
```

## Troubleshooting

### AADSTS70021: No matching federated identity record found

The subject claim in the token doesn't match any federated credential. Verify:
- Organization and repository names are correct
- Branch name matches exactly
- Entity type is correct (Branch vs Environment vs Pull Request)

### Authorization failed

RBAC roles may not have propagated yet. Wait 5-10 minutes after assigning roles before running workflows.

### Storage access denied

Ensure the storage-level RBAC roles are assigned after the storage account is created.
