



(function () {
    "use strict";

    if (!Sfx.isDefined(Sfx)) {
        Sfx = {};
    }

	Sfx.autosuggest = function (control, parameters) {
	    var TAB_KEY = 9;
	    var ENTER_KEY = 13;
	    var UP_KEY = 38;
	    var DOWN_KEY = 40;
	    var ROW_PREFIX = "__sAutoSuggestRow";
	    var MIN_CHARS_TO_SEARCH = 1; // el numero mínimo de caracteres que hay que escribir para hacer una petición
	    var idField; // cuando se selecciona algo se genera un input oculto con el id
	    var lastAjaxCall; // la hora de la ultima petición realizada
	    var ajaxTimeout; // el retraso que se pueda hacer para llamar ajax
	    var rawData;  // all the data available
	    var filteredData; // the filtered data shown in the suggestions window.
	    var dataSearchValue; // the value for which data has been loaded.
	    var partialResults;
	    var ajaxCall;
	    var url;
	    var autosuggestWindow;	    
	    var autocomplete; // indica si copia automáticamente al textbox el valor que se esta seleccionando
	    var maxRows = 15;	    
	    var currentIndex = -1; // el valor que esta seleccionado	    
	    var createRowFunc; // la función que crea el row en formato function createRowFunc(item, colorizedText)
	    var sourceInput; // el control en el que se escribe. Puede ser distinto del que muestra los resultados.
	    var disableIdInput; // si evita que se cree un input hidden para almacenar el valor del id seleccionado.

	    control = Sfx.get(control);
	    initialize(parameters);


	    function initialize(parameters) {
	    	disableIdInput = false;
	    	sourceInput = control;

	        if (parameters) {
	            rawData = parameters.data ? parameters.data : [];
	            url = parameters.url;
	            maxRows = parameters.maxRows;
	            createRowFunc = parameters.createRowFunc;
	            autocomplete = parameters.autoComplete || false;

	            if(parameters.sourceInput){
			        sourceInput = Sfx.get(parameters.sourceInput) || control;
			    }
			    
		        disableIdInput = parameters.disableIdInput || false;
	        }

		    Sfx.attach(sourceInput, "keydown", onKeyDown);
		    Sfx.attach(sourceInput, "keyup", onKeyUp);
	    }

	    function onKeyDown(e) {
	        switch (e.keyCode) {
	            case TAB_KEY:
	                hideWindow();
	                onItemSelected();
	                break;

	            case ENTER_KEY:
	                var cancelBubble = autosuggestWindow != null && autosuggestWindow.parentElement != null;
	                hideWindow();
	                onItemSelected();
	                if (cancelBubble) {
	                    // Unicamente cancelar si estaba desplegadada la ventana. (comprobarlo antes de llamar a hideWindow!)
	                    Sfx.cancelBubble(e);
	                    Sfx.stopBubble(e);
	                    return false;
	                }
	                break;

	            case UP_KEY:
	                onUpArrowClick();
	                break;

	            case DOWN_KEY:
	                onDownArrowClick();
	                break;
	        }
	    }

	    function onKeyUp(e) {
	        switch (e.keyCode) {
	            case 9: // TAB
	            case 13: // enter
	            case 38: // arrow up
	            case 40: // arrow up
	                break;

	            default:
	                onTextChanged();
	                break;
	        }
	    }

	    function onDownArrowClick() {
	        if (filteredData) {
	            if (currentIndex < filteredData.length - 1) {
	                currentIndex++;

	                if (autocomplete) {
	                    // mostrar el contenido en el campo
	                    control.value = getItemText(filteredData[currentIndex]);
	                }
	            }

	            updateSelectedRow();
	        }
	    }

	    function onUpArrowClick() {
	        if (currentIndex > 0) {
	            currentIndex--;

	            if (autocomplete) {
	                // mostrar el contenido en el campo
	                control.value = getItemText(filteredData[currentIndex]);
	            }
	        }

	        updateSelectedRow();
	    }

	    function updateSelectedRow() {
	        for (var i = 0, l = filteredData.length; i < l; i++) {
	            var row = document.getElementById(ROW_PREFIX + i);

	            if (i == currentIndex) {
	                Sfx.addClass(row, "selectedRow");
	            }
	            else {
	                Sfx.removeClass(row, "selectedRow");
	            }
	        }
	    }

	    function onTextChanged() {

	    	onUnselectedItem();

	        var controlValue = sourceInput.value;

	        if (controlValue == null || controlValue.length == 0) {
	            hideWindow();
	        }
	        else if (filteredData == null || // si no se han recibido datos todavia
	                    partialResults || // si los datos no son todos los disponibles.
	                    !new RegExp(Sfx.removeAccents(dataSearchValue), "i").test(Sfx.removeAccents(controlValue))) // si el valor no empieza como lo que se pidió
	        {
	            if (controlValue.length >= MIN_CHARS_TO_SEARCH) // hacer que no empieze a buscar hasta que no se metan n letras
	            {
	                loadData(controlValue);
	            }
	        }
	        else {
	            filterData(controlValue);
	            showWindow(controlValue);
	        }
	    }

	    function showWindow(searchText) {
	        if (filteredData == null) {
	            // dont show the window if there is no data to show.
	            return;
	        }

	        var itemCount = filteredData.length;

	        if (itemCount == 0) {
	            hideWindow();
	            return;
	        }

	        if (autosuggestWindow != null) {
	            Sfx.clearChildNodes(autosuggestWindow);

	            if (autosuggestWindow.parentElement == null) {
	                Sfx.appendToBody(autosuggestWindow);
	            }
	        }
	        else {
	            autosuggestWindow = document.createElement("div");
	            Sfx.appendToBody(autosuggestWindow);

	            var controlPosition = Sfx.getPosition(control);
	            var controlSize = Sfx.getSize(control);
	            autosuggestWindow.className = "autosuggestWindow";
	            autosuggestWindow.style.position = "absolute";
	            autosuggestWindow.style.left = controlPosition.x + "px";
	            autosuggestWindow.style.top = controlPosition.y + controlSize.height + "px";
	            Sfx.onGlobal(function () { hideWindow(); });
	        }

	        // add the suggestion rows
	        for (var i = 0; i < itemCount; i++) {
	            var row = document.createElement("div");
	            row.id = ROW_PREFIX + i;
	            autosuggestWindow.appendChild(row);

	            var item = filteredData[i];

	            var colorizedText = getItemText(item).replace(new RegExp(searchText, "gi"), "<b>$&</b>");

	            // si se ha proporcionado una función para generar el html del row, invocarla, si no, meter la descripción.
	            row.innerHTML = createRowFunc ? createRowFunc(item, colorizedText) : colorizedText;

	            var eventName = "click"; //Sfx.isIpadOrIphone() ? "touchend" : "click";
	            Sfx.attach(row, eventName, (function (k) { return function (e) { onRowClick(e, k); } })(i)); // evaluar la función para evitar el closure..
	        }
	    }

	    function hideWindow() {
	        if (autosuggestWindow != null) {
	            try {
	                autosuggestWindow.style.display = "none";
	                Sfx.removeNode(autosuggestWindow);
	            }
	            catch (e) {
	            }
	            autosuggestWindow = null;
	        }
	    }

	    // muestra un panel de solo lectura con el valor 
	    // y oculta el campo
	    function onItemSelected() {
	        if (currentIndex != -1) {
	            var item = filteredData[currentIndex];
	            control.value = getItemText(item);

	            if(!disableIdInput) {
		            // Crear el control que almacena el valor que se envía en el formulario
		            if(idField == null) {
		            	var form = control.form;
		            	if(form != null){
				        	idField = document.createElement("input");
					        idField.type = "hidden";
					        form.appendChild(idField);
					    }
			        }
					    
					if(idField != null) {   
						idField.value = item;
					}
				}

		        Sfx.addClass(control, "selectedItem");

	            if (parameters && parameters.onItemSelected) {
	                parameters.onItemSelected(item);
	            }
	        }
	    }

	    // Cuando se des-selecciona lo que hubiese.
	    function onUnselectedItem() {
	        currentIndex = -1; // reiniciar el elemento actual

	        // eliminar el estilo de seleccionado
	        Sfx.removeClass(control, 'selectedItem');
			
	        if (parameters && parameters.onUnselectedItem) {
	            parameters.onUnselectedItem();
	        }
	    }

	    function onRowClick(e, index) {
	        Sfx.cancelBubble(e);
	        Sfx.stopBubble(e);
	        currentIndex = index;
	        hideWindow();
	        onItemSelected();
	    }

	    function getItemText(item) {
	    	return item.text || item;
	    }

	    function loadData() {
	        if (url != null) {
	            if (ajaxCall) {
	                //ajaxCall.abort();
	                return;
	            }

	            // si ya hay programada una llamada no volver a hacerla
	            if (ajaxTimeout) {
	                return;
	            }

	            var time = new Date().getTime();

	            if (lastAjaxCall && time - lastAjaxCall < 1000) {
	                ajaxTimeout = window.setTimeout(makeAjaxCall, 1000);
	            }
	            else {
	                makeAjaxCall();

	                // marcar el momento de hacer la llamada al servidor
	                lastAjaxCall = time;
	            }

	        }
	        else {
	            var controlValue = sourceInput.value;
	            filterData(controlValue);
	            showWindow(controlValue);
	            dataSearchValue = controlValue;
	        }
	    }

	    function makeAjaxCall() {
	        ajaxTimeout = null;

	        var controlValue = sourceInput.value;

	        ajaxCall = Sfx.getJson({
	            url: url,
	            parameters: { searchText: controlValue, maxResults: maxRows },
	            success: function (result) {
	                rawData = result;

	                if (result.partialResults !== undefined) {
	                    partialResults = result.partialResults;
	                }

	                if (result.createRowFunc) {
	                    createRowFunc = result.createRowFunc;
	                }

	                filterData(controlValue);
	                showWindow(controlValue);
	                dataSearchValue = controlValue;
	                ajaxCall = null;
	            },
	            error: function (result) {
	                ajaxCall = null;
	            }
	        });
	    }

	    function filterData(search) {
	        filteredData = [];

	        if (rawData != null) {
	            var regEx = new RegExp(Sfx.removeAccents(search), "i");

	            var matches = 0;

	            for (var i = 0, l = rawData.length; i < l; i++) {
	                var item = rawData[i];

	                if (regEx.test(Sfx.removeAccents(getItemText(item)))) {
	                    filteredData.push(item);

	                    matches++;

	                    if (matches > maxRows) {
	                        break;
	                    }
	                }
	            }
	        }
	    }
	}

})();

