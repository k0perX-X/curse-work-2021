import web
import discord
import telegram
from threading import Thread


if __name__ == '__main__':
    telegram_thread = Thread(target=telegram.main)
    discord_thread = Thread(target=discord.main)
    web_thread = Thread(target=web.main)

    telegram_thread.start()
    discord_thread.start()
    web_thread.start()
