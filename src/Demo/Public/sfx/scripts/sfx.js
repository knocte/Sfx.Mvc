/**
 @preserve (c) 1999-2013 Santiago Corredoira Lascaray .
 Base Functions for Sfx Framework.
 Version: 2.

 For image cropping includes icropper https://github.com/supnate/icropper
 icropper license (MIT) https://github.com/supnate/icropper/blob/master/LICENSE)
*/

if (typeof Sfx === 'undefined') {

	// Librería base Syltek.
    // ---------------------
	// Define las funciones básicas que son utilizadas por los demás módulos.
	// Copyright 1999 - 2013 Santiago Corredoira Lascaray.
    var Sfx = {};
}

(function () {   
    "use strict";
       
    // Añade una traza en la consola de depuración.
    Sfx.log = function () {
        var line = "";
        for (var i = 0; i < arguments.length; i++) {
            var item = arguments[i];
            if(!Sfx.isValueType(item)){
                item = JSON.stringify(item);
            }
            line += item + "    ";
        }
        line += "\n";
        console.log(line);
    };       

    // Para traducciones.
    Sfx.getText = function(value) {
        return value;
    } 

    // las variables de entorno, como por ejemplo la cultura.
    var environment = {};
        
    // Devuelve el valor de la variable de entorno del objeto Sfx.
    Sfx.getEnv = function (key) {
        return environment[key];
    };
        
    // Establece el valor de la variable de entorno del objeto Sfx.
    Sfx.setEnv = function (key, value) {
        environment[key] = value;
    };
        
    // Traduce el texto al lenguaje especificado en Sfx.env.culture
    Sfx.t = function (text) {
        return text
    };
    
    // Parseint no siempre asume que es en base 10. Si va precedido de un cero pasan cosas raras.
    // https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/parseInt
    // http://stackoverflow.com/a/8763427/4264
    Sfx.parseInt = function(value) {
    	return parseInt(value, 10);
    };
    
       Sfx.parseDate = function (value, format) {
        var parts = Sfx.splitDate(value);

        if(format){
            var formatParts = Sfx.splitDate(format);
            var year = getDatePart(["yyyy"], formatParts, parts);
            var month = getDatePart(["MM", "M"], formatParts, parts);
            var day = getDatePart(["dd", "d"], formatParts, parts);
            return new Date(year, month - 1, day); // months are 0-based
        }
        else {
            var parts = value.match(/(\d+)/g);
            if(parts[0].length == 4){
                return new Date(parts[0], parts[1]-1, parts[2]); // months are 0-based
            } else {
                return new Date(parts[2], parts[1]-1, parts[0]); // months are 0-based
            }
        }
    };

    function getDatePart(formats, formatParts, parts) {
        for(var i=0; i < formats.length; i++) {
            var index = Sfx.indexOf(formatParts, formats[i]);
            if(index > -1) {
                return parseInt(parts[index], 10);
            }
        }
    }

    //  Formatea la fecha deacuerdo format o con Sfx.env.culture.
    //  Valores válidos para format:
    //
    //  * d o dd: el día
    //  * MM: el mes
    //  * yyyy: el año
    Sfx.formatDate = function (date, format) {
        if(!format){
            format = Sfx.getEnv("dateFormat") || "yyyy/M/d";
        }

        var result = format;
        result = result.replace("yyyy", date.getFullYear());

        if(format.indexOf("MM") != -1) {
            result = result.replace("MM", date.getMonth() + 1);
        }
        else if(format.indexOf("M") != -1) {
            result = result.replace("M", date.getMonth() + 1);
        }
        
        var day = date.getDate();
            
        if(format.indexOf("dd") != -1) {
	        if(day < 10){
	            day = "0" + day;
	        }
            result = result.replace("dd", day);
        }
        else if(format.indexOf("d") != -1) {
            result = result.replace("d", day);
        }

        return result;
    };
    
    // Compara el día, mes y año de dos fechas.
    Sfx.compareDatePart = function(a, b){
    	var aYear = a.getFullYear();
    	var bYear = b.getFullYear();
            if(aYear > bYear){
    	    return -1;
    	} else if(aYear < bYear){
    	    return 1;
    	}
    	
    	var aMonth = a.getMonth();
    	var bMonth = b.getMonth();
            if(aMonth > bMonth){
    	    return -1;
    	} else if(aMonth < bMonth){
    	    return 1;
    	}
    	
    	var aDay = a.getDate();
    	var bDay = b.getDate();
            if(aDay > bDay){
    	    return -1;
    	} else if(aDay < bDay){
    	    return 1;
    	}	
    	
    	return 0;
    };

    // Devuelve un nuevo objeto date igual que el que recibe.
    Sfx.copyDate = function (date) {
        var d = new Date();
        d.setTime(date.valueOf());
        return d;
    };
        
    //Añade los días especificados a la fecha.
    Sfx.addDays = function (date, days) {
        date.setDate(date.getDate() + days);
    };
    
    
    // Añade los meses especificados a la fecha.
	Sfx.addMonths = function(date, months) {
		var monthsTotal = date.getMonth() + (date.getFullYear() * 12) + months;
		var day = date.getDate();
		var year = Math.floor(monthsTotal / 12);
		var month = monthsTotal % 12;

		var maxDays = Sfx.getDaysInMonth( year, month );
		if( day > maxDays ){
			// no sobrepasar el max de días en el mes.
			day = maxDays;
		}

		date.setYear( year );
		date.setDate( day );
		date.setMonth( month );
	};

	Sfx.getDaysInMonth = function(year, month){
		var days = [ 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 ];

		//	Check for leap year
		if( month == 1 && year % 4 == 0 && ( year % 100 || year % 400 == 0 ) ){
			return 29;
		}

		return days[month];
	};


    Sfx.isDefined = function (o) {
        return typeof (o) !== 'undefined';
    };

    Sfx.isString = function (o) {
        return typeof (o) === 'string';
    };

    Sfx.isNumber = function (o) {
        return typeof (o) === 'number' && isFinite(o);
    };
        
    Sfx.isEmpty = function (value) {
        return value === null || value === '';
    };

    Sfx.isFunction = function (o) {
        return typeof (o) === 'function';
    };

    Sfx.isArray = function (o) {
        if (o) {
            return Sfx.isNumber(o.length) && Sfx.isFunction(o.splice);
        }
        return false;
    };

    // Devuelve un objeto serializado en JSON formateado.
    Sfx.stringify = function (value) {
        return JSON.stringify(value, true, 4);
    };
     
    // Devuelve true si el objeto es de un tipo number, string o boolean.
    Sfx.isValueType = function (value) {
        switch (typeof (value)) {
            case "number":
            case "string":
            case "boolean":
                return true;
        }
        return false;
    };


    /**
        @title endsWith           
        Devuelve true si str acaba en suffix.
    */
    Sfx.endsWith = function (str, suffix) {
        return str.indexOf(suffix, str.length - suffix.length) !== -1;
    };


    /**
        @title round  
        Redondea un número al número de decimales especificado.
    */
    Sfx.round = function (number, decimals) {
        return Math.round(number * Math.pow(10, decimals)) / Math.pow(10, decimals);
    };
    
    /**
        @title indexOf  
        Devuelve el índice que ocupa el elemento en el array. -1 si no existe.
    */
    Sfx.indexOf = function (array, element) {
        for (var i = 0, l = array.length; i < l; i++) {
            if (array[i] === element) {
                return i;
            }
        }
        return -1;
    };
    
    // Elimina el elemento del array tantas veces como ocurra
    // http://stackoverflow.com/a/10517081
    Sfx.removeFromArray = function (array, element){
		while (array.indexOf(element) !== -1) {
		    array.splice(array.indexOf(element), 1);
		}
    };
    
    Sfx.each = function (collection, functionName) {
        for (var i = 0, l = collection.length; i < l; i++) {
            functionName(collection[i]);
        }
    };

    Sfx.isIpadOrIphone = function () {
        var userAgent = navigator.userAgent;
        return userAgent.match(/iPad/i) || userAgent.match(/iPhone/i) || userAgent.match(/iPod/i);
    };

    Sfx.isIExplorer = function () {
        return navigator.appVersion.indexOf("MSIE") !== -1;
    };
        
    /**
        @title getUrlValue  
        Devuelve una variable del querystring.
    */
    Sfx.getUrlValue = function (variable) {
        var query = window.location.search.substring(1);
        var vars = query.split("&");
        for (var i = 0, l = vars.length; i < l; i++) {
            var indexOfToken = vars[i].indexOf('=');
            if (vars[i].substring(0, indexOfToken) === variable) {
                return vars[i].substring(indexOfToken + 1);
            }
        }
        return null;
    };

    /**
        @title setHash  
        Establece la parte hash (desde el #) de la url.
    */
    Sfx.setHash = function(hash) {
        window.location.hash = hash;
    };

    /**
        @title getHash  
        Obtiene la parte hash (desde el #) de la url.
    */
    Sfx.getHash = function() {
        var hashStr = window.location.hash;
        return hashStr.substring(1, hashStr.length);
    };
                
    /**
        @title load  
        Carga uno o varios scripts u hojas de estilo dinámicamente.
        En caso de que el archivo ya esté cargado no lo añade 2 veces.

        @example
         Sfx.load([
                "/scripts/s/s.dom.js",
                "/scripts/s/s.events.js",
                "/scripts/s/s.routing.js",
                "/scripts/s/s.ajax.js",
                "/scripts/s/s.UI.js",
                "/scripts/s/s.UI.css",
                "/scripts/s/s.UI.grid.js",
                "/scripts/s/s.UI.grid.css",
                "/scripts/s/s.UI.detail.js",
                "/scripts/s/s.UI.detail.css",
                "/scripts/s/s.app.js",
                "/scripts/demo.js"
            ], function () { init() });
    */
    Sfx.load = function(files, callback) {
        if(!Sfx.isArray(files)) {
            files = [files];
        }        
        loadFiles(files, callback);
    };

    // carga un conjunto de archivos en la cabecera y ejecuta callback cuando
    // están todos listos.
    function loadFiles(files, callback) {
        var loadCount = 0;

        // Se llama cada vez que se carga uno de los archivos.
        // Cuando se han cargado todos se llama al callback.
        function loadedCallback() {
            loadCount++;
            if(callback && loadCount === files.length){
                callback();
            }
        }

        for(var i=0, l = files.length; i < l; i++) {
              loadFile(files[i], loadedCallback);
        }
    }

    // Carga un archivo en la cabecera.
    // Si ya está cargado llama al callback directamente
    function loadFile(fileName, onLoaded){
        if(Sfx.indexOf(loadedFiles, fileName) !== -1){
            onLoaded();
            return; // ya está cargado
        }

        var fileref = null;
        
        if(Sfx.endsWith(fileName, "js")){
            fileref=document.createElement('script');
            fileref.setAttribute("type","text/javascript");
            fileref.setAttribute("src", fileName);
            document.getElementsByTagName("head")[0].appendChild(fileref);
        }
        else if(Sfx.endsWith(fileName, "css")){
            fileref=document.createElement("link");
            fileref.setAttribute("rel", "stylesheet");
            fileref.setAttribute("type", "text/css");
            fileref.setAttribute("href", fileName);
        }

        if(fileref !== null) {
            fileref.onload = onLoaded;
            // IE 6 & 7
            fileref.onreadystatechange = function() {
                if (this.readyState === 'loaded' || this.readyState === 'complete') {
                    onLoaded();
                }
            };

            document.getElementsByTagName("head")[0].appendChild(fileref);
            loadedFiles.push(fileName);
        }
    }
            
    // para llevar la cuenta de los archivos cargados y no repetir.
    var loadedFiles = [];



    /**
        @title apply  
        Ejecuta una función a partir del nombre.
        Ver: http://stackoverflow.com/questions/359788
    */
    Sfx.apply = function (functionName /*, args */) {
      var context = window;
      var args = Array.prototype.slice.call(arguments).splice(2);
      var namespaces = functionName.split(".");
      var func = namespaces.pop();
      for(var i = 0; i < namespaces.length; i++) {
        context = context[namespaces[i]];
      }
      return context[func].apply(this, args);
    };


    Sfx.post = function (url, parameters) {
            var tempForm = document.createElement("form");
            document.documentElement.appendChild(tempForm);
            tempForm.method = "post";
            tempForm.action = url;

            if (parameters) {
                for (var parameter in parameters) {
                    var input = document.createElement("input");
                    input.type = "hidden";
                    input.name = parameter;
                    input.value = parameters[parameter];
                    tempForm.appendChild(input);
                }
            }

            tempForm.submit();
            document.documentElement.removeChild(tempForm);
    };
    
    // El objeto error que se utiliza en todo el framework
    function Error(message) {
        this.message = message;
        this.toString = function () {
            return this.message;
        };
    }


    Sfx.removeAccents = function (s) {
            var r = s.toLowerCase();
            r = r.replace(new RegExp("[àáâãäå]", 'g'), "a");
            r = r.replace(new RegExp("æ", 'g'), "ae");
            r = r.replace(new RegExp("[èéêë]", 'g'), "e");
            r = r.replace(new RegExp("[ìíîï]", 'g'), "i");
            r = r.replace(new RegExp("[òóôõö]", 'g'), "o");
            r = r.replace(new RegExp("œ", 'g'), "oe");
            r = r.replace(new RegExp("[ùúûü]", 'g'), "u");
            r = r.replace(new RegExp("[ýÿ]", 'g'), "y");
            return r;
    }
    
    // Añade a un control select un nuevo elemento option.
    Sfx.addToSelect = function (select, text, value, isSelected) {
        select.appendChild(Sfx.createOption(text, value, isSelected));
    }
    
    
    // Genera un elemento option para un control select.
    Sfx.createOption = function (text, value, isSelected) {
        var option = document.createElement("option");

        Sfx.setText(option, text);
        option.value = value;

        if (isSelected) {
            option.selected = true;
        }

        return option;
   }
    
            
})();
   


