@description('Azure region for all resources')
param location string = resourceGroup().location

// Load shared variables
var config = loadJsonContent('shared-variables.json')

// Storage Account
resource storageAccount 'Microsoft.Storage/storageAccounts@2026-06-01' = {
  name: config.storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: true // Required for public image access
    accessTier: 'Hot'
  }
}

// Blob Services (needed for deployment container)
resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2026-06-01' = {
  parent: storageAccount
  name: 'default'
}

// Deployment container for Flex Consumption Function App
resource deploymentContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2026-06-01' = {
  parent: blobServices
  name: 'deployments'
  properties: {
    publicAccess: 'None'
  }
}

// Images container (public read access for serving images)
resource imagesContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2026-06-01' = {
  parent: blobServices
  name: 'images'
  properties: {
    publicAccess: 'Blob' // Anonymous read for blobs only (can't list)
  }
}

// Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: config.appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
  }
}

// Operations workbook; queries run against App Insights (classic tables)
resource operationsWorkbook 'Microsoft.Insights/workbooks@2023-06-01' = {
  name: guid(resourceGroup().id, 'afm-operations-workbook')
  location: location
  kind: 'shared'
  properties: {
    displayName: 'AFM Operations'
    category: 'workbook'
    version: 'Notebook/1.0'
    sourceId: appInsights.id
    serializedData: replace(loadTextContent('../workbooks/afm-operations.workbook.json'), '__APP_INSIGHTS_ID__', appInsights.id)
  }
}

// Outputs
output storageAccountName string = storageAccount.name
output appInsightsConnectionString string = appInsights.properties.ConnectionString
