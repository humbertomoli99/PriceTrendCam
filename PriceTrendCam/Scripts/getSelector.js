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
        svg.style.background = "rgba(0,0,0,0.3)";
        svg.style.pointerEvents = "none";
        document.body.appendChild(svg);
    }

    const height = Math.max(document.body.scrollHeight, document.documentElement.clientHeight);
    const width = Math.max(document.documentElement.clientWidth, document.body.scrollWidth);

    if (svg.getAttribute('height') !== height.toString() || svg.getAttribute('width') !== width.toString()) {
        svg.setAttribute('height', height);
        svg.setAttribute('width', width);
    }
    var links = document.getElementsByTagName("a");

    for (var i = 0; i < links.length; i++) {
        links[i].setAttribute("onclick", "return false;");
    }

    return svg;
})();

function addMarginToSelector(selector) {
    const svg = document.getElementById('rectangulos-svg');
    const rectanguloHTML = document.querySelector(selector);

    const createSVGElement = (id, fill, stroke) => {
        let svgElement = document.getElementById(id);
        if (!svgElement) {
            svgElement = document.createElementNS('http://www.w3.org/2000/svg', 'rect');
            svgElement.setAttribute('id', id);
            svg.appendChild(svgElement);
        }
        svgElement.setAttribute('fill', fill);
        svgElement.setAttribute('stroke', stroke);
        svgElement.setAttribute('stroke-width', '1');
        return svgElement;
    };

    //const rectanguloParent = rectanguloHTML.parentNode.getBoundingClientRect();
    //const rectanguloParentSVG = createSVGElement('parent-element', 'rgba(255,0,0,0.3)', 'red');
    //rectanguloParentSVG.setAttribute('x', rectanguloParent.left + window.scrollX);
    //rectanguloParentSVG.setAttribute('y', rectanguloParent.top + window.scrollY);
    //rectanguloParentSVG.setAttribute('width', rectanguloParent.width);
    //rectanguloParentSVG.setAttribute('height', rectanguloParent.height);

    const rectanguloPosicion = rectanguloHTML.getBoundingClientRect();
    const rectanguloSVG = createSVGElement('main-element', 'rgba(255,255,0,0.3)', 'yellow');
    rectanguloSVG.setAttribute('x', rectanguloPosicion.left + window.scrollX);
    rectanguloSVG.setAttribute('y', rectanguloPosicion.top + window.scrollY);
    rectanguloSVG.setAttribute('width', rectanguloPosicion.width);
    rectanguloSVG.setAttribute('height', rectanguloPosicion.height);

    //const rectanguloChildren = rectanguloHTML.children[0].getBoundingClientRect();
    //const rectanguloChildrenSVG = createSVGElement('children-element', 'rgba(0,0,255,0.3)', 'blue');
    //rectanguloChildrenSVG.setAttribute('x', rectanguloChildren.left + window.scrollX);
    //rectanguloChildrenSVG.setAttribute('y', rectanguloChildren.top + window.scrollY);
    //rectanguloChildrenSVG.setAttribute('width', rectanguloChildren.width);
    //rectanguloChildrenSVG.setAttribute('height', rectanguloChildren.height);
}

