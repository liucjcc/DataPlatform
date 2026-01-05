export function setToken(accessToken, refreshToken) {
    console.log("refresh_token:" + refreshToken);
    localStorage.setItem('access_token', accessToken);
    localStorage.setItem('refresh_token', refreshToken);
    localStorage.setItem('login_time', Date.now());
}

export function getAccessToken() {
    return localStorage.getItem('access_token');
}

export function getRefreshToken() {
    return localStorage.getItem('refresh_token');
}

export function clearToken() {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('login_time');
}

// 模拟 access token 是否过期（假设 30 秒过期）
export function isAccessTokenExpired() {
    return Date.now() - Number(localStorage.getItem('login_time') || 0) > 10 * 1000;
}