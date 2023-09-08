﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenAIRateLimiter.UI.Models;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Text;

namespace Microsoft.OpenAIRateLimiter.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly HttpClient _client;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            
            _client = httpClientFactory.CreateClient("QuotaService");
        }

        public IActionResult Index()
        {            
            return View();
        }

        private async Task<List<ProdQuota>> GetProdQuota()
        {

            using var resp = await _client.GetAsync("/CustomQuota/Quota");

            var body = await resp.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<ProdQuota>>(body) ?? new List<ProdQuota>() ;

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<JsonResult> Search()
        {

            _logger.LogInformation($"Entered  Search ");

            try
            {
                return new JsonResult(await GetProdQuota());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);

                throw;
            }

        }
        
        public async Task<bool> AddQuota(string sub, string prod, string amt)
        {
            try
            {

                var payload = new { subscriptionKey = sub, productName = prod, amount = amt };

                HttpContent c = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                using var resp = await _client.PostAsync("/CustomQuota/Quota", c);

                var body = await resp.Content.ReadAsStringAsync();

                return resp.IsSuccessStatusCode;

            }
            catch (Exception ex)
            {

                _logger.LogError(ex.Message, ex);

                throw;
            }

        }

        public async Task<ProdQuota> EditQuota(string subscription)
        {

            if (string.IsNullOrEmpty(subscription))
                return new ProdQuota();
            
            var quotas = await GetProdQuota();

            return quotas.FirstOrDefault(x => x.SubscriptionKey == subscription);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}