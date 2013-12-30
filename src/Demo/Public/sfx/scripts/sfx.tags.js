
(function () {
    "use strict";

    if (!Sfx.isDefined(Sfx)) {
        Sfx = {};
    }

	Sfx.tags = function (container, parameters) {
		container = document.getElementById(container);
		if(container == null){
			return;
		}
		
		if(parameters == null) {
			paremeters = {};
		}
		
		var form = createForm(container);
		var list = createList(form);
		var textBox = createTextBox(list, form);
		appendTextBox(textBox, list, parameters,  form.id);
		createSelectedInput(form); // se guardan los ids seleccionados en el value de este input hidden 
	}

	// crea el formulario de envio
	function createForm(container) { 
		var form = document.createElement("form");
		form.setAttribute("id", container.id + "Form");
		form.setAttribute("method", "post");
		container.appendChild(form);	
		return form;	
	}	
	
	// crea una lista que engloba los tags
	function createList(container) {
		var ul = document.createElement("ul");
		ul.className = "choices";
		container.appendChild(ul);
		return ul;
	}
	
	// crea el texbox 
	function createTextBox(list, container) {
		var input = document.createElement("input");
		input.setAttribute("type","text");
		input.setAttribute("id", container.id + "Txt");
		var containerSize = Sfx.getSize(container);
		input.style.width = "35px";
		input.className = "txtBox";
		input.onkeyup = function(e) {
			resizeTextBox(e,input)
		}
		return input;
	}
	
	// se añade el textbox a la lista 
	function appendTextBox(input, list, parameters , formId) {
		var li = document.createElement("li");
		li.className = "selectBox";
		li.appendChild(input);
		insertListItem(list, 0, li);
		list.onclick = function() {
			input.focus();
		}		
		appendAutosuggest(input, list, parameters, formId);
	}
	
	// crea el hidden que contiene los ids seleccionados
	function createSelectedInput(form) {
		var idField = document.createElement("input");
        idField.type = "hidden";
        idField.id = form.id + "Values"; 
        form.appendChild(idField);
	}
	
	// añade un tag a la lista ( se llama cuando se hace click en las sugerencias )
	function addTag(item, ul, formId) {
		var id = item.id;
	 	var li = document.createElement("li");
	 	li.className = "choice";
		var span = document.createElement("span");
		li.innerText = item.text;
		var close = document.createElement("a");
		close.setAttribute("href", "#");
		close.className = "close";
		close.setAttribute("rel", id) // --> esto tiene que llevar el id al que hacer ref
		close.onclick = function(){
			deleteTag(close, id, formId);
		}
		li.appendChild(span);
		li.appendChild(close);
		if(addValue(id, formId)) { // si no existe lo añade
			insertListItem(ul, "", li);
		}
	}
	
	function deleteTag(aClose, id, formId) {
		if(aClose != null) {
			 aClose.parentNode.parentNode.removeChild(aClose.parentNode);
			 deleteValue(id, formId);
		}
	}
	
	// borra el último tag
	function deleteLastTag(input) {
		// el último tag es el li inmediatamente superior al input y el input es siempre
		// el último elemento de la lista por tanto el tag es el ul.length - 2
		var formId = input.form.id;
		var ul = input.parentNode.parentElement;
		var tag = ul.getElementsByTagName("li")[ul.childNodes.length -2];
		// el id va en el rel del href del tag
		var aClose = tag.getElementsByTagName("a")[0];
		var id = aClose.getAttribute("rel");
		deleteTag(aClose, id, formId);
	}
	
	// guarda el id en el hidden. Si ya existe no guarda nada
	function addValue(id, formId) {
		var okAdd = true;
		var selectedInput = document.getElementById(formId + "Values");
		if(selectedInput == null) {
			return false;
		}
		var selectedValues = selectedInput.value;
		var values = selectedValues.split(',');
		Sfx.each(values, function (item) {
			if(item == id) {
				okAdd = false;
			}
		});
			
		if(okAdd) {
			selectedInput.value = selectedValues == "" ? id : selectedValues + "," + id;
		}
		return okAdd;
	}
	
	function deleteValue(id, formId) {
		var selectedInput = document.getElementById(formId + "Values");
		if(selectedInput == null) {
			return;
		}
		var selectedValues = selectedInput.value;
		var values = selectedValues.split(',');
		for (var i = 0; i< values.length; i++) {
			if(values[i] == id) {
				values.splice(i, 1);
				break;
			}
		}
		var result = "";
		Sfx.each(values, function (item) {
			result = result == "" ? item : result  +  "," + item;
		}); 
		selectedInput.value = result;
	}
	
	// inserta un li en la primera posición o justo antes del último li
	function insertListItem(list, position, listItem) {
		if(position == "") {
			position = list.childElementCount - 1 ;
		}
	    list.insertBefore(listItem, list.getElementsByTagName("li")[position]);
    }
    
    // cada vez que se escribe en el textbox se hace un poco más grande
    function resizeTextBox(e, textBox) {
        Sfx.cancelBubble(e);
	    Sfx.stopBubble(e);
	    var txtWidth =  Sfx.getSize(textBox);
	    var newTxtWidth = 0;
	    var keyCode = e.which ? e.which : e.keyCode
	    if(keyCode == 8 ) { // Del, borrar último tag
	    	deleteLastTag(textBox);
	    	return;
	    }
	    
	    if( keyCode == 46){ // Supr, disminuir tamaño
	    	newTxtWidth = txtWidth.width - 3;	
	    } else if(isTextCode(keyCode)) { // si son letras o numeros aumentamos
	    	newTxtWidth = txtWidth.width + 3;
	    }
	    
    	if(newTxtWidth != 0) {
    		textBox.style.width = newTxtWidth + "px";
    	}
	}
	
	function appendAutosuggest(input, list, parameters, formId) {
		// establecer la acción cuando se seleccione un elemento de la lista
		parameters.onItemSelected = function(item){
    		addTag(item, list, formId);
    		input.value = "";
    		input.focus();
		};
		//meter el autosuggest a este input
		 Sfx.autosuggest(input.id,parameters);
	}
	
	function isTextCode(charCode) {
	   if ((charCode >= 48 && charCode <= 57) || // 0-9
           (charCode >= 65 && charCode <= 90) || // A-Z
           (charCode >= 97 && charCode <= 122)) { // a-z
        	 return true;
        }
        return false;
	}
	
})();

