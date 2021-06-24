﻿using System;
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
    public class Program
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
            _botClient.OnCallbackQuery += async (sender, e) =>
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
                    }
            };

            _botClient.StartReceiving();
            while (Console.ReadLine() != "exit") { }

            _botClient.CloseAsync();
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
                    text: $"",
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

        private static async void Bot_OnMessage(object sender, MessageEventArgs e)
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
                                text: $"Город {e.Message.Text} начинается не на последнюю букву предыдущего города, попробуйте ещё раз",
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
                                text: $"Город {e.Message.Text} начинается не на последнюю букву предыдущего города, попробуйте ещё раз"
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
                                text: $"Город {e.Message.Text} не найден в базе данных, попробуйте ввести другой город на эту же букву",
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
                                text: $"Город {e.Message.Text} не найден в базе данных, попробуйте ввести другой город на эту же букву"
                            );
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                        await _botClient.SendTextMessageAsync(
                            chatId: e.Message.Chat,
                            text: $"Город {e.Message.Text} не найден в базе данных, попробуйте ввести другой город на эту же букву"
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
                        if (wikiUrl != null)
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
                        else
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

        ~Program()
        {
            _botClient.CloseAsync();
        }
    }
}