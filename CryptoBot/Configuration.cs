using Microsoft.Extensions.Configuration;
using System.IO;

namespace CryptoBot
{
    public class Configuration
    {
        private IConfigurationRoot _configurationRoot;

        public Configuration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configurationRoot = builder.Build();
        }

        public string GetConnectionString()
        {
            return _configurationRoot["ConnectionString"];
        }

        public string GetTelegramBotToken()
        {
            return _configurationRoot["TelegramBotToken"];
        }

        public string GetGptApiKey()
        {
            return _configurationRoot["GptApiKey"];
        }
        public string GetCmcApiKey()
        {
            return _configurationRoot["CoinMarketCapApiKey"];
        }
    }
}
