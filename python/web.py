from flask import Flask
import json
import requests
import logging
import os

if not os.path.exists("logs/site.log"):
    if not os.path.exists("logs/"):
        os.mkdir("logs")
    open("logs/site.log", "w")

logging.basicConfig(filename="logs/site.log", level=logging.INFO)
app = Flask(__name__)


@app.route('/')
def hello_world():
    return 'Hello World!'


def main():
    app.run()


if __name__ == '__main__':
    main()
