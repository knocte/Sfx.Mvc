
(function () {
    "use strict";
 
    if (!Sfx.isDefined(Sfx)) {
        Sfx = {};
    }
    
    var PROPERTY = "searchProperty_";
 	var VALUE = "searchValue_";
 	var COMPARER = "searchComprarer_";
 	var OPERATOR = "operator_";
 	var rowCounter = 0;
    
    Sfx.showSearch = function (control, data) {
  	 	
		var container = document.getElementById(control);
		
		if(container == null){
			return;// no existe el contenedor
		}
        
        // si ya se esta mostrando, cerrar
        Sfx.clearChildNodes(container);
        
        // crear el div de las filas
        var rowsContainer = document.createElement("div");
        container.appendChild(rowsContainer);
        
        // añadir el primer row
        appendNewRow(rowsContainer, true, data);
		
		//añadir botones de + y submit
	 	createActionButtons(container, data, rowsContainer);
	}
	
	
	// Añade una fila al formulario
	function appendNewRow(container, isFirstRow, data) {
		
		var index = rowCounter;
        rowCounter++;
        
        var rowContainer = document.createElement("div");
  		
        if (!isFirstRow) 
        {	// si no es el primer row añadir el operador y/o
           createOperator(rowContainer, index);
        }
        else
        {   // dejar el hueco del operador lógico
        	createEmptyDiv(rowContainer);
        }
        // las propiedades
        createSearchPropery(data, rowContainer, index);
      
  	    // botón de borrar x
        createDeleteButton(rowContainer, index);
        
        container.appendChild(rowContainer);
 	}
 	
 	// los botones de enviar y añadir fila    
	function createActionButtons(container, data, rowsContainer) {
		var buttonsPanel = document.createElement("div");
        buttonsPanel.className = "searchButtons";
        container.appendChild(buttonsPanel);

        var btnAddRow = document.createElement('input');
        btnAddRow.setAttribute("type", "button");
        btnAddRow.setAttribute("value", "+");
        btnAddRow.onclick = function () { appendNewRow(rowsContainer, false, data); };
        buttonsPanel.appendChild(btnAddRow);

        var submitButton = document.createElement('input');
        submitButton.setAttribute("type", "submit");
        submitButton.setAttribute("value", Sfx.t("Buscar"));
        buttonsPanel.appendChild(submitButton);
	}
 	
 	// y/o
 	function createOperator(rowContainer, index) {
 		var logicalOperator = document.createElement("select");
        var logicalOperatorName = OPERATOR + index;
        logicalOperator.setAttribute("id", logicalOperatorName);
        logicalOperator.setAttribute("name", logicalOperatorName);
        logicalOperator.className = "logicalOperator";
        Sfx.addToSelect(logicalOperator, " y ", "and");
        Sfx.addToSelect(logicalOperator, " o ", "or");
        rowContainer.appendChild(logicalOperator);
 	}
 	
 	// div vacío para el hueco del y/o de la primera fila
 	function createEmptyDiv(rowContainer) {
 		var emtpyDiv = document.createElement("div");
    	emtpyDiv.className = "emptyLogicalOperator";
    	rowContainer.appendChild(emtpyDiv);
 	}
 	
 	// botón X
 	function createDeleteButton(rowContainer, index) {
 		var deleteRowButton = document.createElement('input');
        deleteRowButton.setAttribute("type", "button");
        deleteRowButton.setAttribute("value", "x");
        deleteRowButton.onclick = function () { deleteFilter(deleteRowButton,index); };
        rowContainer.appendChild(deleteRowButton);
 	}
 	
 	// elimina un linea del filtro
 	function deleteFilter(deleteRowButton,index) {
 		var search = document.getElementById(PROPERTY + index);
 		if(search != null) {
            Sfx.clearChildNodes(search.parentNode.parentNode);
 		}
 	}
 	
 	// crea el panel/select con las propiedades
 	function createSearchPropery(data, rowContainer, index) {
 	    // el select con todas las propiedades ( luego será una ventanita o algo guai)
        // lo meto dentro de un div para poder añadirle el comparador y el valor
        // y para poder darle diseño al div cuando deje de ser un select
        var divSelectContainer = document.createElement("div");
        divSelectContainer.className = "searchContainer";
		var select = document.createElement("select");
    	select.setAttribute("name", PROPERTY + index);
    	select.setAttribute("id", PROPERTY + index);
    	select.className = "search";
        Sfx.addToSelect(select, "", "");
    	
    	for (var key in data) {
            Sfx.addToSelect(select, data[key].label, key);
		};
		
		divSelectContainer.appendChild(select)
  	    rowContainer.appendChild(divSelectContainer);
        Sfx.attach(select, "change", function(){
			createSearchComparer(index, data);
		});
		
 	}
 	
 	// muestra las opciones de comparación en función del tipo de dato seleccionado
 	function createSearchComparer(rowIndex, data) {
        
        var propertySelect = document.getElementById(PROPERTY + rowIndex);
		if(propertySelect == null) {
			return;
		}
        
        var property = data[propertySelect.value];
        if (property == null) {
            return;
        }
        
        var container = propertySelect.parentNode;
		
		// el select del comparador
        var comparerSelect = document.getElementById(COMPARER + rowIndex);
        if(comparerSelect != null) {
            Sfx.clearChildNodes(comparerSelect);
        } else {
        	comparerSelect = createSelectComparer(container, rowIndex);//si no existe se crea
        }
       
        switch (property.type) {
            case "Int":
            case "Decimal":
            case "Currency":
            case "Percent":
            case "Time":
            case "Date":
            case "DateTime":
                Sfx.addToSelect(comparerSelect, Sfx.t("igual"), "=");
                Sfx.addToSelect(comparerSelect, Sfx.t("distinto"), "!=");
                Sfx.addToSelect(comparerSelect, Sfx.t("mayor"), ">");
                Sfx.addToSelect(comparerSelect, Sfx.t("mayor o igual"), ">=");
                Sfx.addToSelect(comparerSelect, Sfx.t("menor"), "<");
                Sfx.addToSelect(comparerSelect, Sfx.t("menor o igual"), "<=");
                break;

            default:
                Sfx.addToSelect(comparerSelect, Sfx.t("igual"), "=");
                Sfx.addToSelect(comparerSelect, Sfx.t("distinto"), "!=");
                break;
       }
       
       createSearchBox(property, container, rowIndex);
	}
	
	// crea el select vacío con las opciones de comparación
	function createSelectComparer(container, rowIndex){
		var comparerSelect = document.createElement("select");
        var comparerSelectName = COMPARER + rowIndex;
        comparerSelect.setAttribute("id", comparerSelectName);
        comparerSelect.setAttribute("name", comparerSelectName);
        comparerSelect.className = "search";
        container.appendChild(comparerSelect);
        return comparerSelect;
	}

	// crea un control (de distintos tipos) para los valores de la búsqueda
	// en función de la propiedad seleccionada
	function createSearchBox(property, container, rowIndex) {
        var oldTextBox = document.getElementById(VALUE + rowIndex);
        if(oldTextBox != null) {
        	oldTextBox.parentNode.removeChild(oldTextBox);
        }
		
		var textboxName = VALUE + rowIndex;
		
        switch (property.type) {
            case "Date":
            case "DateTime":
            	var textbox = createTextBox(container, textboxName, rowIndex);
                textbox.onclick = function () { Sfx.showPickCalendar(textbox); };
                textbox.className = "datepicker searchText";
                break;
            case "Reference":
            	createTextBox(container, textboxName, rowIndex);
                Sfx.autosuggest(textboxName, property);
                break;
            case "Bool":
            	createBoolSelect(container, rowIndex);
            	break;
            case "Picklist":
            	createPicklistSelect(container, rowIndex, property);
            	break;
           
            default:
            	createTextBox(container, textboxName, rowIndex);
        }
	}
	
	// control campo de texto normal
	function createTextBox(container, textboxName, rowIndex){
	 	var oldTextBox = document.getElementById(VALUE + rowIndex);
        if(oldTextBox != null) {
        	oldTextBox.parentNode.removeChild(oldTextBox);
        }
        var textbox = document.createElement("input");
        textbox.setAttribute("type", "text");
        textbox.setAttribute("id", textboxName);
        textbox.setAttribute("name", textboxName);
        textbox.setAttribute("autocomplete", "off");
        textbox.className = "searchText";
        container.appendChild(textbox);
        return textbox;
	}
	
	// control tipo select con si/no
	function createBoolSelect(container, rowIndex) {
        var oldTextBox = document.getElementById(VALUE + rowIndex);
        if(oldTextBox != null){
        	oldTextBox.parentNode.removeChild(oldTextBox);
        }

        var boolSelect = document.createElement("select");
        var boolSelectName = VALUE + rowIndex;
        boolSelect.setAttribute("id", boolSelectName);
        boolSelect.setAttribute("name", boolSelectName);
        boolSelect.className = "searchValue";
        Sfx.addToSelect(boolSelect, "si", "1");
        Sfx.addToSelect(boolSelect, "no", "0");
        container.appendChild(boolSelect);
    }
    
    // control tipo picklist
    function createPicklistSelect(container, rowIndex, property) {
        var oldTextBox = document.getElementById(VALUE + rowIndex);
        if(oldTextBox != null){
        	oldTextBox.parentNode.removeChild(oldTextBox);
        }

        var select = document.createElement("select");
        var selectName = VALUE + rowIndex;
        select.setAttribute("id", selectName);
        select.setAttribute("name", selectName);
		select.className = "searchValue";
        Sfx.each(property.picklistValues, function (item) {
            Sfx.addToSelect(select, item.label, item.id);
        });

        container.appendChild(select);
    }
	    	
})();

