
import { API_BASE_URL } from '/config.js';
import { getAccessToken, setToken, isAccessTokenExpired, getRefreshToken, clearToken } from '../utils/token.js';
import { request } from './request.js';

// 登录
export async function login(username, password) {
    const payload = { username: username, password: password };
    const res = await fetch(API_BASE_URL + "/api/Auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
    });

    if (!res.ok) throw new Error("Login failed");

    const data = await res.json();

    if (data.accessToken && data.refreshToken) {

        setToken(data.accessToken, data.refreshToken);

        var expire = new Date(data.expiresAt).getTime();
        localStorage.setItem("token_expire", expire);

        return { success: true };

    } else {

        throw new Error("Login response missing tokens");
    }
}

export async function me() {
    console.log("me():", getAccessToken());
    const result = await request.get('/api/Auth/me');
    console.log("me:", result);
    return result;
}

/**
 * 使用 refreshToken 向后端请求新的 accessToken
 * @param {string} refreshToken - 当前有效的 refreshToken 字符串
 * @returns {Promise<{ accessToken: string, expiresIn: number } | null>} 新的 token 信息
 */
export async function refreshAccessToken() {
    const refreshToken = localStorage.getItem('refresh_token');
    if (!refreshToken) throw new Error('No refresh token');
    const res = await fetch(API_BASE_URL + '/api/Auth/refresh', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(
            { RefreshToken: refreshToken }
        )
    });
    if (!res.ok) throw new Error('Refresh failed');

    const data = await res.json();
    console.log(data);
    return data.accessToken;
}

// ----------------------
// 退出登录
// ----------------------
export async function logout() {
    const token = getRefreshToken();
    const result = await request.post("/api/auth/logout", { RefreshToken: token });
    if (result.success) {
        clearToken();
    }
    return result;
}

export function authGuard() {
    const token = getAccessToken();
    if (!token) {
        return false;
    }
    if (isAccessTokenExpired()) {
        return false;
    }
    return true;
}

            