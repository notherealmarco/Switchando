/*using HomeAutomation.Logging.Telegram;
using HomeAutomationCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomation.Logging
{
    public static class Logger
    {
        private static bool Telegram;
        public static void InitTelegram(string token, long logId, long alertId)
        {
            HomeAutomationServer.server.Telegram = new TelegramBot(token);
            HomeAutomationServer.server.Telegram.SetLogChat(logId);
            HomeAutomationServer.server.Telegram.SetLogChat(alertId);
            Telegram = true;
        }
        public static void Log(string msg)
        {
            if (Telegram) HomeAutomationServer.server.Telegram.Log(msg);
        }
        public static void Alert(string msg)
        {
            if (Telegram) HomeAutomationServer.server.Telegram.Alert(msg);
        }
    }
}*/