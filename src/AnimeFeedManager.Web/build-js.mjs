import { transform } from '@swc/core';
import { glob } from 'glob';
import { promises as fs } from 'fs';
import path from 'path';
import { watch } from 'chokidar';

const isWatch = process.argv.includes('--watch');

// 📁 Configure your folders here
const FOLDERS_TO_PROCESS = [
    './wwwroot/scripts',
    './wwwroot/webcomponents'
    // Add more folders as needed
    // './wwwroot/another-folder',
];

// ⚙️ SWC Configuration
const config = {
    minify: true,
    sourceMaps: true,
    jsc: {
        minify: {
            compress: {
                drop_console: false,
                drop_debugger: true,
                unused: false,      // Don't remove "unused" functions
                dead_code: false,   // Don't remove "dead" code
                toplevel: false     // Don't assume file is a module
            },
            mangle: true,
            keep_fnames: false,     // Set to true if you need function names preserved
            toplevel: false         // Don't mangle top-level names
        },
        target: 'es2022'  // 🎯 Modern JavaScript! Supports all ES2022 features
    }
};

function formatFileSize(bytes) {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

async function minifyFile(filePath) {
    try {
        // Normalize path for consistency
        const normalizedPath = filePath.replace(/\\/g, '/');

        // Read the source file content
        const sourceCode = await fs.readFile(normalizedPath, 'utf8');

        if (!sourceCode || sourceCode.trim().length === 0) {
            console.log(`⚠️  Skipping empty file: ${normalizedPath}`);
            return;
        }

        // Get original file size
        const originalSize = Buffer.byteLength(sourceCode, 'utf8');

        // Transform using the source code directly
        const output = await transform(sourceCode, {
            ...config,
            filename: normalizedPath,
        });

        // Check if output has content
        if (!output.code) {
            console.error(`❌ No output generated for ${normalizedPath}`);
            return;
        }

        const minPath = normalizedPath.replace('.js', '.min.js');

        // Write the minified code
        await fs.writeFile(minPath, output.code, 'utf8');

        // Get minified file size
        const minSize = Buffer.byteLength(output.code, 'utf8');
        const savings = originalSize > 0 ? ((1 - minSize / originalSize) * 100).toFixed(1) : 0;

        if (output.map) {
            await fs.writeFile(minPath + '.map', output.map, 'utf8');
        }

        console.log(`✅ ${normalizedPath}`);
        console.log(`   📦 ${formatFileSize(originalSize)} → ${formatFileSize(minSize)} (${savings}% smaller)`);
        console.log(`   📍 ${minPath}`);

        // Debug: Show first 100 chars of output to verify content
        if (minSize < 100) {
            console.log(`   📝 Output: ${output.code}`);
        }
    } catch (error) {
        console.error(`❌ Error processing ${filePath}:`);
        console.error(`   ${error.message}`);
        if (error.stack) {
            console.error(`   Stack: ${error.stack}`);
        }
    }
}

async function buildAll() {
    console.log('🚀 Starting JavaScript minification...\n');

    // Build glob patterns from folders - match .js files directly in folders AND subdirectories
    const patterns = FOLDERS_TO_PROCESS.flatMap(folder => [
        `${folder}/*.js`,      // Files directly in folder
        `${folder}/**/*.js`    // Files in subdirectories
    ]);

    console.log('🔍 Looking for files with patterns:');
    patterns.forEach(p => console.log(`   - ${p}`));
    console.log('');

    const files = await glob(patterns, {
        ignore: ['**/*.min.js']
    });

    if (files.length === 0) {
        console.log('⚠️  No JavaScript files found to process');
        console.log('📍 Searched in:', FOLDERS_TO_PROCESS.join(', '));

        // Let's debug what files exist
        console.log('\n🔍 Checking what files exist:');
        for (const folder of FOLDERS_TO_PROCESS) {
            try {
                const allFiles = await fs.readdir(folder);
                const jsFiles = allFiles.filter(f => f.endsWith('.js'));
                console.log(`   ${folder}: ${jsFiles.length > 0 ? jsFiles.join(', ') : 'No .js files'}`);
            } catch (e) {
                console.log(`   ${folder}: Directory not found`);
            }
        }
        return;
    }

    console.log(`📊 Found ${files.length} file(s) to process:\n`);
    files.forEach(f => console.log(`   📄 ${f}`));
    console.log('');

    for (const file of files) {
        await minifyFile(file);
    }

    console.log('\n✨ Build complete!');
}

if (isWatch) {
    console.log('👀 Starting watch mode...\n');

    // Initial build
    await buildAll();

    // Build watch patterns - use absolute paths for better reliability
    const absolutePaths = FOLDERS_TO_PROCESS.map(folder =>
        path.resolve(folder).replace(/\\/g, '/')
    );

    console.log('\n🔍 Watching absolute paths:');
    absolutePaths.forEach(p => console.log(`   📁 ${p}`));

    // Watch for changes with more explicit configuration
    const watcher = watch(absolutePaths, {
        ignored: /(^|[\/\\])\..|(.*\.min\.js$)|(.*\.min\.js\.map$)/,
        persistent: true,
        ignoreInitial: true,
        awaitWriteFinish: {
            stabilityThreshold: 300,
            pollInterval: 100
        },
        // Use polling for better cross-platform compatibility
        usePolling: true,
        interval: 300
    });

    watcher
        .on('change', async (filePath) => {
            // Only process .js files (not .min.js)
            if (!filePath.endsWith('.js') || filePath.endsWith('.min.js')) {
                return;
            }

            const normalizedPath = filePath.replace(/\\/g, '/');
            console.log(`\n🔄 File changed: ${normalizedPath}`);
            await minifyFile(normalizedPath);
        })
        .on('add', async (filePath) => {
            if (!filePath.endsWith('.js') || filePath.endsWith('.min.js')) {
                return;
            }

            const normalizedPath = filePath.replace(/\\/g, '/');
            console.log(`\n➕ New file detected: ${normalizedPath}`);
            await minifyFile(normalizedPath);
        })
        .on('unlink', async (filePath) => {
            if (!filePath.endsWith('.js') || filePath.endsWith('.min.js')) {
                return;
            }

            const normalizedPath = filePath.replace(/\\/g, '/');
            const minPath = normalizedPath.replace('.js', '.min.js');
            const mapPath = minPath + '.map';

            try {
                await fs.access(minPath);
                await fs.unlink(minPath);
                console.log(`\n🗑️  Removed: ${minPath}`);

                try {
                    await fs.access(mapPath);
                    await fs.unlink(mapPath);
                    console.log(`🗑️  Removed: ${mapPath}`);
                } catch {}
            } catch {}
        })
        .on('ready', () => {
            console.log('\n✅ Watcher is ready!');
            console.log('📡 Monitoring for changes...');
            console.log('🛑 Press Ctrl+C to stop watching\n');
        })
        .on('error', error => console.error(`❌ Watcher error: ${error}`));

} else {
    await buildAll();
}