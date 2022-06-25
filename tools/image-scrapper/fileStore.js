const fs = require('fs');
// Helper funtion in case you want to create a local json file

const writeFile = (fileName, jsonContent) => {    
    fs.writeFile(`./${fileName}`, jsonContent, function (err) {
        if (err) {
            console.log(err);
        }
    });
}

module.exports.writeFile = writeFile;