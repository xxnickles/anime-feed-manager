@description('The name of the function app that you wish to create.')
param appName string = 'anime-manager'

@description('SendGrid Key')
param sendgridKey string

@description('Default Email Sender')
param email string

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Storage Account Name')
param storageAccountName string

@description('Instrumentation Key')
param instrumentationKey string



var functionAppName = appName
var hostingPlanName = 'afm-hosting-plan'
var functionWorkerRuntime = 'dotnet-isolated'


resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
  scope: resourceGroup()
}


resource signalR 'Microsoft.SignalRService/signalR@2023-02-01' = {
  name: 'afm-web-events'
  location: location
  sku: {
    capacity: 1
    name: 'Free_F1'
  }
  kind: 'SignalR'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    tls: {
      clientCertEnabled: false
    }
    features: [
      {
        flag: 'ServiceMode'
        value: 'Serverless'
      }
      {
        flag: 'EnableConnectivityLogs'
        value: string(true)
      }
      {
        flag: 'EnableMessagingLogs'
        value: string(true)
      }
      {
        flag: 'EnableLiveTrace'
        value: string(true)
      }
    ]
    cors: {
      allowedOrigins: [ 'https://delightful-smoke-0eded0c0f.1.azurestaticapps.net' ]
    }

    publicNetworkAccess: 'Enabled'
    disableAadAuth: false
    disableLocalAuth: false
  }
}

// Functions App

resource functionsHostingPlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: hostingPlanName
  location: location
  kind: 'functionapp,linux'
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {
    reserved: true
  }
}

resource functionApp 'Microsoft.Web/sites@2023-01-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: functionsHostingPlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'SignalRConnectionString'
          value: signalR.listKeys().primaryConnectionString
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: toLower(functionAppName)
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: instrumentationKey
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: functionWorkerRuntime
        }
        {
          name: 'Sandbox'
          value: 'false'
        }
        {
          name: 'SendGridKey'
          value: sendgridKey
        }
        {
          name: 'FromEmail'
          value: email
        }
        {
          name: 'FromName'
          value: 'Anime Feed Manager'
        }
        {
          name: 'WEBSITE_TIME_ZONE'
          value: 'America/New_York'
        }
      ]
      linuxFxVersion: 'DOTNET-ISOLATED|8.0'
      use32BitWorkerProcess: false
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
    }
    clientAffinityEnabled: false
    httpsOnly: true
  }
}

output endpoint string = '${functionApp.name}.azurewebsites.net'
output appname string = functionApp.name
