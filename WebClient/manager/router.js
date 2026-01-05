import { routes } from './routes.js';

import { initDeviceDefaultPage, loadDeviceConfigPage } from '/manager/device/device.js';
import { loadModbusConfigPage } from '/manager/device/modbus.js';
import { initUserDefaultPage, initUserConfigPage} from '/manager/user/user.js';

// 获取 app 容器
const app = document.getElementById('app');

function parseModeFromPath(path) {
    if (path.endsWith('/add')) return 'add';
    if (path.endsWith('/edit')) return 'edit';
    if (path.endsWith('/view')) return 'view';
    return 'view'; // 默认
}
// 路由函数
export async function router() {
    const hash = location.hash || '#/device';
    const [path, query] = hash.substring(1).split('?');
    // 匹配最长的 route，防止 /device/add 被 /device 先匹配
    const route = Object.keys(routes)
        .sort((a, b) => b.length - a.length)
        .find(r => path.startsWith(r));
    if (!route) {
        app.innerHTML = '<h2>404</h2>';
        return;
    }

    let url = routes[route];
    if (query) {
        url += (url.includes('?') ? '&' : '?') + query;
    }

    try {
        const res = await fetch(url);
        if (!res.ok) {
            app.innerHTML = `<h2>加载失败: ${res.status}</h2>`;
            return;
        }
        app.innerHTML = await res.text();
    } catch (err) {
        console.error(err);
        app.innerHTML = `<h2>加载异常</h2>`;
        return;
    }

    // 解析 query 参数
    const params = {};
    if (query) {
        query.split('&').forEach(pair => {
            const [k, v] = pair.split('=');
            params[k] = decodeURIComponent(v || '');
        });
    }

    const mode = parseModeFromPath(path);

    // 调用对应 JS 初始化函数
    if (url.includes('/manager/device/config.html')) {
        console.log('params:', params);
        loadDeviceConfigPage(mode, params.deviceId);
    }
    else if (url.includes('/manager/device/modbus.html')) {
        console.log('params:', params);
        loadModbusConfigPage();
    }
    else if (url.includes('/manager/device/default.html')) {

        initDeviceDefaultPage();
    }
    else if (url.includes('/manager/user/config.html')) {

        initUserConfigPage(mode);

    } else if (url.includes('/manager/user/user.html')) {

        initUserDefaultPage();
    }
}

// 初始化路由
export function initRouter() {
    window.addEventListener('hashchange', router);
    router(); // 页面首次加载时执行
}

