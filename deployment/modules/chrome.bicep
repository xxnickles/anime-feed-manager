@description('Azure region for all resources')
param location string = resourceGroup().location

@description('App Service Plan ID to host the Chrome container')
param appServicePlanId string

@secure()
@description('Authentication token for browserless')
param chromeToken string

// Load shared variables
var config = loadJsonContent('shared-variables.json')

// Chrome Web App (browserless container)
resource chromeApp 'Microsoft.Web/sites@2023-12-01' = {
  name: config.chromeName
  location: location
  kind: 'app,linux,container'
  properties: {
    serverFarmId: appServicePlanId
    reserved: true
    siteConfig: {
      linuxFxVersion: 'DOCKER|ghcr.io/browserless/chromium:latest'
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      alwaysOn: true
      webSocketsEnabled: true
      appSettings: [
        {
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'false'
        }
        {
          name: 'TOKEN'
          value: chromeToken
        }
        {
          name: 'TIMEOUT'
          value: '120000'
        }
        {
          name: 'CONCURRENT'
          value: '5'
        }
        {
          name: 'MAX_QUEUE_LENGTH'
          value: '10'
        }
      ]
    }
    httpsOnly: true
  }
}

// Outputs
output chromeEndpoint string = 'https://${chromeApp.properties.defaultHostName}'
