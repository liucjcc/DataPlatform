export function createPage() {
    console.log("page2 createPage");
    function init(ctx) {
        console.log('page2 init', ctx);
        if (ctx.pageTitle) {
            document.getElementById('pageTitle').textContent = ctx.pageTitle;
        }
    }
    function refresh(ctx) {
        console.log('page2 refresh', ctx);
    }
    function destroy() {
        console.log("page2 destroy");
    };

    return { init, refresh, destroy };
}