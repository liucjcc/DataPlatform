
export default [
    {
        path: '/login',
        page: 'login',
        container: 'page-container',
        isMenuItem: false
    },

    {
        path: '/dashboard',
        page: 'dashboard',
        title: 'Dashboard',
        container: 'page-container',
        hasSubpage: true,
        requireAuth: true,
        isMenuItem: true
    },

    {
        path: '/page1',
        page: 'page1',
        container: 'page-container',
        title: '事件报警',
        hasSubpage: true,
        requireAuth: true,
        isMenuItem: true
    },

    {
        path: '/page2',
        page: 'page2',
        container: 'page-container',
        title: '测试页面',
        hasSubpage: true,
        requireAuth: true,
        isMenuItem: true
    },

    {
        path: '/device',
        page: 'device',
        title: '设备仪表盘',
        container: 'page-container',
        subpageContainer: "subpage-container",
        hasSubpage: true,
        requireAuth: true,
        isMenuItem: false
    }
];