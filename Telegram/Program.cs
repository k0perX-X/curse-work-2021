using System;
using Telegram.Bot;
using Database;
using Telegram.Bot.Args;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram
{
    class Program
    {
        private static Logging logging = new Logging(Logging.Level.DEBUG, "Telegram.log", true);

        static TelegramBotClient botClient;
        static Bot.Types.User me;

    


        static void Main()
        {
            Processing.ReadCsv();   
            botClient = new TelegramBotClient(Configuration.BotToken);

            me = botClient.GetMeAsync().Result;

            Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");

            //Bot.Types.ReplyMarkups.InlineKeyboardMarkup keyboard = new Bot.Types.ReplyMarkups.InlineKeyboardMarkup(new[]
            //                {
            //                    new[] { Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Первая", "callback1") },
            //                    new[] { Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Вторая", "callback2") },
            //                });

            //botClient.OnCallbackQuery += async (object sender, CallbackQueryEventArgs ev) =>
            //{
            //    var message = ev.CallbackQuery.Message;
            //    if (ev.CallbackQuery.Data == "callback1")
            //    {
            //        // сюда то что тебе нужно сделать при нажатии на первую кнопку 
            //    }
            //    else
            //    if (ev.CallbackQuery.Data == "callback2")
            //    {
            //        // сюда то что нужно сделать при нажатии на вторую кнопку
            //    }
            //};

            //botClient.OnMessage += Bot_FirstOnMessage;
            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            botClient.StopReceiving();

            Console.ReadKey();


        }

        static async void Bot_FirstOnMessage(object sender, MessageEventArgs e)
        {
            await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Введите первый город");
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                //var keyboard = new InlineKeyboardMarkup(new Bot.Types.InlineKeyboardButton[][]
                //{new [] {
                //        new Telegram.Bot.Types.InlineKeyboardButton("Текст для первой кнопки","callback1"),
                //        new Telegram.Bot.Types.InlineKeyboardButton("Текст для второй кнопки","callback2"),
                //    },
                //});
                //await botClient.SendTextMessageAsync(e.Message.Chat, "Жамкни!", replyMarkup: keyboard);
                logging.INFO($"Telegram: Chat: {e.Message.Chat.Id}, Text: {e.Message.Text}");
                Processing.Get(e.Message.Text, "Telegram." + e.Message.Chat.Id.ToString(), out bool onLastLetter, out bool cityIsUsed, out string outCity, out char nextLetter, out int letterNumberFromEnd, out string wikiUrl,
            out string yandexUrl, out string googleUrl, out string mapUrl, out (double latitude, double longitude) coordinateCity, out string photoUrl);

                if (cityIsUsed == true)
                    await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: $"Город {e.Message.Text} уже был использован, попробуйте ввести другой город на эту же букву");
                else if (onLastLetter == false)
                    await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: $"Город {e.Message.Text} начинается не на последнюю букву предыдущего города, попробуйте ещё раз");
                else if (outCity == null)
                    await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: $"Город {e.Message.Text} не найден в базе данных, попробуйте ввести другой город на эту же букву");
                else await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: $"{outCity}", replyToMessageId: e.Message.MessageId);
                

            }   
        }
        private void processUpdate(Bot.Types.Update update)
        {
            switch (update.Type)
            {
                case Bot.Types.Enums.UpdateType.Message:
                    var text = update.Message.Text;
                    switch (text)
                    {
                        case TEXT_1:
                            botClient.SendTextMessageAsync(update.Message.Chat.Id, "Кнопка 1",  replyMarkup:  GetButtons());
                            break;
                        case TEXT_2:
                            botClient.SendTextMessageAsync(update.Message.Chat.Id, "Кнопка 2", replyMarkup: GetButtons());
                            break;
                        case TEXT_3:
                            botClient.SendTextMessageAsync(update.Message.Chat.Id, "Кнопка 3", replyMarkup: GetButtons());
                            break;
                        case TEXT_4:
                            botClient.SendTextMessageAsync(update.Message.Chat.Id, "Кнопка 4", replyMarkup: GetButtons());
                            break;
                    }
                    botClient.SendTextMessageAsync(update.Message.Chat.Id, "Receive text:" + text, replyMarkup: GetButtons());
                    break;
                default:  Console.WriteLine(update.Type + "Not implemented!");
                    break;
            }
        }















            const string TEXT_1 = "Один";
            const string TEXT_2 = "Два";
            const string TEXT_3 = "Три";
            const string TEXT_4 = "Четыре";
        private Bot.Types.ReplyMarkups.IReplyMarkup GetButtons()
        {
            return new Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
            {
                Keyboard = new List<List<Bot.Types.ReplyMarkups.KeyboardButton>>
                    {
                new List<Bot.Types.ReplyMarkups.KeyboardButton> { new Bot.Types.ReplyMarkups.KeyboardButton { Text = TEXT_1 }, new Bot.Types.ReplyMarkups.KeyboardButton { Text = TEXT_2 }, },
                new List<Bot.Types.ReplyMarkups.KeyboardButton> { new Bot.Types.ReplyMarkups.KeyboardButton { Text = TEXT_3 }, new Bot.Types.ReplyMarkups.KeyboardButton { Text = TEXT_4 }, }
                    }
            };
        }
    }
}
