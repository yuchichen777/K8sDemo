using K8sDemo.SapConsumer.Interfaces;
using K8sDemo.Shared.Models;

namespace K8sDemo.SapConsumer.Services
{
    public class SapApiClient : ISapApiClient
    {
        private readonly HttpClient _httpClient;

        public SapApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> PostMaterialPickedAsync(MaterialPickedEvent evt)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "http://sap-api:8080/api/sap/material-picked",
                evt
            );

            var result = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[SapConsumer] SapApi 回應: {result}");

            return response.IsSuccessStatusCode;
        }
    }
}
