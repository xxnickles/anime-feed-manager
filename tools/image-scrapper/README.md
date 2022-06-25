Simple scrapper using puppeteer to collect image information from Anime Chart. The produced json result is stored in an azure blob that will trigger other functions.

## Usage
Create a file called ".env" to set the environment variables following this template: 

```text
STORAGE_CONNECTION = "<storage-connection>"
DEFAULT_CONTAINER = "<container>"
```

In case there are no environmental variables, the script will default to the local emulator and point to the container "images-process"

## TODO
Move this script to a JS azure function. Not urgent due to the idea of this script is being run on-demand



