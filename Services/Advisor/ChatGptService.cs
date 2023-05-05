using FinancialAdvisorTelegramBot.Services.Advisor.RequestBody;
using FinancialAdvisorTelegramBot.Utils.Attributes;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Headers;
using System.Text;

namespace FinancialAdvisorTelegramBot.Services.Advisor
{
    [CustomService]
    public class ChatGptService : IChatGptService
    {
        private readonly OpenAiSettings _aiSettings;

        public ChatGptService(IOptions<OpenAiSettings> options)
        {
            _aiSettings = options.Value;
        }

        public async Task<ChatGptReturnBody?> CreateRequest(string prompt)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _aiSettings.ApiKey);
            
            string url = $"https://api.openai.com/v1/chat/completions";
            string requestBody = JsonConvert.SerializeObject(new ChatGptRequestBody()
            {
                Model = _aiSettings.Model,
                Messages = new List<ChatGptMessage>()
                {
                    new ChatGptMessage()
                    {
                        Role = "user",
                        Content = prompt
                    }
                }
            }, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            
            HttpContent httpContent = new StringContent(requestBody, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync(url, httpContent);

            if (response.IsSuccessStatusCode)
            {
                string responseText = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ChatGptReturnBody>(responseText,
                    new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            }
            else return null;
        }
    }
}
