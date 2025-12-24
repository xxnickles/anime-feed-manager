@description('Azure region for all resources')
param location string = resourceGroup().location

@secure()
@description('Gmail sender email address')
param gmailFromEmail string

@secure()
@description('Gmail app password')
param gmailAppPassword string

@secure()
@description('Passwordless API key')
param passwordlessApiKey string

@secure()
@description('Passwordless API secret')
param passwordlessApiSecret string

@secure()
@description('Chrome authentication token for browserless')
param chromeToken string

@description('Gmail sender display name')
param gmailFromName string = 'Anime Feed Manager'

@description('Cron schedule for feed notifications (NCrontab 6-field format)')
param feedNotificationSchedule string = '0 0 * * * *'

@description('Cron schedule for library scraping (NCrontab 6-field format)')
param scrapingSchedule string = '0 0 0 * * *'

// Load shared variables for computed values
var config = loadJsonContent('modules/shared-variables.json')

// Deploy common resources (Storage Account, Application Insights)
module common 'modules/common.bicep' = {
  name: 'common'
  params: {
    location: location
  }
}

// Deploy Functions (Function App, SignalR, RBAC)
module functions 'modules/functions.bicep' = {
  name: 'functions'
  params: {
    location: location
    storageAccountName: common.outputs.storageAccountName
    appInsightsConnectionString: common.outputs.appInsightsConnectionString
    gmailFromEmail: gmailFromEmail
    gmailAppPassword: gmailAppPassword
    gmailFromName: gmailFromName
    feedNotificationSchedule: feedNotificationSchedule
    scrapingSchedule: scrapingSchedule
    // Chrome WebSocket endpoint (computed to avoid circular dependency)
    chromeEndpoint: 'wss://${config.chromeName}.azurewebsites.net/chromium'
    chromeToken: chromeToken
  }
}

// Deploy Blazor Web App (App Service, RBAC)
// Note: This creates the shared App Service Plan used by both Web and Chrome
module blazorApp 'modules/blazor-app.bicep' = {
  name: 'blazor-app'
  params: {
    location: location
    storageAccountName: common.outputs.storageAccountName
    appInsightsConnectionString: common.outputs.appInsightsConnectionString
    signalREndpoint: functions.outputs.signalREndpoint
    passwordlessApiKey: passwordlessApiKey
    passwordlessApiSecret: passwordlessApiSecret
  }
}

// Deploy Chrome Web App (browserless container for scraping)
module chrome 'modules/chrome.bicep' = {
  name: 'chrome'
  params: {
    location: location
    appServicePlanId: blazorApp.outputs.webPlanId
    chromeToken: chromeToken
  }
}

// Outputs
output storageAccountName string = common.outputs.storageAccountName
output functionAppName string = functions.outputs.functionAppName
output webAppName string = blazorApp.outputs.webAppName
output webAppUrl string = blazorApp.outputs.webAppUrl
output signalREndpoint string = functions.outputs.signalREndpoint
output chromeEndpoint string = chrome.outputs.chromeEndpoint
