import discord
import os
import logging

if not os.path.exists("logs/discord.log"):
    if not os.path.exists("logs/"):
        os.mkdir("logs")
    open("logs/discord.log", "w")

logging.basicConfig(filename="logs/discord.log", level=logging.INFO)


def main():
    pass


if __name__ == '__main__':
    main()