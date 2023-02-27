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
