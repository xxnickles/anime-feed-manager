@description('Location for all resources.')
param location string = resourceGroup().location

@description('SendGrid Key')
param sendgridKey string

@description('Deafult Email Sender')
param email string

module common './common.bicep' = {
  name: 'common-deploy'
  params: {
    location: location
  }
}

module functions './functions.bicep' = {
  name: 'functionDeploy'
  params: {
    location: location
    sendgridKey: sendgridKey
    email: email
    instrumentationKey: common.outputs.instrumentationKey
    storageAccountName: common.outputs.storageAccountName    
  }
}

output endpoint string = functions.outputs.endpoint
output appname string = functions.outputs.appname
