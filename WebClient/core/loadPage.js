// core/loadPage.js

/**
 * 加载一级页面
 * @param {string} pageName       页面名，如 'dashboard' / 'device'
 * @param {string} containerId    容器 id
 * @returns {Promise<PageContext>}
 */
export async function loadPage(pageName, containerId) {
    const container = document.getElementById(containerId);
    if (!container) {
        throw new Error(`[loadPage] container not found: ${containerId}`);
    }

    // ----------------------
    // 加载 HTML
    // ----------------------
    const htmlPath = `/pages/${pageName}/${pageName}.html`;
    container.innerHTML = await fetchHtml(htmlPath);

    // ----------------------
    // 加载 JS 模块
    // ----------------------
    const modPath = `/pages/${pageName}/${pageName}.js`;
    const mod = await import(modPath);

    if (typeof mod.createPage !== 'function') {
        throw new Error(
            `[loadPage] ${modPath} must export createPage()`
        );
    }

    const ctx = mod.createPage();
    validatePageContext(ctx, modPath);

    return ctx;
}

/* ---------------------------------- */
/* helpers                            */
/* ---------------------------------- */

async function fetchHtml(path) {
    const res = await fetch(path);
    if (!res.ok) {
        throw new Error(`[loadPage] failed to load html: ${path}`);
    }
    return await res.text();
}

function validatePageContext(ctx, from) {
    const required = ['init', 'refresh', 'destroy'];
    for (const fn of required) {
        if (typeof ctx[fn] !== 'function') {
            throw new Error(
                `[loadPage] PageContext missing "${fn}" in ${from}`
            );
        }
    }
}
