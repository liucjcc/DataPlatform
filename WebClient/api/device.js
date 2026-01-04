import { getAccessToken, setToken, clearToken } from '/utils/token.js';
import { request } from './request.js';

/**
 * 通用 API 调用函数
 * @param {Function} apiFunc 调用 request 的函数，如 request.get/post/put/delete
 * @param  {...any} args apiFunc 的参数
 * @returns {Promise<{success: boolean, data?: any, error?: string}>}
 */
export async function callApi(apiFunc, ...args) {

    try {
        const result = await apiFunc(...args);
        return result;

    } catch (err) {

        if (err.message === 'LOGIN_REQUIRED') {

            alert('Session expired. Please login again.');

        } else {

            alert(`API call error: ${err.message}`);

        }
        return { success: false, error: err.message };
    }
}


/**
 * 获取所有设备列表，需要admin权限
 * @param {} 
 * @returns {Promise<{success: boolean, data?: any, error?: string}>}
 */
export async function getDeviceList() {
    const result = await callApi(request.get, '/api/device/list');
    return result;
}

/**
 * 获取我的设备列表
 * @param {} 
 * @returns {Promise<{success: boolean, data?: any, error?: string}>}
 */
export async function getMyDevices() {
    const result = await request.get('/api/user/devices');
    return result;
}

/**
 * 根据DeviceId获得设备
 * @param {} 
 * @returns {Promise<{success: boolean, data?: any, error?: string}>}
 */
export async function getDeviceById(deviceId) {
    const result = await callApi(request.get, `/api/device/${deviceId}`);
    return result;
}

/**
 * 设备管理
 * @param {} 
 * @returns {Promise<{success: boolean, data?: any, error?: string}>}
 */
export async function addDevice(device) {
    const result = await callApi(request.post, '/api/device/add', device);
    console.log("addDevie", result);
    return result;
}

export async function deleteDevice(deviceId) {
    const result = await callApi(request.delete, `/api/device/${deviceId}`)
    console.table("result:", result);
    return result;
}

export async function updateDevice(device) {
    const result = await callApi(request.post, '/api/device/update', device)
    console.table("result:", result);
    return result;
}

/**
 * 获取最后一条数据
 * @param {string} deviceId 设备ID
 * @returns {Promise<{success: boolean, data?: any, error?: string}>}
 */
export async function getLatestData(deviceId) {
    const result = await callApi(request.get, `/api/Device/${deviceId}/latest`);
    return result;
}

/**
 * 获取设备实时数据
 * @param {string} deviceId 设备ID
 * @returns {Promise<{success: boolean, data?: any, error?: string}>}
 */
export async function getRealtimeData(deviceId) {
    const result = await callApi(request.get, `/api/Device/${deviceId}/realtimeData`);
    return result;
}

/**
* 获取设备历史数据
* @param {string} deviceId 设备ID
* @returns {Promise<{success: boolean, data?: any, error?: string}>}
*/
export async function getHistoryData(deviceId) {
    const result = await callApi(request.get, `/api/Device/${deviceId}/historyData`);
    return result;
}
