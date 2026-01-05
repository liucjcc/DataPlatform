import { getRealtimeData } from '../../api/device.js'
export function createPage() {

    function init(ctx) {
        loadData(ctx.pageParams.deviceId);
    }

    function refresh(ctx) {
        loadData(ctx.pageParams.deviceId);
    }

    function destroy() {
    };

    return { init, refresh, destroy };
}

function loadData(deviceId) {
    getRealtimeData(deviceId).then(result => {
        if (result.success) {
            document.getElementById('deviceRealtimeData').textContent = JSON.stringify(result.data, null, 2);
            renderDeviceData(document.getElementById('tableContainer'), result.data);
        }
    });
}
/**
 * 渲染 JSON 数据为键值表格，可自定义列数
 * @param {HTMLElement} container - 容器元素
 * @param {Object} data - JSON 对象
 * @param {number} columns - 每行显示几对键值（默认2列）
 */
function renderDeviceData(container, data, columns = 2) {
    const table = document.createElement('table');
    table.className = 'device-realtime-table'; // 设置 class

    const entries = Object.entries(data);
    const n = entries.length;
    const N = n % 2 === 0 ? n : n + 1;
    
    for (let i = 0; i < n; i += columns) {
        const tr = document.createElement('tr');
        for (let j = 0; j < columns; j++) {
            const item = entries[i + j];
            if (item) {
                const [key, value] = item;
                const tdLabel = document.createElement('td');
                tdLabel.textContent = key;
                tdLabel.style.fontWeight = 'bold';
                tr.appendChild(tdLabel);
                const tdValue = document.createElement('td');
                tdValue.textContent = value;
                tr.appendChild(tdValue);

            } else {
                const tdLabel = document.createElement('td');
                tr.appendChild(tdLabel);
                const tdValue = document.createElement('td');
                tr.appendChild(tdValue);
            }
        }

        table.appendChild(tr);
    }

    container.innerHTML = '';
    container.appendChild(table);
}

