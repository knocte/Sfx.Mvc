
/**
    @title DOM.
    @summary El módulo DOM define funciones para trabajar con el Document Object Model.
*/
 
(function () {
    "use strict";

    /**
    @title get
    Obtiene el elemento HTML si se pasa el nombre o devuelve el mismo si es objeto.
    */
    Sfx.get = function (element) {
        if (typeof (element) == 'string') {
            element = document.getElementById(element);
        }
        return element;
    };

    // Busca todos los elementos que tengan la clase especificada.
    // Ejemplos:
    //     Sfx.select('menu');
    //     Sfx.select('menu*');
    //     Sfx.select('menu', Sfx.get('panel'), "div");
    //
    Sfx.getByClass = function (searchClass, node, tag) {
            node = node == null ? document : Sfx.get(node);

            if (tag == null) {
                tag = '*';
            }

            var elements = node.getElementsByTagName(tag);

            var pattern = searchClass;

            // si es un comodin, quitarlo y si no indicar que tiene que empezar asi
            pattern = searchClass.charAt(0) != '*' ? '(^|\\s)' + pattern : pattern.substring(1);

            pattern = searchClass.charAt(searchClass.length - 1) != '*' ? 
                pattern + '(\\s|$)' : 
                pattern.substring(0, pattern.length - 1);

            var regex = new RegExp(pattern);

            var elementsLength = elements.length;

            var result = new Array();

            for (var i = 0; i < elementsLength; i++) {
                var className = elements[i].className;
                if(className != null) {
                    var values = className.split(' ');
                    for(var k=0; k < values.length; k++) {
                        if (regex.test(values[k])) {
                            result.push(elements[i]);
                        }
                    }
                }
            }

            return result;
    }

    /**
    @title clearChildNodes
    Remove all child nodes from an element
    */
    Sfx.clearChildNodes = function (element) {
        if (typeof (element) == 'string') {
            element = document.getElementById(element);
        }
        while (element.hasChildNodes()) {
            element.removeChild(element.firstChild);
        }
    };

    /**
    @title hasClass
    Devuelve true en caso de que el elemento tenga establecido el className.
    */
    Sfx.hasClass = function (element, className) {
        if (typeof (element) == 'string') {
            element = document.getElementById(element);
        }
        return new RegExp('\\b' + className + '\\b').test(element.className);
    };

    
    Sfx.getComputedStyle  = function (element, cssProperty){
        if (element.currentStyle) { //IE
          return element.currentStyle[cssProperty];
        }
        else if (document.defaultView && document.defaultView.getComputedStyle) { //Firefox
          return document.defaultView.getComputedStyle(element, "")[cssProperty];
        }
        else { //try and get inline style
          return element.style[cssProperty];
        }
    }

    /**
    @title removeClass
    Elimina el className del elemento.
    */
    Sfx.removeClass = function (element, className) {
        if (typeof (element) == 'string') {
            element = document.getElementById(element);
        }
        var regEx = new RegExp('\\b' + className + '\\b');
        element.className = element.className.replace(regEx, ' ');
    };

    /**
    @title addClass
    Añade el className al elemento.
    */
    Sfx.addClass = function (element, className) {
        if (typeof (element) == 'string') {
            element = document.getElementById(element);
        }
        if (!Sfx.hasClass(element, className)) {
            element.className += " " + className;
        }
    };

    /**
    @title swapClass
    Intercambia los className en el elemento.
    */
    Sfx.swapClass = function (element, classA, classB) {
        if (typeof (element) == 'string') {
            element = document.getElementById(element);
        }
        var regEx = new RegExp('\\b' + classA + '\\b');
        element.className = element.className.replace(regEx, " " + classB + " ");
    };

    /**
    @title getFormValues
    Devuelve un diccionario con los valores del formulario
    */
    Sfx.getFormValues = function (form) {
        var array = [];
        for (var i = 0, l = form.elements.length; i < l; i++) {
            var element = form.elements[i];
            if (element.name !== undefined) {
                if (element.type !== undefined && element.type.toLowerCase() == "checkbox") {
                    if (element.checked == true) {
                        array.push({ name: element.name, value: "on" });
                    }
                }
                else if (element.value !== undefined) {
                    array.push({ name: element.name, value: element.value });
                }
            }
        }

        return array;
    };


    Sfx.getBody = function () {
        var items = document.getElementsByTagName("body");
        return items.length > 0 ? items[0] : null;
    };

    
    /**
    Añade el item al elemento Body de la página
    */
    Sfx.appendToBody = function (element) {
        Sfx.getBody().appendChild(element);
    };
    
    // Elimina el item del elemento Body de la página
    Sfx.removeFromBody = function (element) {
        element = Sfx.get(element);
        if (element != null && (element.parentElement != null || element.parentNode != null)) {
           document.getElementsByTagName("body")[0].removeChild(element);
        }
    };
    
    // Obtiene el elemento que contiene a element.
    Sfx.parentNode = function (element) {
        return element.parentElement || element.parentNode;
    };
    
    // Añade element al parent de obj
    Sfx.appendToParent = function (obj, element) {
        var parent = Sfx.parentNode(obj);
        parent.appendChild(element);
    };
    
    /**
    @title removeNode
    Elimina el nodo del elemento que lo contiene.
    */
    Sfx.removeNode = function (element) {
    	if(element != null) {    	
            var parent = Sfx.parentNode(element);
	        if(parent != null){
                Sfx.parentNode(element).removeChild(element);
	        }
        }
    };
    
    
    // Obtiene la posición de un elemento en lá página
    Sfx.getPosition = function (element) {
        var posx = 0;
        var posy = 0;

        if (element.offsetParent) {
            do {
                posx += element.offsetLeft;
                posy += element.offsetTop;

                if (element.style.position != "") { // = 'absolute') {
                    break;
                }
            }
            while (element = element.offsetParent)
        }
        else if (element.x) {
            posx += element.x;
            posy += element.y;
        }

        return { x: posx, y: posy };
    };

    Sfx.getWindowSize = function () {
        var width = 0;
        var height = 0;

        if (typeof (window.innerWidth) == 'number') {
            //Non-IE
            width = window.innerWidth;
            height = window.innerHeight;
        }
        else if (document.documentElement && (document.documentElement.clientWidth ||
             document.documentElement.clientHeight)) {
            //IE 6+ in 'standards compliant mode'
            width = document.documentElement.clientWidth;
            height = document.documentElement.clientHeight;
        }
        else if (document.body && (document.body.clientWidth || document.body.clientHeight)) {
            //IE 4 compatible
            width = document.body.clientWidth;
            height = document.body.clientHeight;
        }

        return { width: width, height: height };
    };

    Sfx.getDocumentSize = function (item) {
        var body = Sfx.getBody();
        var width = body ? body.clientWidth : window.innerWidth;
        var height = body ? body.clientHeight : window.innerHeight;
        return { width: width, height: height };
    };

    // Returns the size of the object as an array [width, height]
    Sfx.getSize = function (item) {
        item = Sfx.get(item);
        return { width: item.offsetWidth, height: item.offsetHeight };
    };

    Sfx.getScrollPosition = function () {
        var x = 0;
        var y = 0;

        if (document.documentElement.scrollTop) {
            x = document.documentElement.scrollLeft;
            y = document.documentElement.scrollTop;
        }
        else if (document.body.scrollTop) {
            x = document.body.scrollLeft;
            y = document.body.scrollTop;
        }
        else if (window.pageXOffset) {
            x = window.pageXOffset;
            y = window.pageYOffset;
        }
        else if (document.body.parentElement && document.body.parentElement.scrollTop) {
            x = document.body.parentElement.scrollLeft;
            y = document.body.parentElement.scrollTop;
        }
        else if (window.scrollLeft) {
            x = window.scrollLeft;
            y = window.scrollTop;
        }

        return { x: x, y: y }
    };

    // Sets the inner text of an Html element
    Sfx.setText = function (element, text) {
        if (text == null) {
            text = "";
        }

        element = Sfx.get(element);

        if (Sfx.isDefined(element.innerText)) {
            element.innerText = text;
        }
        else {
            element.textContent = text;
        }
    };

    // Devuelve el texto que contiene el elemento.
    Sfx.getText = function (element) {
        element = Sfx.get(element);

        if (Sfx.isDefined(element.innerText)) {
            return element.innerText;
        }
        else {
            return element.textContent;
        }
    };
    
	// Devuelve el formulario que contiene el elemento.
	Sfx.getForm = function (element) {
		return Sfx.getParent(element, ":form");
	};
    
    // Busca un elemento padre en función de un filtro.
    //   * Si el filtro empieza por : busca por tag, por ejemplo :div
    //   * Si empieza por . busca por class
    //   * Si empieza por # busca por id
    Sfx.getParent = function (element, filter) {
        element = Sfx.get(element);
        var filterType = filter.charAt(0);
        var filterText = filter.substring(1);

        while(element){
            element = element.parentNode;

            if(element != null) {
                switch(filterType){
                    case ":":
                        if(element.tagName && 
                            element.tagName.toLowerCase() == filterText ){
                            return element;
                        }
                        break;

                    case ".":
                        if(element.className && 
                            element.className.toLowerCase() == filterText ){
                            return element;
                        }
                        break;

                    case "#":
                        if(element.id && 
                            element.id.toLowerCase() == filterText ){
                            return element;
                        }
                        break;
                }

            }
        }
        return null;
    };

})();
   






































