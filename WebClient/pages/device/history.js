import { getHistoryData } from '../../api/device.js'
export function createPage() {

    function init(ctx) {
        loadData();
    }

    function refresh(ctx) {
        loadData();
    }

    function destroy() {

    };

    return { init, refresh, destroy };
}

function loadData(deviceId) {

    getHistoryData(deviceId).then(result => {

        if (result.success) {
            document.getElementById('deviceHistoryData').textContent = JSON.stringify(result.data, null, 2);
            renderDeviceData(document.getElementById('tableContainer'), result.data);
        }

    });

}

function renderDeviceData(container, data) {

    if (!data || !data.points || data.points.length === 0) {
        container.innerHTML = '<p>No data available</p>';
        return;
    }

    const table = document.createElement('table');
    table.className = 'device-realtime-table'; // 使用 CSS 类

    // 创建表头
    const thead = document.createElement('thead');
    const headerRow = document.createElement('tr');
    ['时间', '温度(°C)', '湿度(%)'].forEach(text => {
        const th = document.createElement('th');
        th.textContent = text;
        headerRow.appendChild(th);
    });
    thead.appendChild(headerRow);
    table.appendChild(thead);

    // 创建表体
    const tbody = document.createElement('tbody');

    data.points.forEach(point => {
        const tr = document.createElement('tr');

        // 时间
        const tdTime = document.createElement('td');
        tdTime.textContent = new Date(point.timestamp).toLocaleString();
        tr.appendChild(tdTime);

        // 温度
        const tdTemp = document.createElement('td');
        tdTemp.textContent = point.temperature.toFixed(2);
        tr.appendChild(tdTemp);

        // 湿度
        const tdHum = document.createElement('td');
        tdHum.textContent = point.humidity.toFixed(2);
        tr.appendChild(tdHum);

        tbody.appendChild(tr);
    });

    table.appendChild(tbody);

    // 清空容器并插入表格
    container.innerHTML = '';
    container.appendChild(table);
}
