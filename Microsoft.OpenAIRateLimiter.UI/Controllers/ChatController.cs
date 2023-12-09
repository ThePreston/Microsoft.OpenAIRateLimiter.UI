
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenAIRateLimiter.UI.Models;
using Newtonsoft.Json;
using System.Text;

namespace Microsoft.OpenAIRateLimiter.UI.Controllers
{
    
    public class ChatController : Controller
    {
        private readonly ILogger<ChatController> _logger;

        private readonly HttpClient _chatClient;

        private readonly HttpClient _prodClient;

        public ChatController(ILogger<ChatController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;

            _chatClient = httpClientFactory.CreateClient("ChatAPI");

            _prodClient = httpClientFactory.CreateClient("QuotaService");

        }

        // GET: ChatController
        public async Task<ActionResult> Index()
        {

            using var resp = await _prodClient.GetAsync("/CustomQuota/Quota");

            var body = await resp.Content.ReadAsStringAsync();

            return View(JsonConvert.DeserializeObject<List<ProdQuota>>(body) ?? new List<ProdQuota>());

        }

        public async Task<string> Chat(string sub, string prompt, bool stream = false)
        {
            try
            {
                var messageList = new[] { new { role = "user", content = prompt } };
                    
                var payload = new { model = "gpt-35-turbo", stream, messages = messageList };

                HttpContent c = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                _chatClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", sub);

                using var resp = await _chatClient.PostAsync("/OpenAI/deployments/AIGPT/chat/completions?api-version=2023-05-15", c);

                var body = await resp.Content.ReadAsStringAsync();

                if (resp.IsSuccessStatusCode)
                    return body;
                else
                    throw new Exception(body);

            }
            catch (Exception ex)
            {

                _logger.LogError(ex.Message, ex);

                throw new Exception(ex.Message);
            }

        }

    }
}