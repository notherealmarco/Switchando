# Switchando for developers
Switchando offers two APIs, the Switchando API (version 4.0) and the Plugin API (version 3.1).

## Switchando API 4.0
This is the REST API for Switchando, it works both in HTTP and MQTT and this is currently the only way to comunicate to Switchando and its plugins. Plugins can create their own API 4.0 functions by using the Plugin API.
This API can be used also to create new devices and not only to control them.
The default Switchando UI uses this API.

## Plugin API 3.1
This is a C# library that allows you to create plugins (in any .NET Core supported language). You can use this API to add new functions to the Switchando REST API.
