import { getAccessToken, setToken, clearToken } from '/utils/token.js';
/*import { addDevice } from '/api/admin/device.js';*/

// 页面初始化
let currentMode = 'view'; // 模块作用域全局变量
export function initUserConfigPage(mode = 'view') {

    currentMode = mode;

    const deviceIdInput = document.getElementById("deviceId");
    const nameInput = document.getElementById("deviceName");
    const btnSave = document.getElementById("btnSave");
}

document.getElementById('app').addEventListener('click', async (e) => {

    if (e.target.id !== 'btnSave') return;

    const idInput = document.getElementById("deviceId");
    const nameInput = document.getElementById("deviceName");
    if (!idInput || !nameInput) return;

    const device = {
        id: idInput.value.trim(),
        name: nameInput.value.trim()
    };

    if (!device.id || !device.name) {
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
            const res = await addDevice(device, token);
            if (res.ok) {
                alert("保存成功");
                location.hash = "#/device"; // 回列表
            } else {
                const errText = await res.text();
                alert("保存失败: " + errText);
            }
        } else if (currentMode === 'edit') {
            // TODO: 调用编辑接口
            alert("编辑功能待实现");
        }
    } catch (err) {
        console.error(err);
        alert("保存异常: " + err.message);
    }
});

export function initUserDefaultPage() {
    console.log("loadUserList");
};