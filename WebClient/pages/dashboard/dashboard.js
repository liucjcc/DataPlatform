export function createPage() {
    function init(ctx) {
        if (ctx.pageTitle) {
            document.getElementById('pageTitle').textContent = ctx.pageTitle;
        }
    }

    function refresh(ctx) {

    }

    function destroy() {


    };

    return { init, refresh, destroy };
}