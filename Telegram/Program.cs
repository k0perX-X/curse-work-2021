using System;
using Telegram.Bot;
using Database;
using Telegram.Bot.Args;

namespace Telegram
{
    class Program
    {
        static ITelegramBotClient botClient;
        static void Main()
        {
            Processing.Get(string city, string id, out bool cityIsUsed, out string outCity, out int letterNumberFromEnd, out string wikiUrl,
            out string yandexUrl, out string googleUrl, out string mapUrl, out (double latitude, double longitude) coordinateCity, out string photoUrl);
            var botClient = new TelegramBotClient(Configuration.BotToken);
            var me = botClient.GetMeAsync().Result;
            Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");

            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

            botClient.StopReceiving();

            Console.ReadKey();
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                Console.WriteLine($"Получено письмо из чата {e.Message.Chat.Id}.");

                await botClient.SendTextMessageAsync(
                  chatId: e.Message.Chat,
                  text: "You said:\n" + e.Message.Text
                );
            }
        }
    }
}
