// Reference file for Blazor Web App deployment workflow
// Used to retrieve the Web App name after infrastructure deployment

// Load shared variables
var config = loadJsonContent('modules/shared-variables.json')

// Output the web app name for deployment
output webAppName string = config.webAppName
