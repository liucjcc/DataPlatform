
import { getAccessToken, setToken, clearToken } from '/utils/token.js';
import { getDeviceList, getMyDevices,  getDeviceById, addDevice, updateDevice,  deleteDevice, getLatestData } from '/api/device.js';
import { JsonModal } from '/components/jsonModal/jsonModal.js';
import '/components/jsonModal/jsonModal.css';

let currentMode = 'view';
const jsonModal = new JsonModal();

export function loadDeviceConfigPage(mode = 'view', deviceId = null) {

    currentMode = mode;

    const btnSave = document.getElementById("btnSave");
    const idInput = document.getElementById("deviceId");
    const nameInput = document.getElementById("deviceName");
    if (!idInput || !nameInput) return;

    if (mode === 'edit') {

        loadDeviceById(deviceId)

        document.getElementById('tableTitle').innerText = "编辑设备配置信息";

    } else if (mode === 'view') {

        loadDeviceById(deviceId)

    } else if (mode === 'add') {

        document.getElementById('tableTitle').innerText = "添加设备";

    }
}

document.getElementById('app').addEventListener('click', async (e) => {

    if (e.target.id !== 'btnSave') return;

    const idInput = document.getElementById("deviceId");
    const nameInput = document.getElementById("deviceName");
    if (!idInput || !nameInput) return;

    const device = {
        deviceId: idInput.value.trim(),
        deviceName: nameInput.value.trim()
    };

    if (!device.deviceId || !device.deviceName) {
        alert("设备ID和名称不能为空");
        return;
    }

    const token = getAccessToken();
    if (!token) {
        alert("请先登录");
        return;
    }

    try {
        if (currentMode === 'add') {
            const res = await addDevice(device);
            if (res.success) {
                alert("保存成功");
                location.hash = "#/device";
            }
            else {
                const errText = await res.data;
                alert("保存失败: " + errText);
            }
        } else if (currentMode === 'edit') {
            const res = await updateDevice(device);
            if (res.success) {
                alert("保存成功");
                location.hash = "#/device";
            }
            else {
                const errText = await res.data;
                alert("保存失败: " + errText);
            }
        }
    } catch (err) {
        console.error(err);
        alert("保存异常: " + err.message);
    }
});

export function initDeviceDefaultPage() {
    loadDeviceList();
};

function loadDeviceList() {
    getDeviceList().then(result => {
        if (result.success) {
            renderDeviceTable(document.getElementById('tableContainer'), result.data);
        }
    });
}

function loadDeviceById(deviceId) {

    const deviceIdInput = document.getElementById("deviceId");
    const deviceNameInput = document.getElementById("deviceName");
    if (!deviceIdInput || !deviceNameInput) return;

    getDeviceById(deviceId).then(result => {
        if (result.success) {
            deviceIdInput.value = result.data.deviceId;
            deviceNameInput.value = result.data.deviceName;
        }
    });
}

function renderDeviceTable(container, data, columns = 4) {
    // 如果传入的是 ID，获取元素
    if (typeof container === 'string') {
        container = document.getElementById(container);
    }
    if (!container) return;

    // 清空容器
    container.innerHTML = '';

    // 检查 data 是否为数组
    if (!Array.isArray(data)) {
        container.innerHTML = '<p style="color:red;">设备数据格式错误</p>';
        console.error('renderDeviceTable: data 不是数组', data);
        return;
    }

    // 创建表格
    const table = document.createElement('table');
    table.className = 'data-table';

    // 表头
    const thead = document.createElement('thead');
    const headerRow = document.createElement('tr');

    const headers = ['设备ID', '点位名称', '上次在线', '状态', '操作'];
    headers.forEach(text => {
        const th = document.createElement('th');
        th.innerText = text;
        th.style.border = '1px solid #ccc';
        th.style.padding = '8px';
        th.style.backgroundColor = '#f0f0f0';
        headerRow.appendChild(th);
    });
    thead.appendChild(headerRow);
    table.appendChild(thead);

    // 表格主体
    const tbody = document.createElement('tbody');
    data.forEach(device => {
        const row = document.createElement('tr');

        // 数据列
        const cells = [
            device.deviceId,
            device.deviceName,
            new Date(device.lastOnlineTime).toLocaleString(),
            device.status ? '在线' : '离线'
        ];
        cells.forEach(text => {
            const td = document.createElement('td');
            td.innerText = text;
            td.style.textAlign = 'center';
            row.appendChild(td);
        });

        // 操作列
        const opTd = document.createElement('td');
        opTd.style.textAlign = 'center';

        // 编辑按钮
        const editBtn = document.createElement('button');
        editBtn.innerText = '编辑';
        editBtn.style.marginRight = '5px';
        editBtn.onclick = () => {
            onEditDevice(device.deviceId);
        };

        // 删除按钮
        const delBtn = document.createElement('button');
        delBtn.innerText = '删除';
        delBtn.onclick = () => {
            onDeleteDevice(device.deviceId);
        };

        // 最后数据
        const latestBtn = document.createElement('button');
        latestBtn.innerText = '最后数据';
        latestBtn.onclick = () => {
            onLatestData(device.deviceId);
        };
        // 通讯协议
        const modbusBtn = document.createElement('button');
        modbusBtn.innerText = '通讯协议';
        modbusBtn.onclick = () => {
            onModbusConfig(device.deviceId);
        };

        opTd.appendChild(editBtn);
        opTd.appendChild(delBtn);
        opTd.appendChild(latestBtn);
        opTd.appendChild(modbusBtn);

        row.appendChild(opTd);

        tbody.appendChild(row);
    });

    table.appendChild(tbody);
    container.appendChild(table);
}

function onEditDevice(deviceId) {
    location.hash = `#/device/edit?deviceId=${deviceId}`;
}

function onDeleteDevice(deviceId) {

    deleteDevice(deviceId).then(result => {

        if (result.success) {

            loadDeviceList();

        } else {

            alert('删除失败: ' + result.error);
        }
    });
}

function showJsonData(data) {
    const container = document.getElementById("jsoneditor");
    const editor = new JSONEditor(container, {
        mode: 'view',      // 只读模式
        mainMenuBar: false, // 不显示顶部菜单
        navigationBar: true, // 显示树状导航
        statusBar: false
    });
    editor.set(data);
}

function onLatestData(deviceId) {
    getLatestData(deviceId).then(result => {
        if (result.success) {
            try {
                const jsonData = result.data;
                jsonData.payload = JSON.parse(jsonData.payload);
                jsonModal.show(jsonData);
            } catch (e) {
                jsonModal.show(result.data);
            }
        } else {
            alert('查询失败: ' + result.error);
        }
    });
}

function onModbusConfig(deviceId) {
    location.hash = `#/device/modbus?deviceId=${deviceId}`;
}

