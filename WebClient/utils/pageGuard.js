// utils/pageGuard.js
export function isStandalonePage() {
    const path = location.pathname;
    return path.endsWith('/demo.html')
        || path.endsWith('/preview.html')
        || path.endsWith('/print.html');
}

