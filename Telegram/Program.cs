using System;
using Telegram.Bot;
using Database;
using Telegram.Bot.Args;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    public class Program
    {
        private static readonly Logging _logging = new Logging(Logging.Level.DEBUG, "Telegram.log", true);

        private static TelegramBotClient _botClient;
        private static Telegram.Bot.Types.User _me;

        public static void Main()
        {
            Processing.ReadCsv();
            _botClient = new TelegramBotClient(Configuration.BotToken);

            _me = _botClient.GetMeAsync().Result;

            Console.WriteLine($"Hello, World! I am user {_me.Id} and my name is {_me.FirstName}.");

            //botClient.OnMessage += Bot_FirstOnMessage;
            _botClient.OnMessage += Bot_OnMessage;
            _botClient.StartReceiving();
            while (Console.ReadLine() != "exit") { }
        }

        private static async void Bot_FirstOnMessage(object sender, MessageEventArgs e)
        {
            await _botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Введите первый город");
        }

        private static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                _logging.INFO($"Telegram: Chat: {e.Message.Chat.Id}, Text: {e.Message.Text}");
                Processing.Get(e.Message.Text, "Telegram." + e.Message.Chat.Id.ToString(), out bool onLastLetter, out bool cityIsUsed, out string outCity,
                    out char nextLetter, out int letterNumberFromEnd, out string wikiUrl, out string yandexUrl, out string googleUrl, out string mapUrl,
                    out (decimal latitude, decimal longitude) coordinateCity, out string photoUrl);

                if (cityIsUsed == true)
                    await _botClient.SendTextMessageAsync(e.Message.Chat,
                        $"Город {e.Message.Text} уже был использован, попробуйте ввести другой город на эту же букву");
                else if (onLastLetter == false)
                    await _botClient.SendTextMessageAsync(e.Message.Chat,
                        $"Город {e.Message.Text} начинается не на последнюю букву предыдущего города, попробуйте ещё раз");
                else if (outCity == null)
                    await _botClient.SendTextMessageAsync(e.Message.Chat,
                        $"Город {e.Message.Text} не найден в базе данных, попробуйте ввести другой город на эту же букву");
                else
                    await _botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        text: outCity + $"\nMore|{outCity}\n|{wikiUrl ?? ""}\n|{yandexUrl ?? ""}\n|{googleUrl ?? ""}\n|{mapUrl ?? ""}\n|{coordinateCity.latitude}\n|{coordinateCity.longitude}\n|{photoUrl ?? ""}",
                        replyToMessageId: e.Message.MessageId,
                        replyMarkup: new InlineKeyboardMarkup(
                            InlineKeyboardButton.WithCallbackData("Больше информации о городе", $"More|{outCity}")
                        ));
            }
        }
    }
}