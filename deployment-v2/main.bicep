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

@description('Gmail sender display name')
param gmailFromName string = 'Anime Feed Manager'

@description('Cron schedule for feed notifications (NCrontab 6-field format)')
param feedNotificationSchedule string = '0 0 * * * *'

@description('Cron schedule for library scraping (NCrontab 6-field format)')
param scrapingSchedule string = '0 0 0 * * *'

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
    storageAccountId: common.outputs.storageAccountId
    appInsightsConnectionString: common.outputs.appInsightsConnectionString
    gmailFromEmail: gmailFromEmail
    gmailAppPassword: gmailAppPassword
    gmailFromName: gmailFromName
    feedNotificationSchedule: feedNotificationSchedule
    scrapingSchedule: scrapingSchedule
  }
}

// Deploy Blazor Web App (App Service, RBAC)
module blazorApp 'modules/blazor-app.bicep' = {
  name: 'blazor-app'
  params: {
    location: location
    storageAccountName: common.outputs.storageAccountName
    storageAccountId: common.outputs.storageAccountId
    appInsightsConnectionString: common.outputs.appInsightsConnectionString
    signalREndpoint: functions.outputs.signalREndpoint
    passwordlessApiKey: passwordlessApiKey
    passwordlessApiSecret: passwordlessApiSecret
  }
}

// Outputs
output storageAccountName string = common.outputs.storageAccountName
output functionAppName string = functions.outputs.functionAppName
output webAppName string = blazorApp.outputs.webAppName
output webAppUrl string = blazorApp.outputs.webAppUrl
output signalREndpoint string = functions.outputs.signalREndpoint
