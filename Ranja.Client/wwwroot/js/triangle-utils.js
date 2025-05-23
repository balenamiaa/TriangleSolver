// Triangle Utility Functions

window.getBoundingClientRect = (element) => {
    if (!element) return { left: 0, top: 0, width: 0, height: 0 };
    
    const rect = element.getBoundingClientRect();
    return {
        left: rect.left,
        top: rect.top,
        width: rect.width,
        height: rect.height
    };
};

window.triangleUtils = {
    // Get element bounding rectangle
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

    // Calculate angle between three points
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

    // Calculate distance between two points
    calculateDistance: (p1, p2) => {
        const dx = p2.x - p1.x;
        const dy = p2.y - p1.y;
        return Math.sqrt(dx * dx + dy * dy);
    },

    // Convert SVG coordinates to screen coordinates
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