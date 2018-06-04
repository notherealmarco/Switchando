function loadClients() {
	var server = new Switchando();
	var clients = server.sendCommand("get/clients");

	var parsed = JSON.parse(clients);
	if (typeof parsed.status !== 'undefined' && parsed.status !== null) {
		window.location.replace("/server-error.html?json=" + clients);
	} else {
		for (index = 0; index < parsed.length; ++index) {
			var x = document.getElementById("client_select");
			var option = document.createElement("option");
			option.text = parsed[index].Name;
			x.add(option);
		}
	}
}
function loadRooms() {
	var server = new Switchando();
	var clients = server.sendCommand("get/rooms");
	var parsed = JSON.parse(clients);
	if (typeof parsed.status !== 'undefined' && parsed.status !== null) {
		window.location.replace("/server-error.html?json=" + clients);
	} else {
		for (index = 0; index < parsed.length; ++index) {
			var x = document.getElementById("room_select");
			var option = document.createElement("option");
			option.text = parsed[index].Name;
			x.add(option);
		}
	}
}
function loadButtons() {
	var server = new Switchando();
	var clients = server.sendCommand("get/devices");
	var parsed = JSON.parse(clients);
	if (typeof parsed.status !== 'undefined' && parsed.status !== null) {
		window.location.replace("/server-error.html?json=" + clients);
	} else {
		for (index = 0; index < parsed.length; ++index) {
			if (parsed[index].ObjectKind == "BUTTON") {
				var x = document.getElementById("buttons_select");
				var option = document.createElement("option");
				option.text = parsed[index].Name;
				x.add(option);
			}
		}
	}
}
function loadSwitchButtons() {
	var server = new Switchando();
	var clients = server.sendCommand("get/devices");
	var parsed = JSON.parse(clients);
	if (typeof parsed.status !== 'undefined' && parsed.status !== null) {
		window.location.replace("/server-error.html?json=" + clients);
	} else {
		for (index = 0; index < parsed.length; ++index) {
			if (parsed[index].ObjectKind == "SWITCH_BUTTON") {
				var x = document.getElementById("buttons_select");
				var option = document.createElement("option");
				option.text = parsed[index].Name;
				x.add(option);
			}
		}
	}
}
function loadDevices() {
	var server = new Switchando();
	var clients = server.sendCommand("get/devices");
	var parsed = JSON.parse(clients);
	if (typeof parsed.status !== 'undefined' && parsed.status !== null) {
		window.location.replace("/server-error.html?json=" + clients);
	} else {
		for (index = 0; index < parsed.length; ++index) {
			var x = document.getElementById("devices_select");
			var option = document.createElement("option");
			option.text = parsed[index].Name;
			x.add(option);
		}
	}
}
function loadUsers() {
	var server = new Switchando();
	var clients = server.sendCommand("get/users");
	var parsed = JSON.parse(clients);
	if (typeof parsed.status !== 'undefined' && parsed.status !== null) {
		window.location.replace("/server-error.html?json=" + clients);
	} else {
		for (index = 0; index < parsed.length; ++index) {
			var x = document.getElementById("identities_select");
			var option = document.createElement("option");
			option.text = parsed[index];
			x.add(option);
		}
	}
}
function loadSwitchableDevices() {
	var server = new Switchando();
	var clients = server.sendCommand("get/switchable_devices");
	var parsed = JSON.parse(clients);
	if (typeof parsed.status !== 'undefined' && parsed.status !== null) {
		window.location.replace("/server-error.html?json=" + clients);
	} else {
		for (index = 0; index < parsed.length; ++index) {
			var x = document.getElementById("switchables_select");
			var option = document.createElement("option");
			option.text = parsed[index].Name;
			x.add(option);
		}
	}
}
function loadActions() {
	var server = new Switchando();
	var clients = server.sendCommand("action/getActions");
	var parsed = JSON.parse(clients);
	if (typeof parsed.status == 'undefined' && parsed.status == 0) {
		window.location.replace("/server-error.html?json=" + clients);
	} else {
		for (index = 0; index < parsed.actions.length; ++index) {
			var x = document.getElementById("action_select");
			var option = document.createElement("option");
			option.text = parsed.actions[index];
			x.add(option);
		}
	}
}
function loadGroups() {
	var server = new Switchando();
	var clients = server.sendCommand("get/devices");
	var parsed = JSON.parse(clients);
	if (typeof parsed.status !== 'undefined' && parsed.status !== null) {
		window.location.replace("/server-error.html?json=" + clients);
	} else {
		for (index = 0; index < parsed.length; ++index) {
			if (parsed[index].ObjectType == "DEVICE_GROUP") {
				var x = document.getElementById("group_select");
				var option = document.createElement("option");
				option.text = parsed[index].Name;
				x.add(option);
			}
		}
	}
}