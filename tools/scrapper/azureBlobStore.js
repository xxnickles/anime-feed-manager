const { BlobServiceClient, } = require('@azure/storage-blob');

// TODO: add correct type matadata
const storeOnBlob = (connectionString, containerName) => async (fileName, jsonContent) => {
    const blobServiceClient = BlobServiceClient.fromConnectionString(connectionString);
    const containerClient = blobServiceClient.getContainerClient(containerName);
    const containerExist = await containerClient.exists();
    if (!containerExist) {
        await containerClient.create();
    }

    const blobName = `${fileName}`;
    const blockBlobClient = containerClient.getBlockBlobClient(blobName);
    var options = {
        blobHTTPHeaders: { blobContentType: 'application/json' }
    };

    const uploadBlobResponse = await blockBlobClient.upload(jsonContent, jsonContent.length, options);

    console.log("Blob was uploaded successfully. requestId: ", uploadBlobResponse.requestId);
}

module.exports.storeOnBlob = storeOnBlob;