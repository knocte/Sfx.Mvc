
(function () {
    "use strict";

    var backgroundLayer;
    var messageBox;

    Sfx.alert = function (message, parameters) {

        backgroundLayer = document.createElement('div');
        backgroundLayer.className = "modalLayer";
        document.body.appendChild(backgroundLayer);

        messageBox = document.createElement("div");
        document.body.appendChild(messageBox);
        messageBox.className = "modalBox";

        if (parameters && parameters.title) {
            var title = document.createElement("div");
            title.className = "alertTitle";
            Sfx.setText(title, parameters.title);
            messageBox.appendChild(title);
        }

        var messagePanel = document.createElement("div");
        messagePanel.className = "alertBody";
        messageBox.appendChild(messagePanel);

        // add the message body
        if (message) {
            Sfx.setText(messagePanel, message);
        }
        else if (parameters && parameters.alertBody) {
            messagePanel.appendChild(parameters.alertBody);
        }

        // add the buttons row
        var alertButtons = document.createElement("div");
        messageBox.appendChild(alertButtons);
        alertButtons.className = "alertButtons";

        if (parameters) {
            if (parameters.buttons) {
                Sfx.each(parameters.buttons, function (button) {
                    var input = document.createElement("input");
                    input.type = "button";
                    input.onclick = button.onclick;
                    input.value = button.text;
                    input.className = button.defaultButton ? "metadataButton defaultButton" : "metadataButton";
                    alertButtons.appendChild(input);
                });
            }

            if (parameters.onBackgroundLayerClick) {
                backgroundLayer.onclick = parameters.onBackgroundLayerClick;
            }
        }

        if (!parameters || !parameters.buttons) {
            var input = document.createElement("input");
            input.type = "button";
            input.onclick = Sfx.closeAlert;
            input.value = Sfx.t("Aceptar");
            input.className = "button";
            alertButtons.appendChild(input);
        }

        var messageBoxWidth = (parameters && parameters.width) ? parameters.width : 400;

        var windowSize = Sfx.getWindowSize();
        var scroll = Sfx.getScrollPosition();
        var left = ((windowSize.width - messageBoxWidth) / 2) + scroll.x;
        var top = ((windowSize.height - messageBox.offsetHeight) / 2) + scroll.y;
        messageBox.style.width = messageBoxWidth + 'px';
        messageBox.style.left = left + 'px';
        messageBox.style.top = top + 'px';

        if (parameters && parameters.timeOut) {
            setTimeout(function () {
                Sfx.fadeOut(messageBox, 20, Sfx.closeAlert);
            },
            parameters.timeOut);
        }
    };

    Sfx.closeAlert = function () {
        if (messageBox) {
            Sfx.removeFromBody(messageBox);
            messageBox = null;
        }

        if (backgroundLayer) {
            Sfx.removeFromBody(backgroundLayer);
            backgroundLayer = null;
        }
    };


})();