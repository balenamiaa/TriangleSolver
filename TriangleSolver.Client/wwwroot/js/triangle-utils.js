window.getBoundingClientRect = (element) => { if (!element) return { left: 0, top: 0, width: 0, height: 0 }; const rect = element.getBoundingClientRect(); return { left: rect.left, top: rect.top, width: rect.width, height: rect.height }; }; window.getBoundingClientRectById = (elementId) => { const element = document.getElementById(elementId); if (!element) return { left: 0, top: 0, width: 0, height: 0 }; const rect = element.getBoundingClientRect(); return { left: rect.left, top: rect.top, width: rect.width, height: rect.height }; };

window.getViewportDimensions = () => {
    return {
        width: window.innerWidth,
        height: window.innerHeight
    };
};


window.modalUtils = {
    dotNetRef: null,
    scrollListener: null,
    resizeListener: null,

    init: () => {

    },

    setupScrollListeners: (dotNetRef) => {
        window.modalUtils.dotNetRef = dotNetRef;


        const debounce = (func, wait) => {
            let timeout;
            return function executedFunction(...args) {
                const later = () => {
                    clearTimeout(timeout);
                    func(...args);
                };
                clearTimeout(timeout);
                timeout = setTimeout(later, wait);
            };
        };


        const debouncedHandler = debounce(() => {
            if (window.modalUtils.dotNetRef) {
                window.modalUtils.dotNetRef.invokeMethodAsync('OnScrollOrResize');
            }
        }, 8); // 120fps


        window.modalUtils.scrollListener = debouncedHandler;
        window.modalUtils.resizeListener = debouncedHandler;

        window.addEventListener('scroll', window.modalUtils.scrollListener, { passive: true });
        window.addEventListener('resize', window.modalUtils.resizeListener, { passive: true });


        document.addEventListener('scroll', window.modalUtils.scrollListener, { passive: true, capture: true });
    },

    cleanup: () => {
        if (window.modalUtils.scrollListener) {
            window.removeEventListener('scroll', window.modalUtils.scrollListener);
            document.removeEventListener('scroll', window.modalUtils.scrollListener);
        }
        if (window.modalUtils.resizeListener) {
            window.removeEventListener('resize', window.modalUtils.resizeListener);
        }
        window.modalUtils.dotNetRef = null;
        window.modalUtils.scrollListener = null;
        window.modalUtils.resizeListener = null;
    }
};

window.triangleUtils = {
    getBoundingClientRect: (element) => {
        if (!element) return { left: 0, top: 0, width: 0, height: 0 };

        const rect = element.getBoundingClientRect();
        return {
            left: rect.left,
            top: rect.top,
            width: rect.width,
            height: rect.height
        };
    },

    calculateAngle: (p1, vertex, p2) => {
        const v1 = { x: p1.x - vertex.x, y: p1.y - vertex.y };
        const v2 = { x: p2.x - vertex.x, y: p2.y - vertex.y };

        const dot = v1.x * v2.x + v1.y * v2.y;
        const mag1 = Math.sqrt(v1.x * v1.x + v1.y * v1.y);
        const mag2 = Math.sqrt(v2.x * v2.x + v2.y * v2.y);

        const cos = dot / (mag1 * mag2);
        const angle = Math.acos(Math.max(-1, Math.min(1, cos)));

        return angle * 180 / Math.PI;
    },

    calculateDistance: (p1, p2) => {
        const dx = p2.x - p1.x;
        const dy = p2.y - p1.y;
        return Math.sqrt(dx * dx + dy * dy);
    },

    svgToScreen: (svgElement, svgX, svgY) => {
        if (!svgElement) return { x: 0, y: 0 };

        const rect = svgElement.getBoundingClientRect();
        const viewBox = svgElement.viewBox.baseVal;

        const scaleX = rect.width / viewBox.width;
        const scaleY = rect.height / viewBox.height;

        return {
            x: rect.left + (svgX - viewBox.x) * scaleX,
            y: rect.top + (svgY - viewBox.y) * scaleY
        };
    }
}; 