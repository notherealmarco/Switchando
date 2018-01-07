/*using HomeAutomation.Objects;
using HomeAutomation.Rooms;
using HomeAutomationCore;
using System;

namespace HomeAutomation.Network.Interfaces.Voice
{
    class VoiceInterface
    {
        NetworkInterface networkInterface;

        public VoiceInterface()
        {
            this.networkInterface = new NetworkInterface("voice", Handler);
        }

        public static string Handler(string[] request)
        {
            string identity = "my";
            foreach (string cmd in request)
            {
                string[] command = cmd.Split('=');
                if (command[0].Equals("interface")) continue;
                switch (command[0])
                {
                    case "identity":
                        identity = command[1];
                        break;
                    case "command":

                        string voice = command[1];
                        HomeAutomationServer.server.Telegram.Log("Voice command -> `" + voice + "`.");
                        if (voice.Contains("my ")) voice = voice.Replace("my ", identity.ToLower() + "'s ");
                        if (voice.Contains(" the ")) voice = voice.Replace(" the ", " ");
                        if (voice.StartsWith("turn"))
                        {
                            bool status;
                            voice = voice.Substring(5);
                            if (voice.StartsWith("on"))
                            {
                                status = true;
                                voice = voice.Substring(3);
                                Answer(voice, status, identity);
                                return "";
                            }
                            else if (voice.StartsWith("off"))
                            {
                                status = false;
                                voice = voice.Substring(4);
                                Answer(voice, status, identity);
                                return "";
                            }
                            else if (voice.EndsWith("on"))
                            {
                                status = true;
                                voice = voice.Substring(0, voice.Length - 3);
                                Answer(voice, status, identity);
                                return "";
                            }
                            else if (voice.EndsWith("off"))
                            {
                                status = false;
                                voice = voice.Substring(0, voice.Length - 4);
                                Answer(voice, status, identity);
                                return "";
                            }
                        }
                        break;
                }
            }
            return "";
        }
        static void Answer(string voice, bool status, string identity)
        {
            if (voice.Equals("lights")) voice = identity + "'s lights";
            Console.WriteLine(voice);
            string objname = voice;
            Console.WriteLine(status);
            string[] message = new string[] { "objname=" + objname, "switch=" + status };
            NetworkInterface.FromId("auto").Run(message);
        }
    }
}
*/