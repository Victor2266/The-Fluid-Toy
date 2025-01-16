const http = require('http');
const fs = require('fs');
const path = require('path');
const zlib = require('zlib');

const server = http.createServer((req, res) => {
    // Handle the root path
    let filePath = req.url === '/' ? '/index.html' : req.url;
    filePath = path.join(__dirname, 'build', filePath);

    // Get the file extension
    const extname = path.extname(filePath);

    // Define MIME types for Unity WebGL files
    const mimeTypes = {
        '.html': 'text/html',
        '.js': 'text/javascript',
        '.data': 'application/octet-stream',
        '.wasm': 'application/wasm',
        '.json': 'application/json',
        '.css': 'text/css',
        '.br': 'application/brotli'
    };

    // Check if browser supports brotli
    const acceptEncoding = req.headers['accept-encoding'] || '';
    const supportsBrotli = acceptEncoding.includes('br');

    // If the file doesn't exist, try with .br extension
    if (!fs.existsSync(filePath) && supportsBrotli) {
        if (fs.existsSync(filePath + '.br')) {
            filePath += '.br';
        }
    }

    // Get the base file extension (before .br if it exists)
    const baseExtname = path.extname(filePath.replace('.br', ''));
    const contentType = mimeTypes[baseExtname] || 'application/octet-stream';

    // Read and serve the file
    fs.readFile(filePath, (error, content) => {
        if (error) {
            if (error.code === 'ENOENT') {
                res.writeHead(404);
                res.end('File not found');
            } else {
                res.writeHead(500);
                res.end('Server error: ' + error.code);
            }
            return;
        }

        const headers = {
            'Content-Type': contentType,
            'Cross-Origin-Embedder-Policy': 'require-corp',
            'Cross-Origin-Opener-Policy': 'same-origin'
        };

        // If serving a .br file, add the content-encoding header
        if (filePath.endsWith('.br')) {
            headers['Content-Encoding'] = 'br';
        }

        res.writeHead(200, headers);
        res.end(content);
    });
});

const port = 3000;
server.listen(port, () => {
    console.log(`Server running at http://localhost:${port}`);
});
