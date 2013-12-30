
/**
    @title menu.
    @summary Añade las funcionalidades al menu principal de admin para mostrar y ocultar opciones.
*/
 
(function () {
    "use strict";

    var isSubmenuVisible = false;

	Sfx.onLoad(init);
	// Sfx.attach(window, "resize", hideMenu);

	// Añade los eventos para mostrar subemenus
	function init() {
		// Obtiene los nodos principales del menu.
		var items = Sfx.getByClass("menuItem");
		Sfx.each(items, function(item){
			if(item.id) {
				var submenuName = getSubmenuName(item.id);
				var submenu = document.getElementById(submenuName);

				Sfx.attach(item, "click", function(event){
					menuClick(event, item, submenu);
				});

				Sfx.attach(item, "mouseover", function(event){
					menuMouseOver(event, item, submenu);
				});
			}
		});
	}

	function getMenuName(submenuName) {
		var name = submenuName.substring(submenuName.indexOf('_') + 1);
		return "menu_" + name;
	}

	function getSubmenuName(menuName) {
		var name = menuName.substring(menuName.indexOf('_') + 1);
		return "submenu_" + name;
	}

	function menuClick(event, item, submenu){
		Sfx.cancelBubble(event);
		hideSubmenuWindows(item);
		showSubmenu(item, submenu);
	}

	function menuMouseOver(event, item, submenu){
		if(submenu == null) {
			// puede que no haya submenu.
			return;
		}

		// mostrar directamente en mouseover solo cuando no estamos
		// en el modo reducido
		var parentItemName = getMenuName(submenu.id);
		var parentItem = document.getElementById(parentItemName);
		if(!compactMode(parentItem)) {
			Sfx.cancelBubble(event);
			if(isSubmenuVisible){
				hideSubmenuWindows(parentItem);
				showSubmenu(item, submenu);
			}
		}
	}

	function showSubmenu(item, submenu){

		var parentItemName = getMenuName(submenu.id);
		var parentItem = document.getElementById(parentItemName);
		
		// resaltar el padre del submenu que se esta mostrando
		Sfx.addClass(parentItem, "highlighted");

		if(compactMode(parentItem)) {
			appendtToMenuItem(parentItem, submenu);
		}
		else {
			appendToBody(parentItem, submenu);
		}

		Sfx.swapClass(submenu, "hidden", "submenuVisible");
		isSubmenuVisible = true;

		if(arrowItem(item)) {
			// aleja un poco el submenu cuando pinchas en una flecha.
			addOffset(submenu);
		}

		// esconder si se hace click o redimensiona la ventana
		Sfx.onGlobal("click", function() {
			hideMenu(parentItem);
		});
	}

	function addOffset(submenu){
		var offset = 7;
		var position = Sfx.getPosition(submenu);
		submenu.style.top = (position.y + offset) + "px";	
	}

	// Si el elemento en el que se ha hecho click es una flecha.
	function arrowItem(item){
		return Sfx.hasClass(item, "downArrow");
	}

	// Si esta en modo compacto (estilos responsivos)
	function compactMode(menuItem){
		return Sfx.getComputedStyle(menuItem.parentNode, "float") == "none";
	}

	function appendtToMenuItem(menuItem, submenu){		
		var parent = menuItem.parentNode;		
		if(submenu.parentNode != parent) {
			submenu.style.top = "0px";
			submenu.style.left = "0px";
			parent.appendChild(submenu);
		}
	}

	function appendToBody(menuItem, submenu){		
		var parent = submenu.parentNode;
		var position = Sfx.getPosition(menuItem);
		submenu.style.top = (position.y + menuItem.offsetHeight) + "px";
		submenu.style.left = position.x + "px";		
		if(submenu.parentNode == menuItem.parentNode) {
			Sfx.appendToBody(submenu);
		}
	}

	function hideMenu(menuItem){		
		isSubmenuVisible = false;
		hideSubmenuWindows(menuItem);
	}

	function hideSubmenuWindows(menuItem){
		// oculta las ventanas
		Sfx.each(Sfx.getByClass("submenu"), function(item){
			Sfx.swapClass(item, "submenuVisible", "hidden");
		});

		// muestra el resaltado en las cabeceras
		var menu = Sfx.getParent(menuItem, ".menu");
		Sfx.each(Sfx.getByClass("menuItem", menu), function(item){
			Sfx.removeClass(item, "highlighted");
		});
	}

})();
   






















