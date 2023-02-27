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

    // Comprobar si ya existen los elementos
    let rectanguloParentSVG = document.getElementById('parent-element');
    let rectanguloSVG = document.getElementById('main-element');
    let rectanguloChildrenSVG = document.getElementById('children-element');

    const rectanguloHTML = document.querySelector(selector);

    // Si los elementos no existen, crear nuevos
    if (!rectanguloParentSVG) {
        rectanguloParentSVG = document.createElementNS('http://www.w3.org/2000/svg', 'rect');
        rectanguloParentSVG.setAttribute('id', 'parent-element');
        svg.appendChild(rectanguloParentSVG);
    }

    if (!rectanguloSVG) {
        rectanguloSVG = document.createElementNS('http://www.w3.org/2000/svg', 'rect');
        rectanguloSVG.setAttribute('id', 'main-element');
        svg.appendChild(rectanguloSVG);
    }

    if (!rectanguloChildrenSVG) {
        rectanguloChildrenSVG = document.createElementNS('http://www.w3.org/2000/svg', 'rect');
        rectanguloChildrenSVG.setAttribute('id', 'children-element');
        svg.appendChild(rectanguloChildrenSVG);
    }

    const rectanguloChildren = rectanguloHTML.children[0].getBoundingClientRect();
    rectanguloChildrenSVG.setAttribute('x', rectanguloChildren.left + window.scrollX);
    rectanguloChildrenSVG.setAttribute('y', rectanguloChildren.top + window.scrollY);
    rectanguloChildrenSVG.setAttribute('width', rectanguloChildren.width);
    rectanguloChildrenSVG.setAttribute('height', rectanguloChildren.height);
    rectanguloChildrenSVG.setAttribute('fill', 'transparent');
    rectanguloChildrenSVG.setAttribute('stroke', 'blue');
    rectanguloChildrenSVG.setAttribute('stroke-width', '1');

    const rectanguloParent = rectanguloHTML.parentNode.getBoundingClientRect();
    rectanguloParentSVG.setAttribute('x', rectanguloParent.left + window.scrollX);
    rectanguloParentSVG.setAttribute('y', rectanguloParent.top + window.scrollY);
    rectanguloParentSVG.setAttribute('width', rectanguloParent.width);
    rectanguloParentSVG.setAttribute('height', rectanguloParent.height);
    rectanguloParentSVG.setAttribute('fill', 'transparent');
    rectanguloParentSVG.setAttribute('stroke', 'red');
    rectanguloParentSVG.setAttribute('stroke-width', '1');

    const rectanguloPosicion = rectanguloHTML.getBoundingClientRect();
    rectanguloSVG.setAttribute('x', rectanguloPosicion.left + window.scrollX);
    rectanguloSVG.setAttribute('y', rectanguloPosicion.top + window.scrollY);
    rectanguloSVG.setAttribute('width', rectanguloPosicion.width);
    rectanguloSVG.setAttribute('height', rectanguloPosicion.height);
    rectanguloSVG.setAttribute('fill', 'transparent');
    rectanguloSVG.setAttribute('stroke', 'yellow');
    rectanguloSVG.setAttribute('stroke-width', '1');
}
