
/**
    @title Ajax
    @summary Define las funciones para realizar peticiones Ajax.
*/

(function () {
    "use strict";

    /**
    @title getAjax
    @signature new Sfx.getAjax (parameters);
    Realiza una petición ajax por el método GET.
        	
    @example

    new Sfx.getAjax({
    url:"/program/rules",
    parameters: {},
    success: function(result){ 
    alert("OK!: " + result); 
    },
    error: function(result) { 
    alert("Error: " + JSON.stringify(result)); 
    }
    }); 
    */
    Sfx.getAjax = function (parameters) {
        new ajax().getAjax(parameters, "GET", false);
    }

    /**
    @title postAjax
    @signature new Sfx.postAjax (parameters);
    Realiza una petición ajax por el método POST.
        	
    @example

    new Sfx.postAjax({
    url:"/program/rules",
    parameters: {},
    success: function(result){ 
    alert("OK!: " + result); 
    },
    error: function(result) { 
    alert("Error: " + JSON.stringify(result)); 
    }
    }); 
    */
    Sfx.postAjax = function (parameters) {
        new ajax().getAjax(parameters, "POST", false);
    }

    /**
    @title getJson
    @signature new Sfx.getJson (parameters);
    Realiza una petición ajax por el método GET y devuelve el resultado en formato Json.
        	
    @example

    new Sfx.getJson({
    url:"/program/rules",
    parameters: {},
    success: function(result){ 
    alert("OK!: " + result); 
    },
    error: function(result) { 
    alert("Error: " + JSON.stringify(result)); 
    }
    }); 
    */
    Sfx.getJson = function (parameters) {
        new ajax().getAjax(parameters, "GET", true);
    }

    /**
    @title postJson
    @signature new Sfx.getJson (parameters);
    Realiza una petición ajax por el método POST y devuelve el resultado en formato Json.
        	
    @example

    new Sfx.postJson({
    url:"/program/rules",
    parameters: {},
    success: function(result){ 
    alert("OK!: " + result); 
    },
    error: function(result) { 
    alert("Error: " + JSON.stringify(result)); 
    }
    }); 
    */
    Sfx.postJson = function (parameters) {
        new ajax().getAjax(parameters, "POST", true);
    }

    var ajax = function () {
        var instance = this;
        this.http = null;
        this.url = null;
        this.method = null;
        this.evalResult = false;
        this.parameters = null;
        this.disableCache = null;
        this.serializedParameters = null;
        this.successCallback = null;
        this.errorCallback = null;
        this.errorMessage = "";
        this.async = true;
        this.contentType = "application/x-www-form-urlencoded";
        this.headers = {};
        this.aborted = false;

        var serializeParameter = function (value) {
            var result = [];

            function add(k, v) {
                v = (v == null || v == undefined) ? "" : v;
                result[result.length] = encodeURIComponent(k) + '=' + encodeURIComponent(v);
            };

            // If an array was passed in, assume that it is an array of form elements.
            if (Sfx.isArray(value)) {
                Sfx.each(value, function (item) {
                    add(item.name, item.value);
                });
            } else {
                for (var property in value) {
                    var v = value[property];
                    if (Sfx.isValueType(v)) {
                        add(property, v);
                    } else {
                        add(property, JSON.stringify(v));
                    }
                }
            }

            return result.join("&").replace(/%20/g, "+");
        };

        var getXmlHttp = function () {
            var http = null;

            if (typeof (XMLHttpRequest) != "undefined") {
                http = new XMLHttpRequest();
            }
            else {
                try {
                    http = new ActiveXObject("Msxml2.XMLHTTP");
                }
                catch (e) {
                    try {
                        http = new ActiveXObject("Microsoft.XMLHTTP");
                    }
                    catch (e2) { }
                }
            }
            return http;
        };

        this.abort = function () {
            if (!instance.aborted) {
                instance.aborted = true;

                if (instance.http && instance.http.abort) {
                    instance.http.abort();
                }
            }
        }

        this.appendHeader = function (header, value) {
            instance.headers[header] = value;
        };

        this.getAjax = function (ajaxParameters, method, evalResult) {
            instance.url = ajaxParameters.url;
            instance.parameters = ajaxParameters.parameters;
            instance.successCallback = ajaxParameters.success;
            instance.errorCallback = ajaxParameters.error;
            instance.disableCache = ajaxParameters.disableCache | true;
            instance.method = method;
            instance.evalResult = evalResult;
            instance.async = ajaxParameters.async;
            instance.sendRequest();
            return instance;
        };

        this.sendRequest = function () {
            try {
                instance.aborted = false;

                instance.http = getXmlHttp();

                if (instance.http == null) {
                    instance.errorMessage = "Couldn't create XmlHttp Object";
                    return false;
                }

                if (instance.successCallback || instance.errorCallback) {
                    instance.http.onreadystatechange = instance.onReadyStateChange;
                }

                if (instance.parameters) {
                    instance.serializedParameters = serializeParameter(instance.parameters); //.toString();
                }

                if (instance.disableCache) {
                    var randomParam = "_sclDisableCache=" + new Date().getTime();

                    if (instance.serializedParameters === undefined) {
                        instance.serializedParameters = randomParam;
                    }
                    else {
                        instance.serializedParameters += "&" + randomParam;
                    }
                }

                instance.method = !instance.method ? "GET" : instance.method.toUpperCase();

                if (instance.method == "GET" && instance.serializedParameters) {
                    instance.url += "?" + instance.serializedParameters;
                }

                instance.http.open(instance.method, instance.url, instance.async);

                if (instance.method == "POST") {
                    instance.appendHeader("Content-type", instance.contentType);
                }

                instance.appendHeader("Pragma", "no-cache");

                for (var header in instance.headers) {
                    instance.http.setRequestHeader(header, instance.headers[header]);
                }

                instance.http.send(instance.serializedParameters);
            }
            catch (e) {
                if (typeof (e) == "string") {
                    instance.errorMessage = "XmlHttp Error: " + e;
                }
                else {
                    instance.errorMessage = "XmlHttp Error: " + e.message;
                }

                instance.returnError(instance.errorMessage);

                return false;
            }
            return true;
        };

        this.onReadyStateChange = function () {
            if (instance.aborted) {
                instance.http.onreadystatechange = null;
                instance.http.abort = null;
                instance.http = null;
                return;
            }

            if (instance.http == null || instance.http.readyState != 4) {
                return;
            }

            var errorException = null;
            var result = instance.http.responseText;

            if (instance.http.status != 200) {
                var errorMessage = result;
                if(errorMessage == null){
                    errorMessage = instance.http.statusText;
                }
                errorException = new CallbackException(errorMessage);
            }

            // prevent memory leaks
            if (instance.http) {
                try {
                    instance.http.onreadystatechange = null;
                    instance.http.abort = null;
                    instance.http = null;
                }
                catch (e) {
                }
            }

            if (errorException) {
                if (instance.errorCallback) {
                    instance.errorCallback(errorException, instance);
                }
                return;
            }
            else if (instance.successCallback) {
                instance.successCallback((result && instance.evalResult) ?
			            eval('(' + result + ')') :
			            result, instance);
            }
        };

        this.returnError = function (Message) {
            var errorException = new CallbackException(Message);

            if (instance.errorCallback) {
                instance.errorCallback(errorException, instance);
            }
        };

        var CallbackException = function (Message) {
            this.isCallbackError = true;

            if (typeof (Message) == "object" && Message.message) {
                this.message = Message.message;
            }
            else {
                this.message = Message;
            }
        };
    };


})();