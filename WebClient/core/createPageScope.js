// core/pageScope.js
export function createPageScope() {
    const cleanups = [];
    let destroyed = false;

    function addCleanup(fn) {
        if (destroyed) return;
        cleanups.push(fn);
    }

    function addListener(target, event, handler, options) {
        target.addEventListener(event, handler, options);
        addCleanup(() =>
            target.removeEventListener(event, handler, options)
        );
    }

    function addInterval(fn, ms) {
        const id = setInterval(fn, ms);
        addCleanup(() => clearInterval(id));
        return id;
    }

    function destroy() {
        if (destroyed) return;
        destroyed = true;
        for (const fn of cleanups) {
            try { fn(); } catch (e) { console.error(e); }
        }
        cleanups.length = 0;
    }

    return {
        addCleanup,
        addListener,
        addInterval,
        destroy
    };
}
