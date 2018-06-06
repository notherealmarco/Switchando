using HomeAutomation.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Timers;

namespace Switchando.Collector
{
    public static class SwitchandoCollector
    {
        private static Timer _timer;
        public static void Enable()
        {
            _timer = new Timer();
            _timer.Interval = 300000;
            _timer.Elapsed += Elapsed;
            _timer.Start();
            Elapsed(null, null);
        }
        private static void Elapsed(object a, object b)
        {
            try
            {
                if (File.Exists(Paths.GetServerPath() + "/collector.disabled"))
                {
                    Disable();
                    return;
                }
                var url = "https://cloud.switchando.com/swcapi/account/telemetry/" + HomeAutomationCore.HomeAutomationServer.server.UUID + "/" + HomeAutomationCore.HomeAutomationServer.server.Version + "/" + System.Runtime.InteropServices.RuntimeInformation.OSDescription.Split(' ')[0] + "/" + System.Runtime.InteropServices.RuntimeInformation.OSArchitecture;
                string r = new WebClient().DownloadString(url);
                if (!r.Equals("thanks!")) Console.WriteLine("COLLECTOR | Internal server error -> " + r);
            }
            catch(Exception e)
            {
                Console.WriteLine("COLLECTOR | Exception -> " + e.Message);
            }
        }
        public static void Disable()
        {
            _timer.Stop();
        }
    }
}