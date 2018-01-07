using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Homeautomation.GPIO
{
    public static class PIGPIO
    {
        [DllImport("pigpiod_if2.so", EntryPoint = "pigpio_start")]
        public static extern int pigpio_start(string addStr, string portStr);
        [DllImport("pigpiod_if2.so", EntryPoint = "set_PWM_dutycycle")]
        public static extern int set_PWM_dutycycle(int pi, uint user_gpio, uint dutycycle);
        [DllImport("pigpiod_if2.so", EntryPoint = "set_PWM_frequency")]
        public static extern int set_PWM_frequency(int pi, uint user_gpio, uint frequency);
        [DllImport("pigpiod_if2.so", EntryPoint = "gpio_read")]
        public static extern int gpio_read(int pi, uint gpio);
        [DllImport("pigpiod_if2.so", EntryPoint = "set_pull_up_down")]
        public static extern int set_pull_up_down(int pi, uint gpio, uint pud);
        [DllImport("pigpiod_if2.so", EntryPoint = "gpio_write")]
        public static extern int gpio_write(int pi, uint gpio, uint level);
    }
}
