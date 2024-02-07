var sharedVariables = loadJsonContent('./shared-variables.json')

resource blazorAppService 'Microsoft.Web/sites@2023-01-01' existing = { 
  name: sharedVariables.webAppname
}

output appname string = blazorAppService.name

