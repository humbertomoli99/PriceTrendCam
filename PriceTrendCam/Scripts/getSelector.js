function getCssSelector(el) {
    var path = [];
    while (el.nodeType === Node.ELEMENT_NODE) {
        var selector = el.nodeName.toLowerCase();
        if (el.id) {
            selector += '#' + el.id;
            path.unshift(selector);
            break;
        } else {
            var siblingSelector = '';
            var siblingIndex = 1;
            var sibling = el.previousSibling;
            while (sibling) {
                if (sibling.nodeType === Node.ELEMENT_NODE && sibling.nodeName.toLowerCase() === selector) {
                    siblingIndex++;
                }
                sibling = sibling.previousSibling;
            }
            if (siblingIndex > 1) {
                siblingSelector = ':nth-of-type(' + siblingIndex + ')';
            }
            selector += siblingSelector;
            path.unshift(selector);
        }
        el = el.parentNode;
    }
    return path.join(' > ');
}

(function () {
    let svg = document.getElementById('rectangulos-svg');
    if (!svg) {
        svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
        svg.setAttribute('id', 'rectangulos-svg');
        svg.style.position = "absolute";
        svg.style.top = "0";
        svg.style.left = "0";
        svg.style.pointerEvents = "none";
        document.body.appendChild(svg);
    }

    const height = Math.max(document.body.scrollHeight, document.documentElement.clientHeight);
    const width = Math.max(document.documentElement.clientWidth, document.body.scrollWidth);

    if (svg.getAttribute('height') !== height.toString() || svg.getAttribute('width') !== width.toString()) {
        svg.setAttribute('height', height);
        svg.setAttribute('width', width);
    }

    return svg;
})();


function addMarginToSelector(selector) {
    const svg = document.getElementById('rectangulos-svg');

    const rectanguloHTML = document.querySelector(selector);
    const rectanguloSVG = document.createElementNS('http://www.w3.org/2000/svg', 'rect');

    const rectanguloPosicion = rectanguloHTML.getBoundingClientRect();
    rectanguloSVG.setAttribute('x', rectanguloPosicion.left + window.scrollX);
    rectanguloSVG.setAttribute('y', rectanguloPosicion.top + window.scrollY);
    rectanguloSVG.setAttribute('width', rectanguloPosicion.width);
    rectanguloSVG.setAttribute('height', rectanguloPosicion.height);
    rectanguloSVG.setAttribute('fill', 'transparent');
    rectanguloSVG.setAttribute('stroke', 'red');
    rectanguloSVG.setAttribute('stroke-width', '1');

    svg.appendChild(rectanguloSVG);
}