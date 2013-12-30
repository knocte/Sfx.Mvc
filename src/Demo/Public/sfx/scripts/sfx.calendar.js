

    // Control calendario.
    //
    // Para formato americano: Sfx.setEnv("firstWeekDay", "sun");
    //
    // Para modificar las celdas de los días:
    //    params = { 
    //        buildDayCell: function(td, d) {
    //            if(d.getDay() == 1){
    //                td.style.backgroundColor = "#cccccc";
    //            }
    //        }
    //    }
    
(function () {
    "use strict";

    if (!Sfx.isDefined(Sfx)) {
        Sfx = {};
    }
    
     // Crea un control seleccionar fecha en el elemento.
    // Sfx.pickCalendar(panel);
    //
    // Con parámetros:
    //
    // Sfx.pickCalendar(panel, {
    //   date: new Date(2012, 11, 1)
    // });
    Sfx.showPickCalendar = function (element, params) {
        if (!params) {
            params = {};
        }
                   
        element = Sfx.get(element);

        var date = params.date || new Date();
        if(Sfx.isString(date)){
            date = Sfx.parseDate(date);
        }
   
        var datePicker;

        function dayClick(day) {
            element.value = Sfx.formatDate(day.date, params.format);
            destroyPicker();            
            if(params.change){
                // devuelve los propios params para que el callback pueda tener una
                // referencia a quien le llama.
                params.change(input.value, params);
            }

            // lanzar el evento del control para detectar que ha cambiado
            if(element.onchange != null) {
            	element.onchange();
            }
        }

        function getPos(elem) {
            var docElem, win,
                box = { top: 0, left: 0 },
                doc = elem && elem.ownerDocument;
            if ( !doc ) { return; }
            docElem = doc.documentElement;
            if ( typeof elem.getBoundingClientRect !== typeof undefined ) {
                box = elem.getBoundingClientRect();
            }
            win = self;
            return {
                x: box.left + ( win.pageXOffset || docElem.scrollLeft ) - ( docElem.clientLeft || 0 ),
                y: box.top  + ( win.pageYOffset || docElem.scrollTop )  - ( docElem.clientTop  || 0 )
            };
        }

        function showPicker() {
        	// si ya se está mostrando, ocultarlo y salir
            if(Sfx.showPickCalendar.instance != null) {
        		destroyPicker();
        		return;
        	}
        
            var position = Sfx.getPosition(element);
            datePicker = document.createElement("div");
            datePicker.className = params.datePickerClass || "datePicker";
            datePicker.style.position = "absolute";
            datePicker.style.left = position.x + "px";  
            datePicker.style.top = (position.y + element.offsetHeight) + "px";   
            params.dayClick = dayClick;
            Sfx.calendar(datePicker, params);
            
            // lo añade al elemento que contiene a element
            Sfx.appendToBody(datePicker);
            
            // destruir el calendario si se hace click en cualquier otra parte
            Sfx.onGlobal("click", destroyPicker);
        }

        function destroyPicker() {
            Sfx.removeNode(datePicker);
            Sfx.removeNode(Sfx.showPickCalendar.instance);
            datePicker = null;
            Sfx.showPickCalendar.instance = null;
        }        
        
        showPicker();
        
        // sirve para tener una referencia al calendario que se está mostrando actualmente
        Sfx.showPickCalendar.instance = datePicker;
    };

    // Muestra un calendario expandido en el elemento.
    // 
    // Sfx.calendar(panel, {
    //  date: new Date(2012, 11, 1),
    //  dayClick: function(d){ Sfx.log(d); },
    // });
    Sfx.calendar = function (element, params) {
        if (!params) {
            params = {};
        }

        var date = params.date || new Date();

        // mostrar el calendario
        var calendar = buildCalendar(date, params);
        var panel = document.createElement("div");
        panel.appendChild(calendar.table);
        Sfx.get(element).appendChild(panel);
        
        return calendar;
    };

    // Genera una tabla con un calendario
    // Devuelve un objeto calendar.
    // calendar.table es la tabla HTML DOM.
    // calendar.dates son las referencias a las fechas
    // calendar.dates[0].cell es la referencia a la celda que represennta esa fecha.
    function buildCalendar(date, params) {          
        var calendar = {};
        calendar.date = date;
        calendar.params = params;
        calendar.dates = {};
        calendar.dayClick = params.dayClick;
        calendar.monthChanged = params.monthChanged;
        
        calendar.prevClick = function(event){
            Sfx.cancelBubble(event);

            if(params.prevClick){
                if(!params.prevClick()){
                    return; // Si no devuelve true, salir. Sirve para poder paralizar la acción.
                }
            }
            date.setMonth(date.getMonth() - 1);
            calendar.date = date;
            buildCalendarTable(calendar, date);
            
            if(calendar.monthChanged){
                calendar.monthChanged();
            }
        };
        
        calendar.nextClick = function(event){
            Sfx.cancelBubble(event);
            
            if(params.nextClick){
                if(!params.nextClick()){
                    return; // Si no devuelve true, salir. Sirve para poder paralizar la acción.
                }
            }
            
            Sfx.addMonths(date, 1);
            calendar.date = date;
            buildCalendarTable(calendar, date);
            
            if(calendar.monthChanged){
                calendar.monthChanged();
            }
        };
        
        buildCalendarTable(calendar, date);        
        return calendar;
    }
    
    
    function buildCalendarTable(calendar) {    
        var date = calendar.date;        
        if(Sfx.isString(date)){
            date = Sfx.parseDate(date);
        }

        // el mes que se va a mostrar
        var month = date.getMonth();
        var now = new Date();
        now.setHours(0, 0, 0, 0);

        // el primer día del mes
        var firstDay = new Date(date.getFullYear(), date.getMonth(), 1);
        var lastDay = new Date(new Date(date.getFullYear(), date.getMonth() + 1, 1) - 1);

        // Para las culturas en que la semana empieza el domingo
        var weekDayOffset = Sfx.getEnv("firstWeekDay") === "sun" ? 0 : 1;

        // ajustar los dias de inicio para que sea el primer día de la semana.
        firstDay.setDate(firstDay.getDate() - firstDay.getDay() + weekDayOffset);

        // Ajustar el primer día en caso de que sea el 2 (1 en base cero) empezar a mostrar
        // una semana antes para que aparezca tb el día 1.
        if(firstDay.getDay() == 1){
            firstDay.setDate(firstDay.getDate()-7);
        }

        // ajustar hasta el final de la semana
        if (lastDay.getDay() <= 7 - weekDayOffset) {
            lastDay.setDate(lastDay.getDate() + 6 + weekDayOffset - (lastDay.getDay()));
        }

        // Crear la tabla o reinicializala si ya existe
        if(!calendar.table){
            calendar.table = document.createElement("table");
            calendar.table.className = calendar.params.className || "calendar";
        } else {
            Sfx.clearChildNodes(calendar.table);
        }        
        
        var table = calendar.table;

        var monthNames = ["Enero", "Febrero", "Marzo", "Abril",
                          "Mayo", "Junio", "Julio", "Agosto", "Septiembre",
                          "Octubre", "Noviembre", "Diciembre"];
                          
         var weekDays = ["L", "M", "M", "J", "V", "S", "D"];

        // var monthNames = ["January", "February", "March", "April",
        //                   "May", "June", "July", "August", "September",
        //                   "October", "November", "December"];

        var tr = table.insertRow(-1);
        tr.className = "calendarHeader";

        // botón mes previo
        var td = tr.insertCell(-1);
        var previousMonth = document.createTextNode("<");
        td.appendChild(previousMonth);
        td.onclick = calendar.prevClick;

        // celda del mes
        td = tr.insertCell(-1);
        var monthName = Sfx.t(monthNames[month]) + " " + date.getFullYear();
        td.appendChild(document.createTextNode(monthName));
        td.colSpan = 5;

        // botón siguiente mes
        td = tr.insertCell(-1);
        var nextMonth = document.createTextNode(">");
        td.appendChild(nextMonth);
        td.onclick = calendar.nextClick;
        
        // fila de los días de la semana
		tr = table.insertRow(-1);
        tr.className = "calendarWeekDays";        
        for(var i=0; i <= 6; i++){
            var td = tr.insertCell(-1);
            td.appendChild(document.createTextNode(weekDays[i]));
        }        
        
        // generar los días
        var currentDate = Sfx.copyDate(firstDay);

        while (currentDate < lastDay) {
            if (currentDate.getDay() == weekDayOffset) {
                tr = table.insertRow(-1);
            }

            var td = tr.insertCell(-1);
            td.appendChild(document.createTextNode(currentDate.getDate()));
            td.className = "calendarDay";
            
            // si el día esta fuera del mes, añadir un estilo
            if (currentDate.getMonth() != month) {
                td.className += " calendarDayGrayDay";
            // si es el día actual, añadir su estilo
            } else if (currentDate.getTime() == now.getTime()) {
                td.className += " today";
            }

            (function (date, cell) {  // escapar del closure               
               // la fecha en formato string
               var strDate = Sfx.formatDate(date);
               
               var calendarDate = calendar.dates[strDate];               
               if(!calendarDate){
                    calendarDate = { date: date, cell: cell, calendar: calendar };
                    calendar.dates[strDate] = calendarDate;
               } else {
                    // si ya existe actualizar la celda porque se renueva
                    // el DOM cada vez que se regenera
                    calendarDate.cell = cell;
               }
               
               td.onclick = function () {
                    if (calendar.dayClick) {
                        calendar.dayClick(calendarDate)
                    }
               };
               
            })(Sfx.copyDate(currentDate), td);

            currentDate.setDate(currentDate.getDate() + 1);
        }

        calendar.table = table;
    }

})();
   


