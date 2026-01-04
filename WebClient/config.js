// config.js
// 开发环境与生产环境自动切换
const isDev = location.hostname === 'localhost';

export const API_BASE_URL = isDev
    ? 'https://localhost:7136'
    : 'https://your-production-domain.com';

export const WEBSOCKET_URL = isDev
    ? 'wss://localhost:7136'
    : 'wss://your-production-domain.com';