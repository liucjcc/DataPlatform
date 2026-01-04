// core/loadSubPage.js

/**
 * 加载子页面
 * @param {string} pageName        一级页面名，如 'device'
 * @param {string} subpageName     子页面名，如 'history'
 * @param {string} containerId     子页面容器 id
 * @returns {Promise<PageContext>}
 */
export async function loadSubPage(pageName, subpageName, containerId) {
    const container = document.getElementById(containerId);
    if (!container) {
        throw new Error(`[loadSubPage] container not found: ${containerId}`);
    }

    //const base = `/pages/${pageName}/subpages/${subpageName}`;
    const base = `/pages/${pageName}`;

    // ----------------------
    // HTML
    // ----------------------
    container.innerHTML = await fetchHtml(`${base}/${subpageName}.html`);

    // ----------------------
    // JS
    // ----------------------
    const mod = await import(`${base}/${subpageName}.js`);

    if (typeof mod.createPage !== 'function') {
        throw new Error(
            `[loadSubPage] ${base} must export createPage()`
        );
    }

    const ctx = mod.createPage();
    validatePageContext(ctx, base);

    return ctx;
}

/* ---------------------------------- */
/* helpers（与 loadPage 可抽公共）     */
/* ---------------------------------- */

async function fetchHtml(path) {
    const res = await fetch(path);
    if (!res.ok) {
        throw new Error(`[loadSubPage] failed to load html: ${path}`);
    }
    return await res.text();
}

function validatePageContext(ctx, from) {
    const required = ['init', 'refresh', 'destroy'];
    for (const fn of required) {
        if (typeof ctx[fn] !== 'function') {
            throw new Error(
                `[loadSubPage] PageContext missing "${fn}" in ${from}`
            );
        }
    }
}
