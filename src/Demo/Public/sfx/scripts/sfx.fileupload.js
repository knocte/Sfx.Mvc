
(function () {
    "use strict";
 
 	var MAX_FILE_SIZE = 300000000000;
 	//var MESSAGES_CONTAINER = "uploadMessage";
 	var FILE_SELECT = "uploadFileSelect"
 	
    if (!Sfx.isDefined(Sfx)) {
        Sfx = {};
    }
    
    Sfx.fileUpload = function (action, cropAction, target, width, height) {
    	if(action == null){
    		return; // si no hay action nada que hacer
    	}

    	// valores por defecto de tamaño si no se proporcionan
    	width = width | 200;
    	height = height | 200;
    	
    	var control = document.createElement("div");
    	document.getElementsByTagName('body')[0].appendChild(control);
    	
    	var messages = createMessages(control);
    	
    	var inputFile = createInputFile(control, action, target, messages, cropAction, width, height);
    	
    	var btnSubmit = createCloseButton();
    	
    	createProgressBar(control);
    	
    	var alertParams =  {'alertBody': control, 'buttons':[btnSubmit] }; 
    	
        Sfx.alert("", alertParams);
    }
    
    function createInputFile(control, action, target, messageContainer, cropAction, width, height){
    	
    	var div = document.createElement("div");
    	var inputFile = document.createElement('input');
        inputFile.setAttribute("type", "file");
        inputFile.setAttribute("id", FILE_SELECT);
        inputFile.setAttribute("name", FILE_SELECT);
        inputFile.setAttribute("multiple", "multiple");
        inputFile.onclick = function(){ cleanMessages(messageContainer);};
        inputFile.onchange = function(){
        	fileSelectHandler(action, target, inputFile, messageContainer, cropAction, width, height);
        }
        div.appendChild(inputFile);
        control.appendChild(div);
        return inputFile;
    }
    
    function createCloseButton(){
     	var button = {
            "onclick": function() { Sfx.closeAlert(); },
            "text": Sfx.t("Cancelar")
        };
        return button;
    }
    
    function createProgressBar(control){
    	var div = document.createElement("div");
    	div.setAttribute("id","progress");
    	control.appendChild(div);
    }
    
    function createMessages(control){
    	var div = document.createElement("div");
    	div.setAttribute("id","messagesUpload");
    	control.appendChild(div);
    	return div;
    }
    
    // file selection en onchange
	function fileSelectHandler(action, target, inputFile, messageContainer, cropAction, width, height) {
		cleanMessages(messageContainer);
		
		// fetch FileList object
		var files = inputFile.files;
		
		if(files.length == 0){
            Sfx.closeAlert();
		}

		// process all File objects
		for (var i = 0, f; f = files[i]; i++) {
			UploadFile(f, action, target, messageContainer, cropAction, width, height);
		}
	}
    
    function UploadFile(file, action, target, messageContainer, cropAction, width, height) {
		var xhr = new XMLHttpRequest();
		
		if(file.size > MAX_FILE_SIZE) {
			messageContainer.innerHTML = Sfx.t("Archivo demasiado grande");
			return;
		}
		
		if (xhr.upload) {
			// create progress bar
			var o = document.getElementById("progress");
			var progress = o.appendChild(document.createElement("p"));
			
    		// progress bar
			xhr.upload.addEventListener("progress", function(e) {
				var pc = parseInt(100 - (e.loaded / e.total * 100), 10);
				progress.style.backgroundPosition = pc + "% 0";
			}, false);
			// file received/failed
			xhr.onreadystatechange = function(e) {
				var state = xhr.readyState;
				var response = xhr.responseText;
				var status = state == 4? xhr.status : "";
				onCompleted(state, response, status, target, messageContainer, cropAction, width, height);
			};

			xhr.open("POST", action, true);
			xhr.setRequestHeader("X_FILENAME", file.name);
			// start upload
			var formData = new FormData();
			formData.append(file.name, file);
			xhr.send(formData);
		}
	}
	
	function onCompleted(readyState, responseText, status, target, messageContainer, cropAction, width, height){
		if (readyState == 4) {
			progress.className = (status == 200 ? "success" : "failure");
			if(status != 200) {
				messageContainer.className = "failure";
				messageContainer.innerHTML = responseText;
				return;
			}
			Sfx.closeAlert();
			var targetInput = document.getElementById(target);
			if(cropAction && isImage(responseText)) {
				// abrir el cropper
				loadCropper(responseText, targetInput, cropAction, width, height);
			} else {
				if(targetInput != null) {
					// poner el resultado en el textbox
					targetInput.value = responseText;
				}
			}
		}
    }	
     
	function cleanMessages(messageContainer){
		Sfx.clearChildNodes(messageContainer);
	}
	
	function isImage(name) {
	    var parts = name.split('.');
	    var ext =  parts[parts.length - 1];
	    switch (ext.toLowerCase()) {
		    case 'jpg':
		    case 'gif':
		    case 'bmp':
		    case 'png':
		        return true;
		    }
	    return false;
	}
	
	//crear el croper en un modal
	function loadCropper(responseText, targetInput, cropAction, width, height) {
		
		var cropperDiv = createCropperContaner();
		
		var ic = new ICropper('cropper', {
								keepSquare: false,
								image: responseText
							 });
							
		var btnSubmit = {
			"onclick": function(){ 
				resizeImage(ic.getInfo(),targetInput, responseText, cropAction, width, height);
			},
            "text": Sfx.t("Guardar")
     	};
		
		var btnCancel = {
			"onclick": function(){ 
				Sfx.closeAlert()
			}, 
            "text": Sfx.t("Cancelar")
     	};
		
		var alertParams =  {
			'alertBody': cropperDiv, 
			'buttons':[btnSubmit,btnCancel] 
		}; 
    	
        Sfx.alert("",alertParams);
	}
	
	// petición ajax para recortar la imagen con los parametros del crop
	function resizeImage(info, targetInput, fileName, cropAction, width, height) {
		 var msg = document.getElementById("cropperMessage");
		 
		 new Sfx.postAjax({
		    url: cropAction, 
		    parameters: {"info" : info, "fileName" : fileName, "width": width, "height": height},
		    success: function(result){ 
		    	msg.innerHTML = result;
		    	msg.className = "success";
		    	targetInput.value = fileName;
                setTimeout(function() { Sfx.closeAlert();}, 1000);
		    	
		    },
		    error: function(result) { 
		    	msg.innerHTML = result.message;	
		    	msg.className = "error";
		    }
		 }); 
	}
	
	// crea el div del crop
	function createCropperContaner () {
		// contenedor principal
		var cropperContainer = document.createElement("div");
		cropperContainer.id = "cropperContainer";
		
		// div para el crop
		var cropper = document.createElement("div");
		cropper.id = "cropper";
		cropperContainer.appendChild(cropper);
		
		// span para mensajes
		var span = document.createElement("span");
		span.id = "cropperMessage";
		cropperContainer.appendChild(span);
		
		var body = document.getElementsByTagName("body")[0];
		body.appendChild(cropperContainer);
		return cropperContainer;
	}

})();