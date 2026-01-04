export function parseRoute(hash) {
    // 定义 fullPath
    const fullPath = hash.slice(1) || '/dashboard';  // 去掉 # 符号
    const [path, queryString] = fullPath.split('?');  // 分割 path 与 query
    const segments = path.split('/').filter(Boolean); // 去掉空字符串
    const query = Object.fromEntries(new URLSearchParams(queryString || ''));

    const pageType = segments[0] || 'dashboard';     // 一级页面
    const pageParams = {};
    
    // 动态参数解析
    if (pageType === 'device') {
        pageParams.deviceId = segments[1];
    }
    else if (pageType === 'user') {
        pageParams.userId = segments[1];
    }
    // 可扩展其他页面类型

    // 子页面
    const subpage = segments[2] || segments[1] || '';

    return { fullPath, segments, query, pageType, pageParams, subpage };
}
