import { getMyDevices } from '../../api/device';
import { getRealtimeData } from '../../api/device.js';
import { getHistoryData } from '../../api/device.js';
import { refreshCurrentPage } from '../../core/router.js';
import { createPageScope } from '/core/createPageScope.js'; // 假设你的 scope 工具在这里

export function createPage() {
    const scope = createPageScope();  // 创建页面 scope

    let tabButtons = null;
    let btnRefresh = null;

    function onTabClick(e) {
        const currentTab = e.currentTarget;
        const tabName = currentTab.dataset.tab;

        tabButtons.forEach(t => t.classList.remove('active'));
        currentTab.classList.add('active');

        const subpage = currentTab.dataset.subpage;

        const parts = location.hash.replace(/^#\/?/, '').split('/');
        const deviceId = parts[1];
        if (!deviceId) return;

        location.hash = `#/device/${deviceId}/${subpage}`;
    }

    function onBtnRefreshClick() {
        refreshCurrentPage();
    }

    function init(ctx) {
        if (ctx.pageTitle) {
            document.getElementById('pageTitle').textContent = ctx.pageTitle;
        }

        tabButtons = document.querySelectorAll('.tab-btn');
        if (tabButtons) {
            tabButtons.forEach(t => {
                t.classList.remove('active');
                scope.addListener(t, 'click', onTabClick);
            });

            // 根据子页面激活对应 tab
            switch (ctx.subpage) {
                case 'realtime':
                    tabButtons[0]?.classList.add('active');
                    break;
                case 'history':
                    tabButtons[1]?.classList.add('active');
                    break;
                case 'control':
                    tabButtons[2]?.classList.add('active');
                    break;
            }
        }

        btnRefresh = document.getElementById('btnRefresh');
        if (btnRefresh) {
            scope.addListener(btnRefresh, 'click', onBtnRefreshClick);
        }
    }

    function refresh(ctx) {
        if (ctx.pageParams.deviceId) {
            const deviceId = ctx.pageParams.deviceId;
            document.getElementById('deviceIdDisplay').textContent = deviceId;
        }
    }

    function destroy() {
        scope.destroy();
        tabButtons = null;
        btnRefresh = null;
    }

    return { init, refresh, destroy };
}
