let isMarginActive = true;

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
    var cssSelector = path.join(' > ');
    console.log(cssSelector); // Imprime el selector CSS en la consola
    return cssSelector;
}

//(function () {
//    let svg = document.getElementById('rectangulos-svg');
//    if (!svg) {
//        svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
//        svg.setAttribute('id', 'rectangulos-svg');
//        svg.style.position = "absolute";
//        svg.style.top = "0";
//        svg.style.left = "0";
//        svg.style.background = "rgba(0,0,0,0.3)";
//        svg.style.pointerEvents = "none";
//        document.body.appendChild(svg);
//    }

//    const height = Math.max(document.body.scrollHeight, document.documentElement.clientHeight);
//    const width = Math.max(document.documentElement.clientWidth, document.body.scrollWidth);

//    if (svg.getAttribute('height') !== height.toString() || svg.getAttribute('width') !== width.toString()) {
//        svg.setAttribute('height', height);
//        svg.setAttribute('width', width);
//    }

//    return svg;
//})();

function toggleSvg(enabled) {
    let svg = document.getElementById('rectangulos-svg');
    if (enabled) {
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

        return svg;
    } else {
        if (svg) {
            svg.remove();
        }
    }
}

function toggleLinks(enabled) {
    var links = document.getElementsByTagName("a");

    for (var i = 0; i < links.length; i++) {
        if (enabled) {
            links[i].removeAttribute("onclick");
            links[i].style.pointerEvents = "auto";
            links[i].style.opacity = 1;
        } else {
            links[i].setAttribute("onclick", "return false;");
            links[i].style.opacity = 0.5;
        }
    }
}


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

        if (!isMarginActive) {
            return;
        }
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

function obtenerElementoPadre(selectorCSS) {
    var elementoHijo = document.querySelector(selectorCSS);
    var elementoPadre = elementoHijo.parentNode;
    console.log(elementoPadre);
    return elementoPadre;
}

function obtenerArbolElementos(selectorCSS) {
    // Obtener el elemento seleccionado
    var elementoSeleccionado = document.querySelector(selectorCSS);

    // Obtener el selector CSS del elemento seleccionado
    var selectorCSSActual = getCssSelector(elementoSeleccionado);

    // Obtener el árbol de padre y hijos del elemento seleccionado
    var arbolElementos = [elementoSeleccionado];
    var selectoresCSS = [selectorCSSActual];
    var elementoPadre = obtenerElementoPadre(selectorCSSActual);
    while (elementoPadre.tagName.toLowerCase() !== 'body') {
        arbolElementos.push(elementoPadre);
        selectorCSSActual = getCssSelector(elementoPadre);
        selectoresCSS.push(selectorCSSActual);
        elementoPadre = obtenerElementoPadre(selectorCSSActual);
    }
    arbolElementos.push(elementoPadre);
    selectoresCSS.push('body');

    // Obtener los selectores CSS de cada elemento en el árbol
    var selectoresCSSArbol = [];
    for (var i = 0; i < arbolElementos.length; i++) {
        var selectorCSS = getCssSelector(arbolElementos[i]);
        selectoresCSSArbol.push(selectorCSS);
    }

    // Devolver la lista de selectores CSS del árbol de elementos
    return selectoresCSSArbol;
}


function getElementInnerText(selector) {
    const element = document.querySelector(selector);
    if (!element) {
        console.error(`Element with selector ${selector} not found`);
        return null;
    }
    return element.innerText;
}

function getLinkHref(selector) {
    const link = document.querySelector(selector);
    if (!link) {
        console.error(`Link with selector ${selector} not found`);
        return null;
    }
    return link.href;
}

function getElementSrc(selector) {
    const element = document.querySelector(selector);
    if (!element) {
        console.error(`Element with selector ${selector} not found`);
        return null;
    }
    return element.src;
}

function getAttributeNames(selector) {
    var element = document.querySelector(selector);
    var attributes = element.attributes;
    var attributeNames = [];

    Array.from(attributes).forEach(function (attribute) {
        var attributeName = attribute.name;
        attributeNames.push(attributeName);
    });

    return attributeNames;
}

// Seleccionar todos los elementos con la etiqueta "div"
const elements = document.querySelectorAll("*");

// Array vacío para almacenar los objetos de los elementos
let elementObjects = [];

// Recorrer los elementos y construir objetos para cada uno
elements.forEach((element) => {
    let elementObject = {
        tagName: element.tagName,
        id: element.id,
        classes: [...element.classList],
        text: element.innerText,
        attributes: {},
    };

    // Recorrer los atributos del elemento y agregarlos al objeto
    for (let i = 0; i < element.attributes.length; i++) {
        let attribute = element.attributes[i];
        elementObject.attributes[attribute.nodeName] = attribute.nodeValue;
    }

    // Agregar el objeto del elemento al array
    elementObjects.push(elementObject);
});

// Convertir el array de objetos a JSON
let json = JSON.stringify(elementObjects);
console.log(json);

function getElementsAsJson() {
    const elements = document.querySelectorAll("*");
    let elementObjects = [];
    elements.forEach((element) => {
        let elementObject = {
            tagName: element.tagName,
            id: element.id,
            classes: [...element.classList],
            text: element.innerText,
            attributes: {},
        };
        for (let i = 0; i < element.attributes.length; i++) {
            let attribute = element.attributes[i];
            elementObject.attributes[attribute.nodeName] = attribute.nodeValue;
        }
        elementObjects.push(elementObject);
    });
    return JSON.stringify(elementObjects);
}
function getElementsAsJson2() {
    const elements = document.querySelectorAll("*");
    let elementObjects = [];
    elements.forEach((element) => {
        let elementObject = {
            tagName: element.tagName.toLowerCase(),
            id: element.id,
            classes: [...element.classList],
            text: element.innerText,
            attributes: {},
        };
        for (let i = 0; i < element.attributes.length; i++) {
            let attribute = element.attributes[i];
            elementObject.attributes[attribute.nodeName] = attribute.nodeValue;
        }
        elementObjects.push(elementObject);
    });
    return JSON.stringify(elementObjects);
}

function getPrototypeChain(obj) {
    var chain = [];
    while (obj !== null) {
        chain.push(Object.getOwnPropertyNames(obj));
        obj = Object.getPrototypeOf(obj);
    }
    return chain.flat().join(', ');
}

function isElementInDOM(selector) {
    return Boolean(document.querySelector(selector));
}
