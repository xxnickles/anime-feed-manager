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

var sharedVariables = loadJsonContent('./shared-variables.json')
var webAppHostingPlanName = 'afm-blazor-hosting'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
  scope: resourceGroup()
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
  name: sharedVariables.webAppname
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: blazorHostingPlan.id
    httpsOnly: true

  }
}

resource siteConfig 'Microsoft.Web/sites/config@2023-01-01' = {
  name: 'web'
  parent: blazorAppService
  properties: {
    appSettings: [
      {
        name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
        value: instrumentationKey
      }
      {
        name: 'StorageAccountName'
        value: storageAccountName
      }
      {
        name: 'Passwordless__ApiKey'
        value: passwordlessApiKey
      }
      {
        name: 'Passwordless__ApiSecret'
        value: passwordlessApiSecret
      }
    ]
    linuxFxVersion: 'DOTNETCORE|8.0'
  }
}

var roleDefinitionIds = [
  '974c5e8b-45b9-4653-ba55-5f855dd0fb88' //Storage Queue Data Contributor
  '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3' //Storage Table Data Contributor
  'ba92f5b4-2d11-453d-a403-e96b0029c9fe' //Storage Blob Data Contributor
]

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for roleDefinitionId in roleDefinitionIds: {
  scope: storageAccount
  name: guid(storageAccount.id, 'amf-blazor-identity', roleDefinitionId)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionId)
    principalId: blazorAppService.identity.principalId
  }
}]

resource webAppSourceControl 'Microsoft.Web/sites/sourcecontrols@2023-01-01' = if (contains(repoUrl, 'http')) {
  name: 'web'
  parent: blazorAppService
  properties: {
    repoUrl: repoUrl
    branch: 'main'
    isManualIntegration: true
  }
}
