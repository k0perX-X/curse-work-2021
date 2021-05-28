using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;


namespace WhatsApp
{
    public class WaApi
    {
        private string APIUrl = "";
        private string token = "";

        public WaApi(string aPIUrl, string token)
        {
            APIUrl = aPIUrl;
            this.token = token;
        }
        //асинхронный метод для отправки POST запросов
        public async Task<string> SendRequest(string method, string data)
        {
            string url = $"{APIUrl}{method}?token={token}";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                var content = new StringContent(data, Encoding.UTF8, "application/json");
                var result = await client.PostAsync("", content);
                return await result.Content.ReadAsStringAsync();
            }
        }
        //Отправка сообщений, отправляем json строку, которая фотримурется через библиотеку Newtonsoft.Json
        public async Task<string> SendMessage(string chatId, string text)
        {
            var data = new Dictionary<string, string>()
        {
            {"chatId",chatId },
            { "body", text }
        };
            return await SendRequest("sendMessage", JsonConvert.SerializeObject(data));
        }
        public async Task<string> CreateGroup(string author)
        {
            var phone = author.Replace("@c.us", "");
            var data = new Dictionary<string, string>()
        {
            { "groupName", "Group C#"},
            { "phones", phone },
            { "messageText", "This is your group." }
        };
            return await SendRequest("group", JsonConvert.SerializeObject(data));
        }
    }
}
