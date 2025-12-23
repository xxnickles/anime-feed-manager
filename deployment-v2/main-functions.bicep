// Reference file for Functions deployment workflow
// Used to retrieve the Function App name after infrastructure deployment

// Load shared variables
var config = loadJsonContent('modules/shared-variables.json')

// Output the function app name for deployment
output functionAppName string = config.functionAppName
