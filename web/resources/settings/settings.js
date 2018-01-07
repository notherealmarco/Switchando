function onStartup() {
    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (xhr.readyState == XMLHttpRequest.DONE) {
            var replaced = xhr.responseText.replace(/\\/g, '');
            onClientsLoad(replaced);
        }
    }
    xhr.open('GET', get('protocol') + '://' + get('ip') + "/api?get=clients&password=" + get('password'), false);
    xhr.send();

    xhr.onreadystatechange = function () {
        if (xhr.readyState == XMLHttpRequest.DONE) {
            var replaced = xhr.responseText.replace(/\\/g, '');
            onRoomsLoad(replaced);
        }
    }
    xhr.open('GET', get('protocol') + '://' + get('ip') + "/api?get=rooms&password=" + get('password'), false);
    xhr.send();
}
function loadRooms() {
    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (xhr.readyState == XMLHttpRequest.DONE) {
            var replaced = xhr.responseText.replace(/\\/g, '');
            onRoomsLoad(replaced);
        }
    }
    xhr.open('GET', get('protocol') + '://' + get('ip') + "/api?get=rooms&password=" + get('password'), false);
    xhr.send();
}
function buttonaddobjectpageload()
{
    loadSwitchables();
    loadButtons();
}
function switchbuttonaddobjectpageload() {
    loadSwitchables();
    loadSwitchButtons();
}
function loadSwitchables() {
    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (xhr.readyState == XMLHttpRequest.DONE) {
            var replaced = xhr.responseText.replace(/\\/g, '');
            onSwitchablesLoad(replaced);
        }
    }
    xhr.open('GET', get('protocol') + '://' + get('ip') + "/api?get=switchabledevices&password=" + get('password'), false);
    xhr.send();
}
function loadButtons() {
    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (xhr.readyState == XMLHttpRequest.DONE) {
            var replaced = xhr.responseText.replace(/\\/g, '');
            onButtonsLoad(replaced);
        }
    }
    xhr.open('GET', get('protocol') + '://' + get('ip') + "/api?get=devices&password=" + get('password'), false);
    xhr.send();
}
function loadSwitchButtons() {
    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (xhr.readyState == XMLHttpRequest.DONE) {
            var replaced = xhr.responseText.replace(/\\/g, '');
            onSwitchButtonsLoad(replaced);
        }
    }
    xhr.open('GET', get('protocol') + '://' + get('ip') + "/api?get=devices&password=" + get('password'), false);
    xhr.send();
}
function onRoomsLoad(json) {
    var parsed = JSON.parse(json);
    for (index = 0; index < parsed.length; ++index) {

        var x = document.getElementById("room_select");
        var option = document.createElement("option");
        option.text = parsed[index].Name;
        x.add(option);
    }
}
function onClientsLoad(json) {
    var parsed = JSON.parse(json);
    for (index = 0; index < parsed.length; ++index) {

        var x = document.getElementById("client_select");
        var option = document.createElement("option");
        option.text = parsed[index].Name;
        x.add(option);
    }
}
function onSwitchablesLoad(json) {
    var parsed = JSON.parse(json);
    for (index = 0; index < parsed.length; ++index) {

        var x = document.getElementById("switchables_select");
        var option = document.createElement("option");
        option.text = parsed[index].Name;
        x.add(option);
    }
}
function onButtonsLoad(json) {
    var parsed = JSON.parse(json);
    for (index = 0; index < parsed.length; ++index) {
        if (parsed[index].ObjectType == "BUTTON") {
            var x = document.getElementById("buttons_select");
            var option = document.createElement("option");
            option.text = parsed[index].Name;
            x.add(option);
        }
    }
}
function onSwitchButtonsLoad(json) {
    var parsed = JSON.parse(json);
    for (index = 0; index < parsed.length; ++index) {
        if (parsed[index].ObjectType == "SWITCH_BUTTON") {
            var x = document.getElementById("buttons_select");
            var option = document.createElement("option");
            option.text = parsed[index].Name;
            x.add(option);
        }
    }
}
function addRoom(id) {
    var name = id.name.value;
    var friendlynames = id.friendlynames.value;
    var hidden = id.ifhidden.checked;

    var xhr = new XMLHttpRequest();
    xhr.open('GET', get('protocol') + '://' + get('ip') + "/api?interface=configuration&addroom=" + name + "&setfriendlynames=" + friendlynames + "&hiddenroom=" + hidden + "&password=" + get('password'), false);
    xhr.send();
    alert("Request sent! Please return back");
}
function addLightRGB(id) {
    var name = id.name.value;
    var friendlynames = id.friendlynames.value;
    var description = id.description.value;
    var pinred = id.pinred.value;
    var pingreen = id.pingreen.value;
    var pinblue = id.pinblue.value;
    var client = id.client_select.value;
    var room = id.room_select.value;

    var xhr = new XMLHttpRequest();
    xhr.open('GET', get('protocol') + '://' + get('ip') + "/api?interface=configuration&addlightrgb=" + name + "&setfriendlynames=" + friendlynames + "&description=" + description + "&addpinr=" + pinred + "&addping=" + pingreen + "&addpinb=" + pinblue + "&client=" + client + "&room=" + room + "&password=" + get('password'), false);
    xhr.send();
    alert("Request sent! Please return back");
}
function addLightW(id) {
    var name = id.name.value;
    var friendlynames = id.friendlynames.value;
    var description = id.description.value;
    var pin = id.pin.value;
    var client = id.client_select.value;
    var room = id.room_select.value;

    var xhr = new XMLHttpRequest();
    xhr.open('GET', get('protocol') + '://' + get('ip') + "/api?interface=configuration&addlightw=" + name + "&setfriendlynames=" + friendlynames + "&description=" + description + "&addpin=" + pin + "&client=" + client + "&room=" + room + "&password=" + get('password'), false);
    xhr.send();
    alert("Request sent! Please return back");
}
function addRelay(id) {
    var name = id.name.value;
    var friendlynames = id.friendlynames.value;
    var description = id.description.value;
    var pin = id.pin.value;
    var client = id.client_select.value;
    var room = id.room_select.value;

    var xhr = new XMLHttpRequest();
    xhr.open('GET', get('protocol') + '://' + get('ip') + "/api?interface=configuration&addrelay=" + name + "&setfriendlynames=" + friendlynames + "&description=" + description + "&addpin=" + pin + "&client=" + client + "&room=" + room + "&password=" + get('password'), false);
    xhr.send();
    alert("Request sent! Please return back");
}
function addWebRelay(id) {
    var name = id.name.value;
    var identifier = id.identifier.value;
    var friendlynames = id.friendlynames.value;
    var description = id.description.value;
    var room = id.room_select.value;
    var createBtn = "none";
    if (id.btnNone.checked == true) createBtn = "none";
    if (id.btnBtn.checked == true) createBtn = "button";
    if (id.btnSwbtn.checked == true) createBtn = "switch_button";

    var xhr = new XMLHttpRequest();
    xhr.open('GET', get('protocol') + '://' + get('ip') + "/api?interface=configuration&addwebrelay=" + name + "&id=" + identifier + "&setfriendlynames=" + friendlynames + "&description=" + description + "&room=" + room + "&password=" + get('password') + "&createbutton=" + createBtn, false);
    xhr.send();
    alert(get('protocol') + '://' + get('ip') + "/api?interface=configuration&addwebrelay=" + name + "&id=" + identifier + "&setfriendlynames=" + friendlynames + "&description=" + description + "&room=" + room + "&password=" + get('password') + "&createbutton=" + createBtn);
}
function addSimpleFan(id) {
    var name = id.name.value;
    var friendlynames = id.friendlynames.value;
    var description = id.description.value;
    var pin = id.pin.value;
    var client = id.client_select.value;
    var room = id.room_select.value;

    var xhr = new XMLHttpRequest();
    xhr.open('GET', get('protocol') + '://' + get('ip') + "/api?interface=configuration&addsimplefan=" + name + "&setfriendlynames=" + friendlynames + "&description=" + description + "&addpin=" + pin + "&client=" + client + "&room=" + room + "&password=" + get('password'), false);
    xhr.send();
    alert("Request sent! Please return back");
}
function addButton(id) {
    var name = id.name.value;
    var pin = id.pin.value;
    var client = id.client_select.value;
    var room = id.room_select.value;

    var xhr = new XMLHttpRequest();
    xhr.open('GET', get('protocol') + '://' + get('ip') + "/api?interface=configuration&addbutton=" + name + "&addpin=" + pin + "&client=" + client + "&room=" + room + "&password=" + get('password'), false);
    xhr.send();
    alert("Request sent! Please return back");
}
function addBlinds(id) {
    var name = id.name.value;
    var friendlynames = id.friendlynames.value;
    var description = id.description.value;
    var room = id.room_select.value;
    var totalsteps = id.time.value;

    if (document.getElementById("btnRaspi").checked)
    {
        var openpin = id.openpin.value;
        var closepin = id.closepin.value;
        var client = id.client_select.value;

        var xhr = new XMLHttpRequest();
        xhr.open('GET', get('protocol') + '://' + get('ip') + "/api?interface=configuration&addblinds=" + name + "&description=" + description + "&setfriendlynames=" + friendlynames + "&openpin=" + openpin + "&closepin=" + closepin + "&totalsteps=" + totalsteps + "&client=" + client + "&room=" + room + "&password=" + get('password'), false);
        xhr.send();
    }
    else
    {
        var openpin = id.openwebrelayid.value;
        var closepin = id.closewebrelayid.value;
        var client = "local";

        var xhr = new XMLHttpRequest();
        xhr.open('GET', get('protocol') + '://' + get('ip') + "/api?interface=configuration&addblinds=" + name + "&description=" + description + "&setfriendlynames=" + friendlynames + "&webrelayopenid=" + openpin + "&webrelaycloseid=" + closepin + "&totalsteps=" + totalsteps + "&client=" + client + "&room=" + room + "&password=" + get('password'), false);
        xhr.send();
    }
    
    alert("Request sent! Please return back");
}
function buttonAddCommand(id) {
    var name = id.name.value;
    var command = id.btncmd.value;

    command = command.replace(/=/g, ',,').replace(/&/g, ',,,');

    var xhr = new XMLHttpRequest();
    xhr.open('GET', get('protocol') + '://' + get('ip') + "/api?interface=configuration&buttonaddcommand=" + name + "&command=" + command + "&password=" + get('password'), false);
    xhr.send();
    alert("Request sent! Please return back");
}
function buttonAddObject(id) {
    var name = id.buttons_select.value;
    var command = id.switchables_select.value;

    command = command.replace(/=/g, ',,').replace(/&/g, ',,,');

    var xhr = new XMLHttpRequest();
    xhr.open('GET', get('protocol') + '://' + get('ip') + "/api?interface=configuration&buttonaddobject=" + name + "&object=" + command + "&password=" + get('password'), false);
    xhr.send();
    alert("Request sent! Please return back");
}
function addSwitchButton(id) {
    var name = id.name.value;
    var pin = id.pin.value;
    var client = id.client_select.value;
    var room = id.room_select.value;

    var xhr = new XMLHttpRequest();
    xhr.open('GET', get('protocol') + '://' + get('ip') + "/api?interface=configuration&addswitchbutton=" + name + "&addpin=" + pin + "&client=" + client + "&room=" + room + "&password=" + get('password'), false);
    xhr.send();
    alert("Request sent! Please return back");
}
function switchButtonAddCommand(id) {
    var name = id.name.value;
    var command = id.btncmd.value;
    var pretype = id.optionon.checked;
    var type;
    if (pretype == true)
    {
        type = "on";
    }
    else
    {
        type = "off";
    }

    var xhr = new XMLHttpRequest();

    command = command.replace(/=/g, ',,').replace(/&/g, ',,,');

    xhr.open('GET', get('protocol') + '://' + get('ip') + "/api?interface=configuration&switchbuttonaddcommand=" + name + "&command=" + command + "&type=" + type + "&password=" + get('password'), false);
    xhr.send();
    alert("Request sent! Please return back");
}
function switchButtonAddObject(id) {
    var name = id.name.value;
    var command = id.btncmd.value;

    command = command.replace(/=/g, ',,').replace(/&/g, ',,,');

    var xhr = new XMLHttpRequest();
    xhr.open('GET', get('protocol') + '://' + get('ip') + "/api?interface=configuration&switchbuttonaddobject=" + name + "&object=" + command + "&password=" + get('password'), false);
    xhr.send();
    alert("Request sent! Please return back");
}
function get(name) {
    if (name = (new RegExp('[?&]' + encodeURIComponent(name) + '=([^&]*)')).exec(location.search))
        return decodeURIComponent(name[1]);
}
function redirect(uri) {
    uri = uri + document.location.search;
    window.location.href = uri;
}
