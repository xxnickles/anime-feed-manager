const { BlobServiceClient } = require('@azure/storage-blob');

// TODO: add correct type matadata
const storeOnBlob = (connectionString, containerName) => async (seasonInfo, jsonContent) => {
    const blobServiceClient = await BlobServiceClient.fromConnectionString(connectionString);
    const containerClient = await blobServiceClient.getContainerClient(containerName);
    if (!containerClient.exists())
        await containerClient.create();
    const blobName = `./images-${seasonInfo.season}-${seasonInfo.year}.json`;  
    const blockBlobClient = containerClient.getBlockBlobClient(blobName);
   
    const uploadBlobResponse = await blockBlobClient.upload(jsonContent, jsonContent.length)

    console.log("Blob was uploaded successfully. requestId: ", uploadBlobResponse.requestId);
}

module.exports.storeOnBlob = storeOnBlob;