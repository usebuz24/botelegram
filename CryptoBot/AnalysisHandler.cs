using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;

namespace CryptoBot
{
    internal class AnalysisHandler
    {
        private IWebDriver driver;
        private Configuration config = new Configuration();
        public AnalysisHandler()
        {
            driver = new ChromeDriver();
        }

        public async Task<string> GetAnalysisAsync(string slug)
        {
            var rawData = GetAboutSection(slug);
            string analysis = await CallGpt3Api(slug, rawData);
            // Return the generated analysis
            return analysis;
        }
        public string GetAboutSection(string cryptoName)
        {

            var url = $"https://coinmarketcap.com/currencies/{cryptoName}/";

            // Navigate to the page
            driver.Navigate().GoToUrl(url);

            // Check if the "Read More" span exists
            var readMoreSpans = driver.FindElements(By.XPath(".//span[text()='Read More']"));

            if (readMoreSpans.Count > 0)
            {
                // If the "Read More" span exists, get the text from the specified div
                var aboutSectionElement = driver.FindElement(By.XPath("//*[@id='__next']/div[2]/div[1]/div[2]/div/div[3]/div/div[1]/div[2]/div[4]/section/div/div/div/div[1]/div/div"));
                var aboutSectionHtml = aboutSectionElement.GetAttribute("innerHTML");

                // Load the HTML into HtmlAgilityPack
                var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(aboutSectionHtml);

                // Find all <p> elements and extract their text
                var paragraphs = htmlDoc.DocumentNode.SelectNodes("//p");
                var aboutSection = string.Join("\n", paragraphs.Select(p => p.InnerText));
                return aboutSection;
            }
            else
            {
                var aboutSectionElement = driver.FindElement(By.XPath("//*[@id='section-coin-about']"));

                // Get the HTML of the "About" section
                var aboutSectionHtml = aboutSectionElement.GetAttribute("innerHTML");

                // Load the HTML into HtmlAgilityPack
                var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(aboutSectionHtml);

                // Find all <p> elements and extract their text
                var paragraphs = htmlDoc.DocumentNode.SelectNodes("//p");
                var aboutSection = string.Join("\n", paragraphs.Select(p => p.InnerText));

                Console.WriteLine(aboutSection.ToString());
                return aboutSection;
            }
        }
        public async Task<string> CallGpt3Api(string slug, string rawData)
        {
            // Create a new HttpClient instance
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(5);
                string endpoint = "https://api.openai.com/v1/chat/completions"; 

                var messages = new[]
                {
                    new {role = "system", content = $"Given the following information about the cryptocurrency {slug}, generate a comprehensive fundamental analysis that includes an overview of the cryptocurrency, its potential future prospects in the space, and any risks or concerns. The analysis must be structured with an introduction, a discussion section discussing the points above, and a conclusion summarizing the findings.Be skeptical and inquisitive, I need to understand if the project really has prospects or if it is another failure. Analysis MUST be NO LONGER than 4000 characters"},
                    new {role = "user", content = rawData}
                };
                var data = new
                {
                    model = "gpt-3.5-turbo-16k",
                    messages = messages,
                    temperature = 1
                };
                string jsonString = JsonConvert.SerializeObject(data);

                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.GetGptApiKey()}");

                var response = await client.PostAsync(endpoint, content);

                var responseContent = await response.Content.ReadAsStringAsync();

                var jsonResponse = JObject.Parse(responseContent);

                var generatedText = jsonResponse["choices"][0]["message"]["content"].Value<string>();

                return generatedText;
            }
        }
    }
}
