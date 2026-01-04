// manager.js
// 管理端入口逻辑

const API_BASE = "http://localhost:5000/api/admin"; // 替换成你的 WebAPI 地址
const logEl = document.getElementById("log");

// 获取 token（假设存 localStorage）
function getToken() {
    return localStorage.getItem("access-token") || "";
}

// 简单日志
function log(msg, isError = false) {
    logEl.textContent = msg;
    logEl.className = isError ? "error" : "log";
}

// 添加设备按钮事件
document.getElementById("addDeviceBtn").addEventListener("click", async () => {
    const name = document.getElementById("deviceName").value.trim();
    const id = document.getElementById("deviceId").value.trim();

    if (!name || !id) {
        log("请填写设备名称和ID", true);
        return;
    }

    try {
        const resp = await fetch(`${API_BASE}/devices`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${getToken()}`
            },
            body: JSON.stringify({ name, deviceId: id })
        });

        if (!resp.ok) {
            const text = await resp.text();
            throw new Error(text || resp.statusText);
        }

        log(`✅ 设备 "${name}" 添加成功`);
    } catch (err) {
        log(`❌ 添加失败: ${err.message}`, true);
    }
});
