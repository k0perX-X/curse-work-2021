using System;
using System.Collections;
using Telegram.Bot;
using Database;
using Telegram.Bot.Args;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    public static class Program
    {
#if DEBUG
        private static readonly Logging Logging = new Logging(Logging.LevelLogging.DEBUG, "Telegram.log", true);
#else
        private static readonly Logging Logging = new Logging(Logging.LevelLogging.INFO, "Telegram.log", true);
#endif

        private static TelegramBotClient _botClient;
        private static Telegram.Bot.Types.User _me;

        public static void Main()
        {
            _botClient = new TelegramBotClient(Configuration.BotToken);

            _me = _botClient.GetMeAsync().Result;

            Console.WriteLine($"Hello, World! I am user {_me.Id} and my name is {_me.FirstName}.");

            _botClient.OnMessage += Bot_OnMessage;
            _botClient.OnMessageEdited += Bot_OnMessage;
            _botClient.OnCallbackQuery += Bot_InlineKeyboardButton;

            Processing.TelegramUserTimerMinute += EveryMinuteEvent;

            _botClient.StartReceiving();
            while (Console.ReadLine() != "exit") { }

            _botClient.CloseAsync();
        }

        private static void Bot_InlineKeyboardButton(object sender, CallbackQueryEventArgs e)
        {
            try
            {
                var s = e.CallbackQuery.Data.Split("|");
                if (s.Length > 0)
                    switch (s[0])
                    {
                        case "More":
                            Bot_MoreCityInfo(s, sender, e);
                            break;

                        case "NextLetter":
                            Bot_NextLetter(s, sender, e);
                            break;

                        case "Restart":
                            Bot_RestartUser(s, sender, e);
                            break;
                    }
            }
            catch (Exception exception)
            {
                var s = $"Bot_InlineKeyboardButton - e.Message.Text: {e.CallbackQuery.Message.Text} - e.Message.Chat.Id: {e.CallbackQuery.Message.Chat.Id} - {exception}";
                Logging.ERROR(s);
            }
        }

        private static void Bot_RestartUser(string[] s, object sender, CallbackQueryEventArgs e)
        {
            Processing.DeleteUser(s[1]);
            Bot_StartCommand(sender, new MessageEventArgs(e.CallbackQuery.Message));
        }

        private static async void EveryMinuteEvent(string ID, Processing.User user, DateTime currentTime)
        {
            if (user.LastCall > currentTime.AddHours(6 * Math.Pow(2, user.RequestsToContinueGame)) && user.RequestsToContinueGame < 4)
            {
                try
                {
                    await _botClient.SendTextMessageAsync(
                        chatId: ID.Replace("Telegram.", ""),
                        text: $"Нашему боту скучно, поиграйте с ним.\nВам на букву {user.NextLetter}",
                        disableNotification: true
                    );
                }
                catch (Exception exception)
                {
                    var message = $"EveryMinuteEvent - ID: {ID} - User {user} - {exception}";
                    Logging.ERROR(message);
                }
                user.RequestsToContinueGame++;
            }
        }

        private static async void Bot_MoreCityInfo(string[] s, object sender, CallbackQueryEventArgs e)
        {
            try
            {
                Processing.City city = Processing.DatabaseFind[s[1].ToLower()[0]].First(x => x.Name == s[1]);
                await _botClient.SendLocationAsync(
                    chatId: e.CallbackQuery.Message.Chat.Id,
                    latitude: (float)city.Latitude,
                    longitude: (float)city.Longitude,
                    disableNotification: true,
                    replyToMessageId: e.CallbackQuery.Message.MessageId
                );
                await _botClient.SendTextMessageAsync(
                    chatId: e.CallbackQuery.Message.Chat.Id,
                    text: city.WikiSummary,
                    parseMode: ParseMode.Html
                );
            }
            catch (Exception exception)
            {
                var message = $"Bot_MoreCityInfo - e.Message.Chat.Id: {e.CallbackQuery.Message.Chat.Id} - {exception}";
                Logging.ERROR(message);
            }
        }

        private static async void Bot_NextLetter(string[] s, object sender, CallbackQueryEventArgs e)
        {
            try
            {
                await _botClient.SendTextMessageAsync(
                    chatId: e.CallbackQuery.Message.Chat.Id,
                    text: $"Теперь вам на букву {Processing.NextLetterUser("Telegram." + e.CallbackQuery.Message.Chat.Id.ToString())}",
                    parseMode: ParseMode.Html
                );
            }
            catch (Exception exception)
            {
                var message = $"Bot_NextLetter - e.Message.Chat.Id: {e.CallbackQuery.Message.Chat.Id} - {exception}";
                Logging.ERROR(message);
            }
        }

        private static void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                switch (e.Message.Text[0])
                {
                    case '/':
                        Bot_Commands(sender, e);
                        break;

                    default:
                        Bot_Get(sender, e);
                        break;
                }
            }
            catch (Exception exception)
            {
                var s = $"Bot_OnMessage - e.Message.Text: {e.Message.Text} - e.Message.Chat.Id: {e.Message.Chat.Id} - {exception}";
                Logging.ERROR(s);
            }
        }

        private static void Bot_Commands(object sender, MessageEventArgs e)
        {
            try
            {
                string s = e.Message.Text.Replace("/", "").Split()[0];
                switch (s)
                {
                    case "start":
                        Bot_StartCommand(sender, e);
                        break;

                    case "restart":
                        Bot_RestartCommand(sender, e);
                        break;
                }
            }
            catch (IndexOutOfRangeException exception) { }
            catch (Exception exception)
            {
                var s = $"Bot_Commands - e.Message.Text: {e.Message.Text} - e.Message.Chat.Id: {e.Message.Chat.Id} - {exception}";
                Logging.ERROR(s);
            }
        }

        private static async void Bot_StartCommand(object sender, MessageEventArgs e)
        {
            if (Processing.DatabaseUsers.ContainsKey("Telegram." + e.Message.Chat.Id.ToString()))
            {
                Bot_RestartCommand(sender, e);
            }
            else
            {
                try
                {
                    await _botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        text: "Приветствую! Наш бот предлагает вам сыграть в города!\n" +
                              "Правила игры: игра, в которой каждый участник в свою " +
                              "очередь называет реально существующий город России, " +
                              "название которого начинается на ту букву, которой " +
                              "оканчивается название предыдущего города(если это возможно), " +
                              "если города на эту букву кончились или не существуют, " +
                              "то берется следующая по счету с конца буквы предыдущего города.\n" +
                              "Введите первый город на любую букву."
                    );
                }
                catch (Exception exception)
                {
                    var s = $"Bot_StartCommand - e.Message.Text: {e.Message.Text} - e.Message.Chat.Id: {e.Message.Chat.Id} - {exception}";
                    Logging.ERROR(s);
                }
            }
        }

        private static async void Bot_RestartCommand(object sender, MessageEventArgs e)
        {
            if (!Processing.DatabaseUsers.ContainsKey("Telegram." + e.Message.Chat.Id.ToString()))
            {
                Bot_StartCommand(sender, e);
            }
            else
            {
                try
                {
                    await _botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat.Id,
                        text: $"Вы точно хотите начать игру заново?",
                        replyMarkup: new InlineKeyboardMarkup(
                            new List<List<InlineKeyboardButton>>
                            {
                                new List<InlineKeyboardButton>
                                {
                                    InlineKeyboardButton.WithCallbackData("Да",
                                        $"Restart|{"Telegram." + e.Message.Chat.Id.ToString()}"),
                                },
                            })
                    );
                }
                catch (Exception exception)
                {
                    var s = $"Bot_RestartCommand - e.Message.Text: {e.Message.Text} - e.Message.Chat.Id: {e.Message.Chat.Id} - {exception}";
                    Logging.ERROR(s);
                }
            }
        }

        private static async void Bot_Get(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                string city = e.Message.Text;
                Processing.Get(ref city, "Telegram." + e.Message.Chat.Id.ToString(), out bool onLastLetter, out bool cityIsUsed, out string outCity,
                    out char nextLetter, out byte letterNumberFromEnd, out string wikiUrl, out string yandexUrl, out string googleUrl, out string mapUrl,
                    out (decimal latitude, decimal longitude) coordinateCity, out string photoUrl);
                Logging.DEBUG($"Telegram: Chat: {e.Message.Chat.Id}, Text: {e.Message.Text} " +
                              $"onLastLetter {onLastLetter}, cityIsUsed {cityIsUsed}, outCity {outCity}, nextLetter {nextLetter}, letterNumberFromEnd {letterNumberFromEnd}, " +
                              $"wikiUrl {wikiUrl}, yandexUrl {yandexUrl}, googleUrl {googleUrl}, mapUrl {mapUrl}, coordinateCity.latitude {coordinateCity.latitude}, " +
                              $"coordinateCity.longitude {coordinateCity.longitude}, photoUrl {photoUrl}");

                if (cityIsUsed == true)
                {
                    try
                    {
                        if (letterNumberFromEnd != Processing.DatabaseUsers["Telegram." + e.Message.Chat.Id.ToString()].OutCity.Length - 1)
                        {
                            await _botClient.SendTextMessageAsync(
                                chatId: e.Message.Chat,
                                text: $"Город {e.Message.Text} уже был использован, попробуйте ввести другой город на эту же букву",
                                replyMarkup: new InlineKeyboardMarkup(
                                    new List<List<InlineKeyboardButton>>
                                    {
                                        new List<InlineKeyboardButton>
                                        {
                                            InlineKeyboardButton.WithCallbackData("Попробовать на следующую букву",
                                                $"NextLetter")
                                        },
                                    })
                            );
                        }
                        else
                        {
                            await _botClient.SendTextMessageAsync(
                                chatId: e.Message.Chat,
                                text: $"Город {e.Message.Text} уже был использован, попробуйте ввести другой город на эту же букву"
                            );
                        }
                    }
                    catch (Exception exception)
                    {
                        var s = $"CityIsUsed - e.Message.Text: {e.Message.Text} - e.Message.Chat.Id: {e.Message.Chat.Id} - {exception}";
                        Logging.ERROR(s);
                    }
                }
                else if (onLastLetter == false)
                {
                    try
                    {
                        if (letterNumberFromEnd != Processing.DatabaseUsers["Telegram." + e.Message.Chat.Id.ToString()].OutCity.Length - 1)
                        {
                            await _botClient.SendTextMessageAsync(
                                chatId: e.Message.Chat,
                                text: $"Город {e.Message.Text} начинается не на требуемую букву предыдущего города. Ваша буква {nextLetter}.",
                                replyMarkup: new InlineKeyboardMarkup(
                                    new List<List<InlineKeyboardButton>>
                                    {
                                        new List<InlineKeyboardButton>
                                        {
                                            InlineKeyboardButton.WithCallbackData("Попробовать на следующую букву",
                                                "NextLetter")
                                        },
                                    })
                            );
                        }
                        else
                        {
                            await _botClient.SendTextMessageAsync(
                                chatId: e.Message.Chat,
                                text: $"Город {e.Message.Text} начинается не на требуемую букву предыдущего города. Ваша буква {nextLetter}."
                            );
                        }
                    }
                    catch (Exception exception)
                    {
                        var s = $"NotOnLastLetter - e.Message.Text: {e.Message.Text} - e.Message.Chat.Id: {e.Message.Chat.Id} - {exception}";
                        Logging.ERROR(s);
                    }
                }
                else if (outCity == null)
                {
                    try
                    {
                        if (letterNumberFromEnd != Processing.DatabaseUsers["Telegram." + e.Message.Chat.Id.ToString()]
                            .OutCity.Length - 1)
                        {
                            await _botClient.SendTextMessageAsync(
                                chatId: e.Message.Chat,
                                text: $"Город {e.Message.Text} не найден в базе данных, попробуйте ввести другой город на эту же букву.",
                                replyMarkup: new InlineKeyboardMarkup(
                                    new List<List<InlineKeyboardButton>>
                                    {
                                        new List<InlineKeyboardButton>
                                        {
                                            InlineKeyboardButton.WithCallbackData("Попробовать на следующую букву",
                                                "NextLetter")
                                        },
                                    })
                            );
                        }
                        else
                        {
                            await _botClient.SendTextMessageAsync(
                                chatId: e.Message.Chat,
                                text: $"Город {e.Message.Text} не найден в базе данных, попробуйте ввести другой город на эту же букву."
                            );
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                        await _botClient.SendTextMessageAsync(
                            chatId: e.Message.Chat,
                            text: $"Город {e.Message.Text} не найден в базе данных."
                        );
                    }
                    catch (Exception exception)
                    {
                        var s = $"OutCityNull - e.Message.Text: {e.Message.Text} - e.Message.Chat.Id: {e.Message.Chat.Id} - {exception}";
                        Logging.ERROR(s);
                    }
                }
                else
                {
                    try
                    {
                        await _botClient.SendChatActionAsync(
                            chatId: e.Message.Chat.Id,
                            chatAction: ChatAction.UploadPhoto
                        );
                        await _botClient.SendPhotoAsync(
                                chatId: e.Message.Chat.Id,
                                caption: $"<b>{outCity}</b>",
                                parseMode: ParseMode.Html,
                                photo: photoUrl,
                                replyToMessageId: e.Message.MessageId,
                                replyMarkup: new InlineKeyboardMarkup(
                                    new List<List<InlineKeyboardButton>>
                                    {
                                            new List<InlineKeyboardButton>
                                            {
                                                InlineKeyboardButton.WithUrl("Википедия", wikiUrl),
                                                InlineKeyboardButton.WithUrl("Google", googleUrl),
                                                InlineKeyboardButton.WithUrl("Яндекс", yandexUrl)
                                            },
                                            new List<InlineKeyboardButton>
                                            {
                                                InlineKeyboardButton.WithCallbackData("Больше информации о городе",
                                                    $"More|{outCity}")
                                            },
                                    })
                                );
                    }
                    catch (Telegram.Bot.Exceptions.ApiRequestException)
                    {
                        try
                        {
                            await _botClient.SendTextMessageAsync(
                                chatId: e.Message.Chat,
                                text: $"<b>{outCity}</b>",
                                parseMode: ParseMode.Html,
                                replyToMessageId: e.Message.MessageId,
                                replyMarkup: new InlineKeyboardMarkup(
                                    new List<List<InlineKeyboardButton>>
                                    {
                                                new List<InlineKeyboardButton>
                                                {
                                                    InlineKeyboardButton.WithUrl("Википедия", wikiUrl),
                                                    InlineKeyboardButton.WithUrl("Google", googleUrl),
                                                    InlineKeyboardButton.WithUrl("Яндекс", yandexUrl)
                                                },
                                                new List<InlineKeyboardButton>
                                                {
                                                    InlineKeyboardButton.WithCallbackData("Больше информации о городе",
                                                        $"More|{outCity}")
                                                },
                                    })
                            );
                        }
                        catch (Exception exception)
                        {
                            var s = $"WithoutPic - outCity: {outCity} - e.Message.Text: {e.Message.Text} - e.Message.Chat.Id: {e.Message.Chat.Id} - {exception}";
                            Logging.ERROR(s);
                        }
                    }
                    catch (Exception exception)
                    {
                        var s = $"WithPic - outCity: {outCity} - e.Message.Text: {e.Message.Text} - e.Message.Chat.Id: {e.Message.Chat.Id} - {exception}";
                        Logging.ERROR(s);
                    }
                }
            }
        }
    }
}