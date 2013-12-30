
/**
    @title Events.
    @summary Define funciones para trabajar con eventos.
*/


(function () {
    "use strict";

    /**
    @title attach           
    Attaches an event to an Html element
    @param userCapture ver https://developer.mozilla.org/en/AddEventListener
    */
    Sfx.attach = function (element, evType, func, useCapture) {
        if (typeof (element) == 'string') {
            element = document.getElementById(element);
        }

        if (element.addEventListener) {
            element.addEventListener(evType, func, useCapture);
        }
        else if (element.attachEvent) {
            element.attachEvent('on' + evType, func);
        }
        else {
            element['on' + evType] = func;
        }
    };

    /**
    @title unAttach           
    Attaches an event to an Html element
    @param userCapture ver https://developer.mozilla.org/en/AddEventListener
    */
    Sfx.detach = function (element, evType, func, useCapture) {
        if (typeof (element) == 'string') {
            element = document.getElementById(element);
        }

        if (Sfx.isDefined(element.addEventListener)) {
            element.removeEventListener(evType, func, useCapture);
        }
        else if (Sfx.isDefined(element.attachEvent)) {
            element.detachEvent('on' + evType, func);
        }
        else {
            element['on' + evType] = null;
        }
    };

    /**
    @title onLoad
    Ejecuta la función cuando la página está cargada.
    */
    Sfx.onLoad = function (func) {
        Sfx.attach(window, 'load', func);
    };

    // Ejecuta la función cuando la página tiene el DOM listo para trabajar, 
    // aunque falten por cargar imágenes pesadas, etc..
    // http://www.javascriptkit.com/dhtmltutors/domready.shtml
    Sfx.onReady = function (funcName) {
        var alreadyrunflag = 0 //flag to indicate whether target function has already been run

        if (document.addEventListener) {
            document.addEventListener("DOMContentLoaded", function () {
                alreadyrunflag = 1;
                funcName();
            }, false);
        } else if (document.all && !window.opera) {
            document.write('<script type="text/javascript" id="contentloadtag" defer="defer" src="javascript:void(0)"><\/script>');
            document.getElementById("contentloadtag").onreadystatechange = function () {
                if (this.readyState == "complete") {
                    alreadyrunflag = 1;
                    funcName();
                }
            }
        }

        window.onload = function () {
            setTimeout(function () { if (!alreadyrunflag) funcName(); }, 0);
        };
    };
        
    // Ejecuta la función cuando el hashtag de la URL haya cambiado.
    Sfx.onHashChange = function (func) {
        if (("onhashchange" in window) && !(Sfx.isIExplorer())) {
            window.onhashchange = function () {
                func();
            }
        }
        else {
            var prevHash = window.location.hash;
            window.setInterval(function () {
                if (window.location.hash != prevHash) {
                    storedHash = window.location.hash;
                    func();
                }
            }, 100);
        }
    }
    
     // Llama a la función si se hace click en cualquier elemento del formulario
	Sfx.onGlobal = function (eventName, funcName) {
		var globalAttached = true;

		function onEvent() {
			if (globalAttached) {
			    globalAttached = false;
			    Sfx.detach(window.document.body, eventName, onEvent);
			    funcName();
			}
		}

		setTimeout(function () { Sfx.attach(window.document.body, eventName, onEvent); }, 300);
	};

    //Prevents further propagation of the current event.
    Sfx.cancelBubble = function (event) {
        if (!event) {
            event = event || window.event;
        }
        /*ojo en firefox event y window.event siempre es  null*/
            //if (!event) return;
        if (event.cancelBubble) {
            event.cancelBubble = true;
        }

        if (Sfx.isDefined(event.stopPropagation)) {
            event.stopPropagation();
        }
    };

    /**
    * Previene que el navegador ejecute su accion por defecto pero permite que otros elementos sigan escuchando al evento.
    * Por ejemplo previente que presionar Enter en un formulario haga submit, pero deja que otros controles procesen el Enter.
    */
    Sfx.stopBubble = function (event) {
        if (!event) {
            event = window.event;
        }

        if (Sfx.isDefined(event.preventDefault)) {
            event.preventDefault();
        }
        else {
            event.returnValue = false;
        }
    };

    // Devielve el elemento que ha originado el evento
    Sfx.getTarget = function(event) {
        event = event || window.event;
        return event.target || event.srcElement;
    };

})();
   


