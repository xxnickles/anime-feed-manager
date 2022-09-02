@description('The name of the function app that you wish to create.')
param appName string = 'anime-manager'

@description('SendGrid Key')
param sendgridKey string

@description('Deafult Email Sender')
param email string

@description('Location for all resources.')
param location string = resourceGroup().location

var storageAccountType = 'Standard_LRS'
var functionAppName = appName
var hostingPlanName = 'afm-hosting-plan'
var applicationInsightsName = appName
var storageAccountName = 'animefeedmanagerstorage'
var functionWorkerRuntime = 'dotnet-isolated'


resource signalR 'Microsoft.SignalRService/signalR@2022-02-01' = {
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
    networkACLs: {
      defaultAction: 'Deny'
      publicNetwork: {
        allow: [
          'ServerConnection'
          'ClientConnection'
        ]
      }
      privateEndpoints: []
    }
    publicNetworkAccess: 'Enabled'
    disableAadAuth: false
    disableLocalAuth: false
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-09-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: storageAccountType
  }
  kind: 'StorageV2'
  properties: {
    supportsHttpsTrafficOnly: true
  }
}

resource hostingPlan 'Microsoft.Web/serverfarms@2021-03-01' = {
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

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
  }
}

resource functionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: hostingPlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'SignalRConnectionString'
          value: listKeys(signalR.id, signalR.apiVersion).primaryConnectionString
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
          value: applicationInsights.properties.InstrumentationKey
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
          value: 'Eastern Standard Time'
        }
      ]
      linuxFxVersion: 'DOTNET-ISOLATED|6.0'
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
