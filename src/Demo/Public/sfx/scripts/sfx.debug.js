
/**
    @title Debug.
    @summary El módulo Debug proporciona utilidades para depurar páginas.
*/



(function () {
    "use strict";

    Sfx.debug = function () {
        showWindow();

        var line = "";

        for (var i = 0; i < arguments.length; i++) {
            line += arguments[i] + "    ";
        }

        line += "\n";

        traceWindow.appendChild(document.createTextNode(line));
        traceWindow.scrollTop = traceWindow.scrollHeight;
    };

    var debugWindow = null;
    var traceWindow = null;

    function showWindow() {
        if (debugWindow == null) {
            debugWindow = document.createElement('div');
            debugWindow.style.position = 'absolute';
            debugWindow.style.right = '10px';
            debugWindow.style.top = '10px';
            debugWindow.style.border = '1px solid #888888';
            debugWindow.style.backgroundColor = '#f4fafe';
            debugWindow.style.fontSize = '11px';

            var header = document.createElement('div');
            header.style.backgroundColor = '#dedede';
            header.style.padding = '3px 5px';
            header.appendChild(document.createTextNode("S debug Window"));
            debugWindow.appendChild(header);

            header.appendChild(document.createTextNode(" "));

            var clearLink = document.createElement("a");
            clearLink.style.padding = '0 5px';
            clearLink.href = "#";
            clearLink.appendChild(document.createTextNode("Clear"));
            clearLink.onclick = function () {
                Sfx.clearChildNodes(traceWindow);
            };
            header.appendChild(clearLink);

            header.appendChild(document.createTextNode(" "));

            var closeLink = document.createElement("a");
            closeLink.style.padding = '0 5px';
            closeLink.href = "#";
            closeLink.appendChild(document.createTextNode("Close"));
            closeLink.onclick = function () {
                Sfx.removeFromBody(debugWindow);
                traceWindow = null;
                debugWindow = null;
            };
            header.appendChild(closeLink);

            traceWindow = document.createElement("div");
            traceWindow.style.overflow = 'auto';
            traceWindow.style.maxHeight = '200px';
            traceWindow.style.width = '350px';
            traceWindow.style.padding = '10px';
            debugWindow.appendChild(traceWindow);

            Sfx.appendToBody(debugWindow);
        }
    }

})();
   


