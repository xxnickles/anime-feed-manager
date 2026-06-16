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

const updated = [];
const alreadyUpToDate = [];
const errors = [];

libmanConfig.libraries.forEach((lib, index) => {
    const libraryName = lib.library || lib;

    // Remove version information (everything after the LAST @)
    const lastAtIndex = libraryName.lastIndexOf('@');
    const libraryNameWithoutVersion = lastAtIndex !== -1
        ? libraryName.substring(0, lastAtIndex)
        : libraryName;

    console.log(`[${index + 1}/${libmanConfig.libraries.length}] Updating ${libraryNameWithoutVersion}...`);

    try {
        const output = execSync(`libman update ${libraryNameWithoutVersion}`, { encoding: 'utf8' });
        
        // Check if library was already up to date (case-insensitive)
        if (/up to date/i.test(output)) {
            alreadyUpToDate.push(libraryNameWithoutVersion);
        } else {
            updated.push(libraryNameWithoutVersion);
        }
    } catch (error) {
        console.error(`❌ Error updating ${libraryNameWithoutVersion}`);
        errors.push(libraryNameWithoutVersion);
    }
});

console.log('\n✅ Update complete!\n');

if (updated.length > 0) {
    console.log(`📦 Updated (${updated.length}):`);
    updated.forEach(lib => console.log(`   - ${lib}`));
    console.log('');
}

if (alreadyUpToDate.length > 0) {
    console.log(`✓ Already up-to-date (${alreadyUpToDate.length}):`);
    alreadyUpToDate.forEach(lib => console.log(`   - ${lib}`));
    console.log('');
}

if (errors.length > 0) {
    console.log(`❌ Errors (${errors.length}):`);
    errors.forEach(lib => console.log(`   - ${lib}`));
}