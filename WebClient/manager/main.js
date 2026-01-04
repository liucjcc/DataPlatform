//import { initRouter } from './router.js';
//initRouter();

import { initRouter } from './router.js';
import { login, me, logout } from '/api/auth.js';
import { getAccessToken, getRefreshToken } from '/utils/token.js';

export function showLoginModal() {

    document.getElementById("login-modal").classList.remove("hidden");

    document.getElementById("btn-login").onclick = async () => {
        const username = document.getElementById("login-username").value;
        const password = document.getElementById("login-password").value;

        try {
            const result = await login(username, password);
            
            if (result.success) {
                me().then(result => {
                    if (result.data.roles.includes('Admin')) {
                        location.reload();
                    } else {
                        document.getElementById("login-error").innerText = "不是管理员账号";
                    }
                });
            }
        } catch {
            document.getElementById("login-error").innerText = "登录失败";
        }
    };
}
function bootstrapAdmin() {

    if (getAccessToken() == null || getRefreshToken() == null) {
        showLoginModal();
        return;

    } else {
        me().then(result => {
            if (result.data.roles.includes('Admin')) {

            } else {
                alert('当前账号不是管理员，请重新登录!');
                showLoginModal();
                return;
            }
        });
    }

    const btnLogout = document.getElementById("btnLogout");

    btnLogout.addEventListener('click', function (e) {

        logout().then(result => {
            if (result.success) {
                showLoginModal();
            }
        });
    });
    initRouter();
}

bootstrapAdmin();

