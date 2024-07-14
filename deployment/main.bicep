@description('Location for all resources.')
param location string = resourceGroup().location

@description('SendGrid Key')
param sendgridKey string

@description('Deafult Email Sender')
param email string

@description('Passworless Api Key')
@secure()
param passwordlessApiKey string

@description('Passworless Api Secret')
@secure()
param passwordlessApiSecret string

@description('Optional Git Repo URL')
param repoUrl string = ' '


module common './modules/common.bicep' = {
  name: 'common-deploy'
  params: {
    location: location
  }
}

module functions './modules/functions.bicep' = {
  name: 'functionDeploy'
  params: {
    location: location
    sendgridKey: sendgridKey
    email: email
    instrumentationKey: common.outputs.instrumentationKey
    storageAccountName: common.outputs.storageAccountName    
  }
}

module blazor './modules/blazor-app.bicep' = {
  name: 'blazorDeploy'
  params: {
    location: location  
    storageAccountName: common.outputs.storageAccountName    
    instrumentationKey: common.outputs.instrumentationKey
    passwordlessApiKey: passwordlessApiKey
    passwordlessApiSecret: passwordlessApiSecret
    repoUrl: repoUrl
    signalREnpoint: functions.outputs.signalREndpoint
  }
}
