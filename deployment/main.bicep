@description('Location for all resources.')
param location string = resourceGroup().location

@description('SendGrid Key')
param sendgridKey string

module functions './functions.bicep' = {
  name: 'functionDeploy'
  params: {
    location: location
    sendgridKey: sendgridKey
  }
}

output endpoint string = functions.outputs.endpoint
output appname string = functions.outputs.appname
