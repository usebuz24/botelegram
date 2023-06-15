using HtmlAgilityPack;
using Microsoft.Data.SqlClient;
using OpenQA.Selenium.Chrome;
using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CryptoBot
{
    class Program
    {
        static async Task Main()
        {
            var botCommandHandler = new BotCommandHandler();
            JSONHandler jsonHandler = new JSONHandler("users.json");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}