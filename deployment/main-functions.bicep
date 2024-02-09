var sharedVariables = loadJsonContent('./modules/shared-variables.json')
resource functionApp 'Microsoft.Web/sites@2023-01-01' existing = {
  name: sharedVariables.functionAppName
}

output endpoint string = '${functionApp.name}.azurewebsites.net'
output appname string = functionApp.name
