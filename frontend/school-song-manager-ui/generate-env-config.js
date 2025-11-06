const fs = require('fs');
const path = require('path');

// Get environment variables
const apiUrl = process.env.API_URL || 'https://localhost:7159';
const nodeEnv = process.env.NODE_ENV || 'development';

// Create the environment configuration
const envConfig = {
  API_URL: apiUrl,
  NODE_ENV: nodeEnv
};

// Write to a file that can be loaded by the Angular app
const envConfigFile = `
(function (window) {
  window.__env = window.__env || {};
  window.__env.API_URL = '${envConfig.API_URL}';
  window.__env.NODE_ENV = '${envConfig.NODE_ENV}';
}(this));
`;

const outputPath = path.join(__dirname, 'public', 'assets', 'env-config.js');
// Ensure the assets directory exists
const assetsDir = path.join(__dirname, 'public', 'assets');
if (!fs.existsSync(assetsDir)) {
  fs.mkdirSync(assetsDir, { recursive: true });
}
fs.writeFileSync(outputPath, envConfigFile);

console.log('Environment configuration generated:', envConfig);