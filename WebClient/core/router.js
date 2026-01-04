// core/router.js

import routes from './routes.js';
import { parseRoute } from './parseRoute.js';
import { loadPage } from './loadPage.js';
import { loadSubPage } from './loadSubPage.js';
import { authGuard } from '../api/auth.js';
import { loadPageCss } from './loadCss.js'
import { isStandalonePage } from '/utils/pageGuard.js';
import { initAuthorizedApp } from '../index.js';

/**
 * Router 内部唯一状态
 */
const state = {
    page: null,          // 当前一级页面名
    subpage: null,       // 当前子页面名
    pageCtx: null,       // PageContext
    subpageCtx: null,     // SubPageContext
    routeInfo:null
};

let navigating = false;
let pending = false;

async function renderRoute(routeInfo) {

    // ----------------------
    // 路由匹配
    // ----------------------
    const basePath = '/' + (routeInfo.segments[0] || 'dashboard');
    const route = routes.find(r => r.path === basePath);

    if (!route) {
        location.hash = '/dashboard';
        return;
    }

    const nextPage = route.page;
    const nextSubpage = route.hasSubpage ? routeInfo.subpage : null;

    // ==================================================
    // 一级页面切换
    // ==================================================
    if (state.page !== nextPage) {

        // 1. 销毁旧子页面
        if (state.subpageCtx) {
            safeDestroy(state.subpageCtx);
            state.subpageCtx = null;
            state.subpage = null;
        }

        // 2. 销毁旧一级页面
        if (state.pageCtx) {
            safeDestroy(state.pageCtx);
            state.pageCtx = null;
            state.page = null;
        }

        // 3. 加载新一级页面
        state.pageCtx = await loadPage(nextPage, route.container);
        state.page = nextPage;

        loadPageCss(nextPage);

        // 4. 初始化一级页面
        routeInfo.pageTitle = route.title;
        state.routeInfo = routeInfo;
        state.pageCtx.init(routeInfo);

    } else {
        // 同一一级页面，仅刷新
        state.pageCtx?.refresh(routeInfo);
    }

    // ==================================================
    // 子页面处理
    // ==================================================
    if (route.hasSubpage && nextSubpage) {

        const subpageKey = `${nextPage}/${nextSubpage}`;

        if (state.subpage !== subpageKey) {

            // 1. 销毁旧子页面
            if (state.subpageCtx) {
                safeDestroy(state.subpageCtx);
            }

            // 2. 加载新子页面
            state.subpageCtx = await loadSubPage(
                nextPage,
                nextSubpage,
                route.subpageContainer
            );

            state.subpage = subpageKey;

            // 3. 初始化子页面
            state.routeInfo = routeInfo;
            state.subpageCtx.init(routeInfo);

        } else {
            // 同一子页面，仅刷新
            state.subpageCtx?.refresh(routeInfo);
        }

    } else {
        // 当前路由不需要子页面，但之前存在 → 销毁
        if (state.subpageCtx) {
            safeDestroy(state.subpageCtx);
            state.subpageCtx = null;
            state.subpage = null;
        }
    }
}

/**
 * 导航流程（权限感知版）
 */
async function performNavigation() {

    const routeInfo = parseRoute(location.hash);
    const path = location.hash.slice(1) || '/dashboard';

    //不需要验证的页面
    //if (!routeInfo.requiresAuth) {
    //    await renderRoute(routeInfo);
    //    return;
    //}

    //登录页面
    if (routeInfo.pageType === 'login') {
        await renderRoute(routeInfo);
        return;
    }

    //需要验证的页面
    if (!authGuard(routeInfo)) {

        localStorage.setItem('redirect_after_login', path);
        // 防止重复跳转死循环
        if (location.hash !== '#/login') {
            location.hash = '/login';
        }

        return;
    }

    await initAuthorizedApp();

    await renderRoute(routeInfo);
}


/**
 * 导航流程
 */
async function performNavigation1() {

    const routeInfo = parseRoute(location.hash);
    // ----------------------
    // 登录拦截
    // ----------------------
    const path = location.hash.slice(1) || '/dashboard';

    if (routeInfo.pageType === 'login') {
        const isLoggedIn = localStorage.getItem('isLoggedIn');

        return;
    }
    else if(!authGuard(routeInfo)) {
        localStorage.setItem('redirect_after_login', path);
        location.hash = '/login';
        return;
    }

    initAuthorizedApp();

    // ----------------------
    // 路由匹配
    // ----------------------
    const basePath = '/' + (routeInfo.segments[0] || 'dashboard');
    const route = routes.find(r => r.path === basePath);

    if (!route) {
        location.hash = '/dashboard';
        return;
    }

    const nextPage = route.page;
    const nextSubpage = route.hasSubpage ? routeInfo.subpage : null;

    // ==================================================
    // 一级页面切换
    // ==================================================
    if (state.page !== nextPage) {
        // 1. 销毁子页面
        if (state.subpageCtx) {
            safeDestroy(state.subpageCtx);
            state.subpageCtx = null;
            state.subpage = null;
        }

        // 2. 销毁一级页面
        if (state.pageCtx) {
            safeDestroy(state.pageCtx);
            state.pageCtx = null;
            state.page = null;
        }

        // 3. 加载新页面
        state.pageCtx = await loadPage(nextPage, route.container);
        state.page = nextPage;
        loadPageCss(nextPage);

        // 4. 初始化页面
        routeInfo['pageTitle'] = route.title;
        state.routeInfo = routeInfo;
        state.pageCtx.init(routeInfo);

    } else {
        // 同一页面，仅刷新
        state.pageCtx?.refresh(routeInfo);
    }

    // ==================================================
    // 子页面处理
    // ==================================================
    if (route.hasSubpage && nextSubpage) {
        const subpageKey = `${nextPage}/${nextSubpage}`;

        if (state.subpage !== subpageKey) {
            // 1. 销毁旧子页面
            if (state.subpageCtx) {
                safeDestroy(state.subpageCtx);
            }

            // 2. 加载新子页面
            state.subpageCtx = await loadSubPage(
                nextPage,
                nextSubpage,
                route.subpageContainer
            );
            state.subpage = subpageKey;

            // 3. 初始化子页面s
            state.routeInfo = routeInfo;
            state.subpageCtx.init(routeInfo);

        } else {
            // 同一子页面，仅刷新
            state.subpageCtx?.refresh(routeInfo);
        }
    } else {
        // 当前路由不需要子页面，但之前有 → 销毁
        if (state.subpageCtx) {
            safeDestroy(state.subpageCtx);
            state.subpageCtx = null;
            state.subpage = null;
        }
    }
}

/**
 * 防并发导航（只保留最后一次）
 */
async function navigate() {
    if (navigating) {
        pending = true;
        return;
    }

    navigating = true;
    try {
        await performNavigation();
    } catch (err) {
        console.error('[router] navigation error:', err);
    } finally {
        navigating = false;
        if (pending) {
            pending = false;
            navigate();
        }
    }
}

/**
 * destroy 安全包装
 */
function safeDestroy(ctx) {
    try {
        ctx.destroy();
    } catch (e) {
        console.error('[router] destroy error:', e);
    }
}

/**
 * Router 初始化
 */
export function initRouter() {

    if (isStandalonePage()) {
        console.log('standalone page, router bypassed');
        return;
    }

    window.addEventListener('load', navigate);
    window.addEventListener('hashchange', navigate);
}

export function refreshCurrentPage() {
    // 优先刷新子页面
    if (state.subpageCtx?.refresh) {
        state.subpageCtx.refresh(state.routeInfo);
        return;
    }
    // 刷新一级页面
    if (state.pageCtx?.refresh) {
        state.pageCtx.refresh(state.routeInfo);
    }
}
