let currentPageCss = null;

/**
 * 动态加载页面 CSS
 * @param {string} pageName 页面名称，对应 /pages/{pageName}/{pageName}.css
 * @param {function} callback 加载完成后执行回调（可选）
 */
export function loadPageCss(pageName, callback) {

    // 卸载旧CSS
    unloadPageCss();

    const link = document.createElement('link');
    link.rel = 'stylesheet';

    // 防缓存处理
    link.href = `/pages/${pageName}/${pageName}.css?v=${Date.now()}`;
    link.dataset.page = pageName;

    // 加载完成回调
    link.onload = () => {
        if (typeof callback === 'function') {
            callback();
        }
    };

    link.onerror = () => {
        console.error(`Failed to load CSS: /pages/${pageName}/${pageName}.css`);
    };

    document.head.appendChild(link);
    currentPageCss = link;
}

/**
 * 卸载当前页面 CSS
 */

export function unloadPageCss() {
    if (currentPageCss) {
        currentPageCss.remove();
        currentPageCss = null;
    }
}
