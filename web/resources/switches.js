var protocol = "";
var host = "";
var login = "";
var firstRun = true;
var server;

function onStartup() {
	server = new Switchando();
	/*var xhr = server.sendCommand("get/rooms", "/auth.html", "auth.html?data=Wrong username and / or password");
	/*xhr.onreadystatechange = function () {
		var data = this.responseText;
		try {
		} catch (ex) {
			window.location = "/auth.html";
			return;
		}
		if (data.includes("{\"status\":")) {
			var parsed = JSON.parse(data);
			window.location = "/auth.html?data=" + parsed.description;
			return;
		}
		if (this.responseText === "") return;
		onLoad(data);
		componentHandler.upgradeDom();
	};
	xhr.send();
	onLoad(xhr); //*/
	var data = "";
	try {
		data = server.sendCommand("get/rooms", "/auth.html", "auth.html?data=Wrong username and / or password");
	} catch (ex) {
		window.location = "/auth.html";
		return;
	}
	if (data.includes("{\"status\":")) {
		var parsed = JSON.parse(data);
		window.location = "/auth.html?data=" + parsed.description;
		return;
	}
	onLoad(data);
}
function onLoad(json)
{
    if (firstRun == false)
    {
        return;
    }
    var parsed = JSON.parse(json);
    var innerHtml = "";

    for (index = 0; index < parsed.length; ++index) {

	    innerHtml += "<br /><br />";
	    if (parsed[index].Hidden == true) continue;

	    var roomWidget = "";
	    var mainSwitch = false;

	    var objectsInnerHtml = "";
        for (objectIndex = 0; objectIndex < parsed[index].Objects.length; ++objectIndex)
		{
			if (typeof parsed.status === 'undefined' && parsed[index].Objects[objectIndex].ObjectModel == null) {
				continue;
			}
			var fragment = loadFile("/switchando-fragments/" + parsed[index].Objects[objectIndex].ObjectModel);
			
			Object.getOwnPropertyNames(parsed[index].Objects[objectIndex]).forEach(
				function (val, idx, array) {
					fragment = replaceAll(fragment, "%" + val + "%", parsed[index].Objects[objectIndex][val]);
				}
			);
            objectsInnerHtml += fragment;
        }

        roomWidget += '<div id="div-' + parsed[index].Name + '" style="width:100%;position:relative;" class="">' +
                    '<h4 style="color:black">' + parsed[index].Name + '</h4>' +
                    '<div style="position:absolute;right:15px;top:0px">' +
                    '<label style="height:100%" class="mdl-switch mdl-js-switch mdl-js-ripple-effect" for="switch-' + parsed[index].Name + '">';
        if (mainSwitch == false) {
            roomWidget += '<input type="checkbox" id="switch-' + parsed[index].Name + '" class="mdl-switch__input" onClick="onClick(&quot;' + parsed[index].Name + '&quot;)" />';
        }
        else {
            roomWidget += '<input type="checkbox" id="switch-' + parsed[index].Name + '" class="mdl-switch__input" onClick="onClick(&quot;' + parsed[index].Name + '&quot;)" checked />';
        }
        roomWidget += '<span class="mdl-switch__label"></span>' +
            '</label>' +
            '</div>' +
            '</div>';

        innerHtml += roomWidget;
        innerHtml += objectsInnerHtml;

    }
    document.getElementById('switches_div').innerHTML += innerHtml;
    collapse();
	firstRun = false;
	setInterval(async function () {
		updateSwitches();
    }, 3000);

    $('#search').on('input', function () {
        var pageDivs = document.getElementsByClassName("switchdiv");
        if ($(this).val() == "")
        {
            for (i = 0; i < pageDivs.length; i++) {
                 pageDivs[i].style.display = "";
            }
            return;
        }
        for (i = 0; i < pageDivs.length; i++) {
            var id = pageDivs[i].id.substring(4).toLowerCase();
            if (id.includes($(this).val())) {
                pageDivs[i].style.display = "";
            } else pageDivs[i].style.display = "none";
        }
	});
	setTimeout(async function () {
		loadSwitches(json);
	}, 250);
}
function updateSwitches() {
	var xhr = server.buildRequest("get/rooms");
	xhr.onreadystatechange = function () {
		if (this.responseText === "") return;
		loadSwitches(this.responseText);
	};
	xhr.send();
}
async function loadSwitches(json) {
    var pageDivs = await document.getElementsByClassName("mdl-switch__input");
    var parsed = await JSON.parse(json);

    for (index = 0; index < parsed.length; ++index) {
        if (parsed[index].Hidden == true) continue;
        var mainSwitch = false;
        for (objectIndex = 0; objectIndex < parsed[index].Objects.length; ++objectIndex) {
            if (parsed[index].Objects[objectIndex].Switch != undefined) {
                if (parsed[index].Objects[objectIndex].Switch == true) {
                    var myCheckbox = document.getElementById('switch-' + parsed[index].Objects[objectIndex].Name);
                    myCheckbox.parentElement.MaterialSwitch.on();
                    mainSwitch = true;
                }
                else
                {
                    var myCheckbox = document.getElementById('switch-' + parsed[index].Objects[objectIndex].Name);
                    myCheckbox.parentElement.MaterialSwitch.off();
                }
            }
        }
        if (mainSwitch) {
            var myCheckbox = document.getElementById('switch-' + parsed[index].Name);
            myCheckbox.parentElement.MaterialSwitch.on();
        }
        else {
            var myCheckbox = document.getElementById('switch-' + parsed[index].Name);
            myCheckbox.parentElement.MaterialSwitch.off();
        }
    }
}

function onClick(target)
{
	var xhr;
    if (document.getElementById('switch-' + target).checked)
    {
		xhr = server.buildRequest("id/" + target + "/switch?objname=" + target + "&switch=true");
    }
	else xhr = server.buildRequest("id/" + target + "/switch?objname=" + target + "&switch=false");

	xhr.onreadystatechange = function () {
		updateSwitches();
	};
	xhr.send();
}
function switchOn(target) {
	var xhr = server.buildRequest("id/" + target + "/switch?objname=" + target + "&switch=true");
	xhr.onreadystatechange = function () {
		updateSwitches();
	};
	xhr.send();
}
function switchOff(target) {
	var xhr = server.buildRequest("id/" + target + "/switch?objname=" + target + "&switch=false");
	xhr.onreadystatechange = function () {
		updateSwitches();
	};
	xhr.send();
}

function get(name) {
    if (name = (new RegExp('[?&]' + encodeURIComponent(name) + '=([^&]*)')).exec(location.search))
        return decodeURIComponent(name[1]);
}
function show() {
document.getElementById('fsbtn').style.display = 'none';
	var elem = document.body;
        if ((document.fullScreenElement !== undefined && document.fullScreenElement === null) || (document.msFullscreenElement !== undefined && document.msFullscreenElement === null) || (document.mozFullScreen !== undefined && !document.mozFullScreen) || (document.webkitIsFullScreen !== undefined && !document.webkitIsFullScreen)) {
            if (elem.requestFullScreen) {
                elem.requestFullScreen();
            } else if (elem.mozRequestFullScreen) {
                elem.mozRequestFullScreen();
            } else if (elem.webkitRequestFullScreen) {
                elem.webkitRequestFullScreen(Element.ALLOW_KEYBOARD_INPUT);
            } else if (elem.msRequestFullscreen) {
                elem.msRequestFullscreen();
            }
        } else {
            if (document.cancelFullScreen) {
                document.cancelFullScreen();
            } else if (document.mozCancelFullScreen) {
                document.mozCancelFullScreen();
            } else if (document.webkitCancelFullScreen) {
                document.webkitCancelFullScreen();
            } else if (document.msExitFullscreen) {
                document.msExitFullscreen();
            }
        }
}
function collapse() {
    $(".option-content").hide();
    $(".arrow-up").hide();
    $(".option-heading").click(function () {
        $(this).next(".option-content").slideToggle(500);
        $(this).find(".arrow-up, .arrow-down").toggle(500);
    });
}
function loadFile(filePath) {
    var result = "";
    var xmlhttp = new XMLHttpRequest();
	xmlhttp.open("GET", filePath, false);
	try {
		xmlhttp.send();
		if (xmlhttp.status == 200) {
			result = xmlhttp.responseText;
		}
		return result;
	}
	catch(e) { return ""; }
}
function replaceAll(str, find, replace) {
    return str.replace(new RegExp(find, 'g'), replace);
}