import { login } from "/api/auth.js";
import { createPageScope } from "/core/createPageScope.js"; // 假设 scope 工具在这里

export function createPage() {
    const scope = createPageScope();  // 创建页面 scope

    let loginBtn;
    let errorMsg;

    async function onLoginClick() {
        const username = document.getElementById('username')?.value;
        const password = document.getElementById('password')?.value;

        try {
            const result = await login(username, password);
            if (result.success) {
                const redirect = localStorage.getItem('redirect_after_login') || '/dashboard';
                localStorage.removeItem('redirect_after_login');
                location.hash = redirect;
            }
        } catch (err) {
            if (errorMsg) {
                errorMsg.textContent = '登录失败：' + err.message;
            }
        }
    }

    function init(ctx) {
        loginBtn = document.getElementById('loginBtn');
        errorMsg = document.getElementById('errMsg');

        if (!loginBtn) {
            console.error('loginBtn not found');
            return;
        }

        // 使用 scope 管理事件
        scope.addListener(loginBtn, 'click', onLoginClick);
    }

    function refresh(ctx) {
        // 可选：刷新逻辑
    }

    function destroy() {
        // 自动清理 scope 中注册的事件
        scope.destroy();
        console.log("aaaaaaaaaaaaaaaaaaaaaa");
        // 清理引用，帮助 GC
        loginBtn = null;
        errorMsg = null;
    }

    return { init, refresh, destroy };
}
