﻿using System;
using Telegram.Bot;
using Database;
using Telegram.Bot.Args;

namespace Telegram
{
    class Program
    {
        private static Logging logging = new Logging(Logging.Level.DEBUG, "Telegram.log");

        static ITelegramBotClient botClient;
        static void Main()
        {
            
            var botClient = new TelegramBotClient(Configuration.BotToken);
            var me = botClient.GetMeAsync().Result;
            Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");

            botClient.OnMessage += Bot_FirstOnMessage;
            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

            botClient.StopReceiving();

            Console.ReadKey();
        }

        static void Bot_FirstOnMessage(object sender, MessageEventArgs e)
        {
            Console.WriteLine("Введите первый город");
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                Console.WriteLine($"Получено письмо из чата {e.Message.Chat.Id}.");
                logging.INFO($"Получено письмо из чата {e.Message.Chat.Id}.");
                Processing.Get(e.Message.Text, "Telegram." + e.Message.Chat.Id.ToString(), out bool onLastLetter, out bool cityIsUsed, out string outCity, out int letterNumberFromEnd, out string wikiUrl,
            out string yandexUrl, out string googleUrl, out string mapUrl, out (double latitude, double longitude) coordinateCity, out string photoUrl);
                if (outCity == null)
                    await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: $"Город {e.Message.Text} не найден в базе данных, попробуйте ввести другой город на эту же букву");
                else if (onLastLetter == false)
                    await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: $"Город {e.Message.Text} начинается не на последнюю букву предыдущего города, попробуйте ещё раз");
                    else if (cityIsUsed == true)
                    await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: $"Вы уже использовали город {e.Message.Text}, попробуйте ввести другой город на эту же букву");
                    else await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: $"{outCity}");
            }
        }
    }
}
