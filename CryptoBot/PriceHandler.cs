using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CryptoBot
{
    public class PriceHandler
    {
        public static async Task<string> GetCryptocurrencyPrice(string symbol, string apiKey)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", apiKey);
                string url = $"https://pro-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest?symbol={symbol}";
                HttpResponseMessage response = await client.GetAsync(url);
                await Console.Out.WriteLineAsync(url);
                await Console.Out.WriteLineAsync(apiKey);
                await Console.Out.WriteLineAsync(symbol);
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    JsonDocument document = JsonDocument.Parse(responseContent);
                    JsonElement root = document.RootElement;
                    decimal price = root.GetProperty("data").GetProperty(symbol).GetProperty("quote").GetProperty("USD").GetProperty("price").GetDecimal();
                    string formattedPrice = price >= 1 ? price.ToString("0.00") : price.ToString("0.0000000");
                    return formattedPrice;
                }
                else
                {
                    return $"Error: {response.StatusCode}";
                }
            }
        }
        public static async Task<bool> TickerExists(string apiKey, string symbol)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", apiKey);

                string url = $"https://pro-api.coinmarketcap.com/v1/cryptocurrency/map?symbol={symbol}";
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    JsonDocument document = JsonDocument.Parse(responseContent);
                    JsonElement root = document.RootElement;

                    return root.GetProperty("status").GetProperty("error_code").GetInt32() == 0;
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                    return false;
                }
            }
        }
        public static async Task<string> GetSlug(string apiKey, string symbol)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", apiKey);
                string url = $"https://pro-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest?symbol={symbol}";
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    JsonDocument document = JsonDocument.Parse(responseContent);
                    JsonElement root = document.RootElement;
                    string slug = root.GetProperty("data").GetProperty(symbol).GetProperty("slug").GetString();
                    return slug;
                }
                else
                {
                    return $"Error: {response.StatusCode}";
                }
            }
        }
    }
}
