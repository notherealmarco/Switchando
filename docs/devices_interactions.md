# Devices interactions

### What does it mean?
In switchando, devices can interact with each other.
_When you turn on your Sonoff lamp, turn on also your LED strip._

### How can I do it?
You can use the tools provided in the config index. Once you have your sensor or your switch working in Switchando, in the config index you can configure it to turn on and off any other device, to run a specific Action (will talk about later) or, if you are an expert, directly execute an API request.

## Introducing Actions, Conditions and Events

### Actions
An Action is something that can be executed from any plugin, event or when you turn on and off a switch.
An example could be: _turn on your RGB LED strip and set the color to blue_.

### Conditions
An Action will be executed only if the given conditions are verified.
Conditions are variables created by plugins, for example, a plugin that manages RGB LED strip will create the on/off status, the color and the brightness variables.
An Action with some conditions could be: _Turn on the balcony lights only if it's dark outside_ (a light sensor has a variable that says if it's day or night).

### Events
Events are the one that trigger Actions, Events are also created by plugins.
An example can be: _When I switch my living room light on,_ (the plugin that manages the living room light fires an event when the light's on/off status changes) _execute the Action 'balcony_lights'_.
And the 'balcony_lights' Actions says: _Turn on the balcony lights only if it's dark outside_.

So now when I turn on my living room lights, if it's dark outside, the balcony lights will also turn on.
