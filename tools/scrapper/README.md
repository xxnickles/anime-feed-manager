Simple scrapper using puppeteer to collect image information from Anime Chart. The produced json result is stored in an azure blob that will trigger other functions.

## Usage
Create a file called ".env" to set the environment variables following this template: 

```text
STORAGE_CONNECTION = "<storage-connection>"
DEFAULT_CONTAINER = "<container>"
```

In case there are no environmental variables, the script will default to the local emulator, point to the images' container "images-process", and the titles' container "feed-titles-process" 




