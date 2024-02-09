@description('Location for all resources.')
param location string = resourceGroup().location

var applicationInsightsName  = 'anime-manager'
var storageAccountName = 'animefeedmanagerstorage'
var storageAccountType = 'Standard_LRS'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
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

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
  }
}


output instrumentationKey string = applicationInsights.properties.InstrumentationKey
output storageAccountName string = storageAccount.name
