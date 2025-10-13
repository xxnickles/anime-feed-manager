const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

// Read libman.json
const libmanPath = path.join(__dirname, 'libman.json');

if (!fs.existsSync(libmanPath)) {
    console.error('❌ libman.json not found!');
    process.exit(1);
}

console.log('📚 Reading libman.json...');
const libmanConfig = JSON.parse(fs.readFileSync(libmanPath, 'utf8'));

if (!libmanConfig.libraries || libmanConfig.libraries.length === 0) {
    console.log('ℹ️  No libraries found in libman.json');
    process.exit(0);
}

console.log(`\n🔄 Updating ${libmanConfig.libraries.length} libraries...\n`);

let successCount = 0;
let errorCount = 0;

libmanConfig.libraries.forEach((lib, index) => {
    const libraryName = lib.library || lib;

    // Remove version information (everything after the LAST @)
    const lastAtIndex = libraryName.lastIndexOf('@');
    const libraryNameWithoutVersion = lastAtIndex !== -1
        ? libraryName.substring(0, lastAtIndex)
        : libraryName;

    console.log(`[${index + 1}/${libmanConfig.libraries.length}] Updating ${libraryNameWithoutVersion}...`);

    try {
        execSync(`libman update ${libraryNameWithoutVersion}`, { stdio: 'inherit' });
        successCount++;
    } catch (error) {
        console.error(`❌ Error updating ${libraryNameWithoutVersion}`);
        errorCount++;
    }
});

console.log('\n✅ Update complete!');
console.log(`   Success: ${successCount}`);
if (errorCount > 0) {
    console.log(`   Errors: ${errorCount}`);
}