const azureBlobStore = require('./azureBlobStore');
const anidbScrapper = require('./anidbScrapper');
const titlesScrapper = require('./titlesScrapper');
const witter = require('./fileStore');

// Composition Root

// Get from enviroment variables or default to values
var connectionString = process.env.STORAGE_CONNECTION || "UseDevelopmentStorage=true";
var imagesContainer = process.env.DEFAULT_IMAGES_CONTAINER || "images-process";
var titlesContainer = process.env.DEFAULT_TITLES_CONTAINER || "feed-titles-process";

const imagesStore = azureBlobStore.storeOnBlob(connectionString, imagesContainer)
const titlessStore = azureBlobStore.storeOnBlob(connectionString, titlesContainer)


// run2(writeFile);
// anidbScrapper.runAnidb(witter.writeFile);
// titlesScrapper.runTitles(witter.writeFile)

anidbScrapper.runAnidb(imagesStore);
titlesScrapper.runTitles(titlessStore);