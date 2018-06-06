# How to install Switchando on a Raspberry Pi (or any other armv7 and newer device)

We have made an auto installation script which installs Switchando (and .NET Core dependencies), some basics plugin (including the GPIO controller), [joan2937's pigpio](https://github.com/joan2937/pigpio) library and [Eclipse Mosquitto](https://github.com/eclipse/mosquitto) MQTT broker.

To install Switchando, you only have to paste this command in the terminal (the account needs sudo permissions, the script will try to get root access by sudo)

`wget https://get.switchando.com/switchando/install-switchando.sh && chmod +x install-switchando.sh && ./install-switchando.sh`

It will generate an **admin** user and ask you for a password. When the process is finished, you should be able to reach switchando at `<Raspberry IP>:8080`.

### Using another MQTT broker
If you already have an MQTT broker and you want to use it, just add the `--nomosquitto` argument and the script will ask you for an MQTT broker address, username and password.

***

_Make sure port 8080 and 1883 are open_
