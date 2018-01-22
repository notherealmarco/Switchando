function Switchando() {

	this.login_username = "";
	this.login_password = "";
	this.login_using_token = true;
	this.login_token = "";

	this.protocol = "";
	if (get('protocol') == null) this.protocol = window.location.protocol.slice(0, -1); else this.protocol = get('protocol');

	this.port = location.port || (location.protocol === 'https:' ? '443' : '80');

	this.host = "";
	if (get('ip') == null) {
		this.host = window.location.hostname + ":" + this.port;
	} else this.host = get('ip');

	this.login = function (username, password, keep_logged) {
		this.login_username = username;
		this.login_password = password;
		if (keep_logged) {
			Cookies.set('b0689', username, { expires: 30 });
			Cookies.set('1a1dc', this.generateToken(username, password), { expires: 30 });
		} else {
			sessionStorage.setItem('b0689', username);
			sessionStorage.setItem('1a1dc', this.generateToken(username, password));
		}
	};
	this.sendCommand = function (request) {

		var xhr = new XMLHttpRequest();
		var final_request = "";

		if (request.includes("?")) {
			if (request.endsWith("?")) {
				if (login_using_token) final_request = request + "login_username=" + login_username + "&login_token=" + login_token;
				else final_request = request + "login_username=" + login_username + "&login_password=" + login_password;
			} else {
				if (login_using_token) final_request = request + "&login_username=" + login_username + "&login_token=" + login_token;
				else final_request = request + "&login_username=" + login_username + "&login_password=" + login_password;
			}
		} else {
			if (login_using_token) final_request = request + "?login_username=" + login_username + "&login_token=" + login_token;
			else final_request = request + "?login_username=" + login_username + "&login_password=" + login_password;
		}

		xhr.open('GET', this.protocol + "://" + this.host + "/api/" + final_request, false);
		xhr.send();
		return xhr.responseText;
	};
	this.sendCommandAsync = async function (request) {

		var xhr = new XMLHttpRequest();
		var final_request = "";

		if (request.includes("?")) {
			if (request.endsWith("?")) {
				if (login_using_token) final_request = request + "login_username=" + login_username + "&login_token=" + login_token;
				else final_request = request + "login_username=" + login_username + "&login_password=" + login_password;
			} else {
				if (login_using_token) final_request = request + "&login_username=" + login_username + "&login_token=" + login_token;
				else final_request = request + "&login_username=" + login_username + "&login_password=" + login_password;
			}
		} else {
			if (login_using_token) final_request = request + "?login_username=" + login_username + "&login_token=" + login_token;
			else final_request = request + "?login_username=" + login_username + "&login_password=" + login_password;
		}

		xhr.open('GET', this.protocol + "://" + this.host + "/api/" + final_request, true);
		xhr.send();
		return;
	};
	this.buildRequest = function (request) {

		var xhr = new XMLHttpRequest();
		var final_request = "";

		if (request.includes("?")) {
			if (request.endsWith("?")) {
				if (login_using_token) final_request = request + "login_username=" + login_username + "&login_token=" + login_token;
				else final_request = request + "login_username=" + login_username + "&login_password=" + login_password;
			} else {
				if (login_using_token) final_request = request + "&login_username=" + login_username + "&login_token=" + login_token;
				else final_request = request + "&login_username=" + login_username + "&login_password=" + login_password;
			}
		} else {
			if (login_using_token) final_request = request + "?login_username=" + login_username + "&login_token=" + login_token;
			else final_request = request + "?login_username=" + login_username + "&login_password=" + login_password;
		}

		xhr.open('GET', this.protocol + "://" + this.host + "/api/" + final_request, true);
		return xhr;
	};
	this.sendCommand = function (request, redirectUrl, redirectWrongPassword) {

		var xhr = new XMLHttpRequest();
		var final_request = "";

		if (typeof login_username == 'undefined' || login_username === "") {

			if (login_token === "wrongpwd") {
				window.location = redirectWrongPassword;
			}
			else {
				window.location = redirectUrl;
			}
			return;
		}
		if (login_token === "wrongpwd") {
			window.location = redirectWrongPassword;
			return;
		}
		if (request.includes("?")) {
			if (request.endsWith("?")) {
				if (login_using_token) final_request = request + "login_username=" + login_username + "&login_token=" + login_token;
				else final_request = request + "login_username=" + login_username + "&login_password=" + login_password;
			} else {
				if (login_using_token) final_request = request + "&login_username=" + login_username + "&login_token=" + login_token;
				else final_request = request + "&login_username=" + login_username + "&login_password=" + login_password;
			}
		} else {
			if (login_using_token) final_request = request + "?login_username=" + login_username + "&login_token=" + login_token;
			else final_request = request + "?login_username=" + login_username + "&login_password=" + login_password;
		}
		xhr.open('GET', this.protocol + "://" + this.host + "/api/" + final_request, false);
		xhr.send();
		return xhr.responseText;
	};
	this.generateToken = function (username, password) {
		var xhr = new XMLHttpRequest();
		xhr.open('GET', this.protocol + "://" + this.host + "/api/user/openSession?login_username=" + username + "&login_password=" + password, false);
		xhr.send();
		var parsed = JSON.parse(xhr.responseText);
		if (parsed.status == 0) {
			return parsed.token;
		}
		else return "wrongpwd";
	};
	login_token = "";
	if (Cookies.get('b0689') == null || Cookies.get('1a1dc') == null || Cookies.get('1a1dc') === "null") {
	}
	else {
		login_username = Cookies.get('b0689');
		login_token = Cookies.get('1a1dc');
		login_using_token = true;
	}

	if (sessionStorage.getItem("b0689") != null && sessionStorage.getItem("1a1dc") != null || sessionStorage.getItem("1a1dc") === "null") {
		login_username = sessionStorage.getItem('b0689');
		login_token = sessionStorage.getItem('1a1dc');
		var test = sessionStorage.getItem("b0689fff");
		login_using_token = true;
	}

	if (get('login_username') != null || get('login_password') != null) {
		if (get('login_username') == null) {
			login_username = "admin";
		}
		else {
			login_username = get('login_username');
		}
		login_password = get('login_password');
		login_using_token = false;

		Cookies.set('b0689', login_username, { expires: 30 });
		Cookies.set('1a1dc', this.generateToken(login_username, login_password), { expires: 30 });
	}
}
function get(name) {
	if (name = (new RegExp('[?&]' + encodeURIComponent(name) + '=([^&]*)')).exec(location.search))
		return decodeURIComponent(name[1]);
}