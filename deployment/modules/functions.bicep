@description('Default Email Sender')
param email string

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Storage Account Name')
param storageAccountName string

@description('Instrumentation Key')
param instrumentationKey string

var sharedVariables = loadJsonContent('./shared-variables.json')

var hostingPlanName = 'afm-hosting-plan'
var functionWorkerRuntime = 'dotnet-isolated'
var webSiteUrl = 'https://anime-feed-manager.azurewebsites.net'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
  scope: resourceGroup()
}

resource comunicationService 'Microsoft.Communication/CommunicationServices@2024-09-01-preview' existing = {
  name: sharedVariables.communicationServiceName
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
      allowedOrigins: [webSiteUrl]
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
  name: sharedVariables.functionAppName
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
          value: toLower(sharedVariables.functionAppName)
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
          name: 'FromEmail'
          value: email
        }      
        {
          name: 'RunHeadless'
          value: 'true'
        }
        {
          name: 'CommunicationServiceConnectionString'
          value: comunicationService.listKeys().primaryConnectionString
        }        
      ]
      cors: {
        allowedOrigins: [webSiteUrl]
        supportCredentials: true
      }
      linuxFxVersion: 'DOTNET-ISOLATED|9.0'
      use32BitWorkerProcess: false
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
    }
    clientAffinityEnabled: false
    httpsOnly: true
  }
}


output signalREndpoint string = 'https://${functionApp.properties.defaultHostName}/api'
