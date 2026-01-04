import routes from '/core/routes.js';
import { initRouter } from '/core/router.js';
import { getMyDevices } from './api/device.js';
import { createPageScope } from '/core/createPageScope.js';
import { createComboBox } from '/components/comboBox/comboBox.js';

let scope = null;
let authorizedInited = false;

export function refreshSideBar() {

    if (!scope) return;

    const ul = document.getElementById('device-list');
    ul.innerHTML = '';

    getMyDevices().then(result => {
        if (result.success) {
            result.data.forEach(device => {
                const li = document.createElement('li');
                const link = document.createElement('a');
                link.href = `#/device/${device.id}/realtime`;
                link.textContent = `${device.name}`;
                link.dataset.deviceId = device.id;
                link.dataset.deviceName = device.name;
                link.className = 'nav-item';

                const clickHandler = (e) => {
                    const sidebar = document.getElementById('sidebar');
                    const links = sidebar.querySelectorAll('a');
                    links.forEach(el => el.classList.remove('active'));

                    e.currentTarget.classList.add('active');

                    const hashParts = location.hash.slice(1).split('/').filter(Boolean);
                    const currentSubpage = hashParts[2] || 'realtime';
                    location.hash = `#/device/${device.id}/${currentSubpage}`;
                };

                // 用 scope 管理事件
                scope.addListener(link, 'click', clickHandler);

                li.appendChild(link);
                ul.appendChild(li);
            });
        }
    });
}

function initSidebar() {

    const sidebar = document.getElementById('sidebar');
    const ul = document.getElementById('nav-list');
    ul.innerHTML = '';

    routes.forEach(route => {
        if (!route.isMenuItem) return;

        const li = document.createElement('li');
        const link = document.createElement('a');
        link.href = `#${route.path}`;
        link.className = 'nav-item';
        link.textContent = route.title;

        li.appendChild(link);
        ul.appendChild(li);

        const clickHandler = (e) => {
            const links = sidebar.querySelectorAll('a');
            links.forEach(el => el.classList.remove('active'));
            e.currentTarget.classList.add('active');
        };

        // 用 scope 管理事件
        scope.addListener(link, 'click', clickHandler);
    });

    //
    const comboBox = createComboBox({
        containerId: 'selectContainer',
        items: [
            { value: 'ClassA', text: '空气采样器' },
            { value: 'ClassB', text: '扬尘监测仪' },
            { value: 'ClassC', text: '大流量采样器' }
        ],
        onChange: (value) => {
            console.log('选择的类别:', value);
            // 这里可以触发 sidebar 过滤逻辑
        }
    });
}

export function initApp() {

    scope = createPageScope();
    initRouter();
} 

export async function initAuthorizedApp()
{
    if (authorizedInited) return;

    // ----------------------
    // 初始化导航栏
    // ----------------------
    initSidebar();

    // ----------------------
    // 加载设备列表
    // ----------------------
    refreshSideBar();

    authorizedInited = true;

}

// 页面销毁时统一调用
export function destroyApp() {

    scope?.destroy();
    scope = null;
    console.log("app destroy");

}
