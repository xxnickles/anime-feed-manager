Simple title scrapper using puppeteer to collect image information from Erai raws, which is behind ddos-guard and image information from animechart.  Produces json files that are stored in an azure blob that will trigger other functions. Temporal while figure a reliable way to use pupputer in azure functions

## Usage
Modify appsettings in ScrapperConsole folowing this template 

```text
storageConnection: Connection string for blob storage
actionToExecute: action that is going to be executed. Valid avlues are images, titles, and all
```




