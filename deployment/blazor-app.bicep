@description('Optional Git Repo URL')
param repoUrl string = ' '

@description('Storage Account Name')
param storageAccountName string

@description('Instrumentation Key')
param instrumentationKey string

@secure()
param passwordlessApiKey string

@secure()
param passwordlessApiSecret string

@description('Location for all resources.')
param location string = resourceGroup().location

var webAppName = 'afm-blazor-ssr'
var webAppHostingPlanName = 'afm-blazor-hosting-plan'
var keyVaultName = 'afm-key-vault'
var managedIdentityName = 'afm-managed-identity'

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: managedIdentityName
}

// Create the role assignments for Azure Storage. Doesn't work as you will need to have some werid permisions 

// var roleDefinitionIds = [
//   '974c5e8b-45b9-4653-ba55-5f855dd0fb88'  //Storage Queue Data Contributor
//   '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa'   //Storage Table Data Contributor
//   'ba92f5b4-2d11-453d-a403-e96b0029c9fe'  //Storage Blob Data Contributor
// ]

// resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for roleDefinitionId in roleDefinitionIds: {
//   scope: storageAccount
//   name: guid(storageAccount.id, managedIdentity.id, roleDefinitionId)
//   properties: {
//     roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionId)
//     principalId: managedIdentity.id
//     principalType: managedIdentity.type
//   }
// }]



resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: managedIdentity.properties.principalId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }
    ]
    enabledForDeployment: true
    enabledForDiskEncryption: true
    enabledForTemplateDeployment: true
  }
}

resource passworlessKey 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'Passwordless--ApiKey'
  properties: {
    value: passwordlessApiKey
    contentType: 'string'
  }
}

resource passworlessSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'Passwordless--ApiSecret'
  properties: {
    value: passwordlessApiSecret
    contentType: 'string'
  }
}

resource blazorHostingPlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: webAppHostingPlanName
  location: location
  kind: 'linux'
  properties: {
    reserved: true
  }
  sku: {
    name: 'F1'
  }
}

resource blazorAppService 'Microsoft.Web/sites@2023-01-01' = {
  name: webAppName
  location: location
  identity: {
    type:'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    serverFarmId: blazorHostingPlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'VaultUri'
          value: 'https://${keyVault.name}.vault.azure.net/'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: instrumentationKey
        }
        {
          name: 'StorageAccountName'
          value: storageAccountName
        }
        {
          name: 'AZURE_CLIENT_ID'
          value: managedIdentity.properties.clientId
        }
      ]
      linuxFxVersion: 'DOTNETCORE|8.0'
    }
    keyVaultReferenceIdentity: managedIdentity.id
    httpsOnly: true
    
  }
}

resource webAppSourceControl 'Microsoft.Web/sites/sourcecontrols@2023-01-01' = if(contains(repoUrl,'http')){
  name: 'web'
  parent: blazorAppService
  properties: {
    repoUrl: repoUrl
    branch: 'main'
    isManualIntegration: true
  }
}

output appname string = blazorAppService.name

