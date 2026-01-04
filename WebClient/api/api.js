export const API = {
    DEVICE: {
        MY_DEVICES: '/api/Device/devices',
        REALTIME_DATA: (deviceId) => `/api/Device/${deviceId}/realtimeData`,
        HISTORY_DATA: (deviceId) => `/api/Device/${deviceId}/historyData`,
        UPDATE_STATUS: (deviceId) => `/api/Device/${deviceId}/status`,
        DELETE: (deviceId) => `/api/Device/${deviceId}`,
    },
    AUTH: {
        LOGIN: '/api/Auth/login',
        REFRESH: '/api/Auth/refresh',
    },
    USER: {
        ME: '/api/Auth/me',
        UPDATE_PROFILE: '/api/User/updateProfile',
        LIST: '/api/User/list',
    },
};
