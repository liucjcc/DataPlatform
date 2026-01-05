export function createPage() {
    function init(ctx) {
        console.log("subpage:" + ctx.subpage);
    }

    function refresh(ctx) {
    }

    function destroy() {
    };

    return { init, refresh, destroy };
}