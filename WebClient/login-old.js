import { login } from "/api/auth.js";
export default function initLogin() {

    const loginBtn = document.getElementById('loginBtn');
    const errorMsg = document.getElementById('errMsg');

    if (!loginBtn) {
        console.error('loginBtn not found');
    }

    function loadCss() {
        if (cssLink) return;

        cssLink = document.createElement('link');
        cssLink.rel = 'stylesheet';
        cssLink.href = './login.css';
        cssLink.dataset.page = 'login';

        document.head.appendChild(cssLink);
    }
    function unloadCss() {
        if (cssLink) {
            cssLink.remove();
            cssLink = null;
        }
    }

    //loginBtn.addEventListener('click', async () => {
    //
    //});

    const clickHandler = async () => {
        const username = document.getElementById("username").value;
        const password = document.getElementById("password").value;
        try {
            const result = await login(username, password);
            if (result.success) {
                console.log("login success!");
                //window.location.hash = '/dashboard';
                const redirect = localStorage.getItem('redirect_after_login') || '/dashboard';
                localStorage.removeItem('redirect_after_login');
                console.log("redirect:" + redirect);
                window.location.replace('./index.html#' + redirect);
            }
        } catch (err) {
            if (errorMsg) {
                errorMsg.textContent = "登录失败：" + err.message;
            }
        }
    };

    function onUnload() {
        console.log("login unload");
        destroy();
    }

    loginBtn.addEventListener('click', clickHandler);

    // 使用 pagehide 更可靠（支持浏览器前进/后退）
    //window.addEventListener('pagehide', onUnload);

    // 或使用 beforeunload（兼容性好）
     window.addEventListener('beforeunload', onUnload);

    loadCss();

    return function destroy() {
        loginBtn.removeEventListener('click', clickHandler);
        unloadCss();
    };
}




