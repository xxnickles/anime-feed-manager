@description('Azure region for all resources')
param location string = resourceGroup().location

@description('Storage account name')
param storageAccountName string

@description('Application Insights connection string')
param appInsightsConnectionString string

@secure()
@description('Gmail sender email address')
param gmailFromEmail string

@secure()
@description('Gmail app password')
param gmailAppPassword string

@description('Gmail sender display name')
param gmailFromName string = 'Anime Feed Manager'

@description('Cron schedule for feed notifications (NCrontab 6-field format)')
param feedNotificationSchedule string = '0 0 * * * *'

@description('Cron schedule for library scraping (NCrontab 6-field format)')
param scrapingSchedule string = '0 0 0 * * *'

@description('Web app URL for CORS')
param webAppUrl string = 'https://${loadJsonContent('shared-variables.json').webAppName}.azurewebsites.net'

@description('Chrome remote endpoint URL')
param chromeEndpoint string

@secure()
@description('Chrome authentication token')
param chromeToken string

// Load shared variables
var config = loadJsonContent('shared-variables.json')

// RBAC Role Definition IDs
var storageBlobDataContributorRole = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
var storageQueueDataContributorRole = '974c5e8b-45b9-4653-ba55-5f855dd0fb88'
var storageTableDataContributorRole = '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
var storageAccountContributorRole = '17d1049b-9a84-46fb-8f53-869881c3d3ab'

// Reference to existing storage account
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: storageAccountName
}

// App Service Plan for Functions (Flex Consumption)
resource functionsPlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: config.functionsPlanName
  location: location
  kind: 'functionapp'
  sku: {
    tier: 'FlexConsumption'
    name: 'FC1'
  }
  properties: {
    reserved: true
  }
}

// Function App (Flex Consumption)
resource functionApp 'Microsoft.Web/sites@2024-04-01' = {
  name: config.functionAppName
  location: location
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: functionsPlan.id
    httpsOnly: true
    functionAppConfig: {
      deployment: {
        storage: {
          type: 'blobContainer'
          value: '${storageAccount.properties.primaryEndpoints.blob}deployments'
          authentication: {
            type: 'SystemAssignedIdentity'
          }
        }
      }
      scaleAndConcurrency: {
        maximumInstanceCount: 100
        instanceMemoryMB: 2048
      }
      runtime: {
        name: 'dotnet-isolated'
        version: '10.0'
      }
    }
    siteConfig: {
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      cors: {
        allowedOrigins: [
          webAppUrl
        ]
        supportCredentials: true
      }
    }
  }
}

// App Settings (separate resource for Flex Consumption)
resource functionAppSettings 'Microsoft.Web/sites/config@2024-04-01' = {
  parent: functionApp
  name: 'appsettings'
  properties: {
    // Host storage (managed identity)
    AzureWebJobsStorage__accountName: storageAccountName

    // Storage connections for Aspire SDK clients
    // Uses explicit Aspire configuration sections instead of ConnectionStrings__* format
    //
    // WHY NOT ConnectionStrings__*?
    // Azure Functions triggers check ConnectionStrings__<name> BEFORE <name>__queueServiceUri
    // If ConnectionStrings__QueueConnection exists, triggers try to parse the URI as a connection string
    // and fail with "Settings must be of the form 'name=value'"
    //
    // Aspire SDK client lookup order:
    // 1. ConnectionStrings:<name> (we skip this to avoid trigger conflicts)
    // 2. Aspire:Azure:Storage:Blobs:<name>:ServiceUri (we use this)
    Aspire__Azure__Storage__Blobs__BlobConnection__ServiceUri: 'https://${storageAccountName}.blob.${environment().suffixes.storage}'
    Aspire__Azure__Storage__Queues__QueueConnection__ServiceUri: 'https://${storageAccountName}.queue.${environment().suffixes.storage}'
    Aspire__Azure__Data__Tables__TablesConnection__ServiceUri: 'https://${storageAccountName}.table.${environment().suffixes.storage}'

    // Azure Functions trigger connections (managed identity format)
    // Queue triggers use Connection = "QueueConnection" which looks for __queueServiceUri
    QueueConnection__queueServiceUri: 'https://${storageAccountName}.queue.${environment().suffixes.storage}'

    SignalRConnectionString: signalR.listKeys().primaryConnectionString
    Gmail__FromEmail: gmailFromEmail
    Gmail__FromName: gmailFromName
    Gmail__AppPassword: gmailAppPassword
    APPLICATIONINSIGHTS_CONNECTION_STRING: appInsightsConnectionString
    Sandbox: 'false'
    RunHeadless: 'true'
    FeedNotificationSchedule: feedNotificationSchedule
    ScrapingSchedule: scrapingSchedule

    // Remote Chrome (browserless) configuration
    Chrome__RemoteEndpoint: chromeEndpoint
    Chrome__Token: chromeToken
  }
}

// SignalR Service
resource signalR 'Microsoft.SignalRService/signalR@2024-03-01' = {
  name: config.signalRName
  location: location
  sku: {
    name: 'Free_F1'
    tier: 'Free'
    capacity: 1
  }
  kind: 'SignalR'
  properties: {
    features: [
      {
        flag: 'ServiceMode'
        value: 'Serverless'
      }
    ]
    cors: {
      allowedOrigins: [
        webAppUrl
      ]
    }
  }
}

// RBAC: Storage Blob Data Contributor for Function App
resource blobRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, functionApp.id, storageBlobDataContributorRole)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataContributorRole)
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// RBAC: Storage Queue Data Contributor for Function App
resource queueRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, functionApp.id, storageQueueDataContributorRole)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageQueueDataContributorRole)
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// RBAC: Storage Table Data Contributor for Function App
resource tableRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, functionApp.id, storageTableDataContributorRole)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageTableDataContributorRole)
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// RBAC: Storage Account Contributor for Function App (needed for deployment container access)
resource storageAccountRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, functionApp.id, storageAccountContributorRole)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageAccountContributorRole)
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Outputs
output functionAppName string = functionApp.name
output signalREndpoint string = 'https://${functionApp.properties.defaultHostName}/api'
