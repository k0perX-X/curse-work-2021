using System;
using System.Collections;
using Telegram.Bot;
using Database;
using Telegram.Bot.Args;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    public class Program
    {
        private static readonly Logging Logging = new Logging(Logging.Level.DEBUG, "Telegram.log", true);

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
            _botClient.OnCallbackQuery += async (sender, e) =>
            {
                var s = e.CallbackQuery.Data.Split("|");
                if (s.Length > 0)
                    switch (s[0])
                    {
                        case "More":
                            Bot_MoreCityInfo(s, sender, e);
                            break;
                    }
            };

            _botClient.StartReceiving();
            while (Console.ReadLine() != "exit") { }
        }

        private static async void Bot_MoreCityInfo(string[] s, object sender, CallbackQueryEventArgs e)
        {
            try
            {
                Processing.City city = Processing.DatabaseFind[s[1].ToLower()[0]].First(x => x.Name == s[1]);
                try
                {
                    await _botClient.SendLocationAsync(
                        chatId: e.CallbackQuery.Message.Chat.Id,
                        latitude: (float)city.Latitude,
                        longitude: (float)city.Longitude,
                        disableNotification: true,
                        replyToMessageId: e.CallbackQuery.Message.MessageId
                    );
                    await _botClient.SendTextMessageAsync(
                        chatId: e.CallbackQuery.Message.Chat.Id,
                        text: $"",
                        parseMode: ParseMode.Html
                    );
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                Logging.INFO($"Telegram: Chat: {e.Message.Chat.Id}, Text: {e.Message.Text}");
                Processing.Get(e.Message.Text, "Telegram." + e.Message.Chat.Id.ToString(), out bool onLastLetter, out bool cityIsUsed, out string outCity,
                    out char nextLetter, out int letterNumberFromEnd, out string wikiUrl, out string yandexUrl, out string googleUrl, out string mapUrl,
                    out (decimal latitude, decimal longitude) coordinateCity, out string photoUrl);

                if (cityIsUsed == true)
                {
                    try
                    {
                        await _botClient.SendTextMessageAsync(e.Message.Chat,
                            $"Город {e.Message.Text} уже был использован, попробуйте ввести другой город на эту же букву");
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                        throw;
                    }
                }
                else if (onLastLetter == false)
                {
                    try
                    {
                        await _botClient.SendTextMessageAsync(e.Message.Chat,
                            $"Город {e.Message.Text} начинается не на последнюю букву предыдущего города, попробуйте ещё раз");
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                        throw;
                    }
                }
                else if (outCity == null)
                {
                    try
                    {
                        await _botClient.SendTextMessageAsync(e.Message.Chat,
                            $"Город {e.Message.Text} не найден в базе данных, попробуйте ввести другой город на эту же букву");
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                        throw;
                    }
                }
                else
                {
                    try
                    {
                        if (wikiUrl != null)
                        {
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
                        else
                        {
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
                    }
                    catch (Telegram.Bot.Exceptions.ApiRequestException)
                    {
                        try
                        {
                            try
                            {
                                if (wikiUrl != null)
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
                                else
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
                            }
                            catch (Exception exception)
                            {
                                var s = $"outCity: {outCity}\ne.Message.Text: {e.Message.Text}\ne.Message.Chat.Id: {e.Message.Chat.Id}\n{exception}";
                                Console.WriteLine(s);
                                Logging.ERROR(s);
                            }
                        }
                        catch (Exception exception)
                        {
                            var s = $"outCity: {outCity}\ne.Message.Text: {e.Message.Text}\ne.Message.Chat.Id: {e.Message.Chat.Id}\n{exception}";
                            Console.WriteLine(s);
                            Logging.ERROR(s);
                        }
                    }
                    catch (Exception exception)
                    {
                        var s = $"outCity: {outCity}\ne.Message.Text: {e.Message.Text}\ne.Message.Chat.Id: {e.Message.Chat.Id}\n{exception}";
                        Console.WriteLine(s);
                        Logging.ERROR(s);
                    }
                }
            }
        }
    }
}