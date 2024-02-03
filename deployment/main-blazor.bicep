@description('Location for all resources.')
param location string = resourceGroup().location


@description('Passworless Api Key')
@secure()
param passwordlessApiKey string

@description('Passworless Api Secret')
@secure()
param passwordlessApiSecret string

@description('Optional Git Repo URL')
param repoUrl string = ' '


module common './common.bicep' = {
  name: 'common-deploy'
  params: {
    location: location
  }
}

module blazor './blazor-app.bicep' = {
  name: 'functionDeploy'
  params: {
    location: location  
    storageAccountName: common.outputs.storageAccountName    
    instrumentationKey: common.outputs.instrumentationKey
    passwordlessApiKey: passwordlessApiKey
    passwordlessApiSecret: passwordlessApiSecret
    repoUrl: repoUrl
  }
}

output appname string = blazor.outputs.appname

