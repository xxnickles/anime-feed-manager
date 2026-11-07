@description('Azure region for all resources')
param location string = resourceGroup().location

@description('Storage account name')
param storageAccountName string

@description('Application Insights connection string')
param appInsightsConnectionString string

@description('SignalR Service name')
param signalRName string

@secure()
@description('Passwordless API key')
param passwordlessApiKey string

@secure()
@description('Passwordless API secret')
param passwordlessApiSecret string

// Load shared variables
var config = loadJsonContent('shared-variables.json')

// Reference to existing storage account
resource storageAccount 'Microsoft.Storage/storageAccounts@2026-06-01' existing = {
  name: storageAccountName
}

resource signalR 'Microsoft.SignalRService/signalR@2024-03-01' existing = {
  name: signalRName
}

// RBAC Role Definition IDs
var storageBlobDataContributorRole = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
var storageQueueDataContributorRole = '974c5e8b-45b9-4653-ba55-5f855dd0fb88'
var storageTableDataContributorRole = '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
var signalRRestApiOwnerRole = 'fd53cd77-2268-407a-8f46-7e7863d0f521'

// App Service Plan for the Web App
resource webPlan 'Microsoft.Web/serverfarms@2025-03-01' = {
  name: config.webPlanName
  location: location
  kind: 'linux'
  sku: {
    name: 'B2'
    tier: 'Basic'
  }
  properties: {
    reserved: true
  }
}

// Web App (Blazor SSR)
resource webApp 'Microsoft.Web/sites@2025-03-01' = {
  name: config.webAppName
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: webPlan.id
    reserved: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      alwaysOn: false
      healthCheckPath: '/health'
      appSettings: [
        {
          name: 'ConnectionStrings__BlobConnection'
          value: 'https://${storageAccountName}.blob.${environment().suffixes.storage}'
        }
        {
          name: 'ConnectionStrings__QueueConnection'
          value: 'https://${storageAccountName}.queue.${environment().suffixes.storage}'
        }
        {
          name: 'ConnectionStrings__TablesConnection'
          value: 'https://${storageAccountName}.table.${environment().suffixes.storage}'
        }
        {
          name: 'Passwordless__ApiKey'
          value: passwordlessApiKey
        }
        {
          name: 'Passwordless__ApiSecret'
          value: passwordlessApiSecret
        }
        {
          name: 'ConnectionStrings__signalr'
          value: 'Endpoint=https://${signalR.properties.hostName};AuthType=azure.msi;Version=1.0;'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
      ]
    }
    httpsOnly: true
  }
}

// RBAC: Storage Blob Data Contributor for Web App
resource blobRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, webApp.id, storageBlobDataContributorRole)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataContributorRole)
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// RBAC: Storage Queue Data Contributor for Web App
resource queueRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, webApp.id, storageQueueDataContributorRole)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageQueueDataContributorRole)
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// RBAC: Storage Table Data Contributor for Web App
resource tableRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, webApp.id, storageTableDataContributorRole)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageTableDataContributorRole)
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// RBAC: SignalR REST API Owner for Web App (negotiate tokens, group sync)
resource signalRRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(signalR.id, webApp.id, signalRRestApiOwnerRole)
  scope: signalR
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', signalRRestApiOwnerRole)
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Outputs
output webAppName string = webApp.name
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output webPlanId string = webPlan.id
