
export function createPage() {

    function init(ctx) {
        console.log('page1 init', ctx);

        if (ctx.pageTitle) {
            document.getElementById('pageTitle').textContent = ctx.pageTitle;
        }
    }
    function refresh(ctx) {

        console.log('page1 refresh');

    }
    function destroy() {
        console.log('page1 destory!');

    };

    return { init, refresh, destroy };
}