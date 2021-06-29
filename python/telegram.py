import telebot
import TOKENS
import logging
import os

if not os.path.exists("logs/telegram.log"):
    if not os.path.exists("logs/"):
        os.mkdir("logs")
    open("logs/telegram.log", "w")

logging.basicConfig(filename="logs/telegram.log", level=logging.INFO)
logging.info("Telegram bot initialized")
bot = telebot.TeleBot(TOKENS.telegram)


# Сообщения
help_message = "Для начала игры введите название города"


# Функции бота
@bot.message_handler(commands=['start', 'help'])
def send_welcome(message: telebot.types.Message):
    logging.info(F"help message - {message.chat}")
    try:
        bot.send_message(message.chat.id, help_message)
    except Exception as e:
        logging.error(F"help message - {e}")


def main():
    bot.polling()


if __name__ == '__main__':
    main()
