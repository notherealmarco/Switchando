/*using HomeAutomationCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace HomeAutomation.Logging.Telegram
{
    public class TelegramBot
    {
        TelegramBotClient Bot;
        long LogChatId;
        long AlertChatId;
        public TelegramBot(string token)
        {
            if (token == null) return;
            Bot = new TelegramBotClient(token);
            var me = Bot.GetMeAsync().Result;
            Console.WriteLine("Telegram -> Loading profile of " + me.FirstName + " @ " + me.Username + "#" + me.Id + "...");
            Bot.OnUpdate += GetUpdates;

            //Bot.StartReceiving();
            //Bot.GetUpdatesAsync();
            
            Console.WriteLine("Telegram:" + token);
            HomeAutomationServer.server.Telegram = this;
        }
        public void GetUpdates(object sender, object eventargs)
        {

        }
        public void SetLogChat(long logChatId)
        {
            this.LogChatId = logChatId;
            Log("HomeAutomation 4 has been initialized!");
        }
        public void SetAlertChat(long alertChatId)
        {
            this.AlertChatId = alertChatId;
        }
        public void Log(string htmlMessage)
        {
            if (this.Bot == null) return;
            //Console.WriteLine("Telegram -> logging in " + LogChatId + "...");
            //Bot.SendTextMessageAsync(LogChatId, htmlMessage, false, false, 0, null, ParseMode.Markdown);
        }
        public void Alert(string htmlMessage)
        {
            if (this.Bot == null) return;
            Console.WriteLine("Telegram -> sending alert...");
            Bot.SendTextMessageAsync(AlertChatId, htmlMessage, false, false, 0, null, ParseMode.Html);
        }

    }
}
*/