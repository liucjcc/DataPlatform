import { API_BASE_URL } from '/config.js';
import { getAccessToken, setToken, clearToken } from '../utils/token.js';
import { refreshAccessToken } from './auth.js'

/**
 * @param {string} url API 路径
 * @param {object} options fetch 配置
 * @param {boolean} retry 401 自动重试
 */
async function requestCore(url, options = {}, retry = true) {

    const token = getAccessToken();

    const headers = {
        'Content-Type': 'application/json',
        ...options.headers,
        ...(token ? { 'Authorization': `Bearer ${token}` } : {})
    };

    try {
        const res = await fetch(API_BASE_URL + url, { ...options, headers });

        // 刷新token或跳转登录
        if (res.status === 401 && retry) {
            console.log("handle401");
            return handle401(url, options);
        }

        // 非2xx状态码
        if (!res.ok) {
            const errorText = await res.text();
            // 直接返回统一的 error 格式
            return { success: false, data: null, error: `HTTP ${res.status}: ${errorText}` };
        }

        // 必须返回json数据，尝试解析 JSON
        let result;
        try {
            result = await res.json();
        } catch {
            // 非JSON格式数据
            result = { success: false, data: null, error: await res.text() };
        }

        // 后台没有返回 success 字段，默认加上
        if (typeof result.success === 'undefined') {
            result = { success: true, data: result, error: null };
        }

        // 后台返回 success = false，直接返回不抛异常
        return result;

    } catch (err) {

        console.error('Request failed:', err);

        // 网络异常或 fetch 报错，也返回统一格式
        return { success: false, data: null, error: err.message };
    }
}

/**
 * 处理 401 错误
 */
async function handle401(url, options) {
    try {

        const newToken = await refreshAccessToken();
        setToken(newToken);
        return requestCore(url, options, false); // 重试一次

    } catch (err) {

        clearToken();

        // 可以弹出登录框或跳转登录页
        // window.location.href = '/login.html';
        throw new Error('LOGIN_REQUIRED');
    }
}

export const request = {
    async get(url, headers = {}) {
        return requestCore(url, { method: 'GET', headers });
    },

    async post(url, data, headers = {}) {

        console.log(JSON.stringify(data));
        return requestCore(url, {
            method: 'POST',
            headers,
            body: JSON.stringify(data)
        });
    },

    async put(url, data, headers = {}) {
        return requestCore(url, {
            method: 'PUT',
            headers,
            body: JSON.stringify(data)
        });
    },

    async patch(url, data, headers = {}) {
        return requestCore(url, {
            method: 'PATCH',
            headers,
            body: JSON.stringify(data)
        });
    },

    async delete(url, headers = {}) {
        return requestCore(url, {
            method: 'DELETE',
            headers
        });
    }
};
