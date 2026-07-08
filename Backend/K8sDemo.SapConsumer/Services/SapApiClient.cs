using K8sDemo.SapConsumer.Interfaces;
using K8sDemo.SapConsumer.Options;
using K8sDemo.Shared.Models;
using Microsoft.Extensions.Options;

namespace K8sDemo.SapConsumer.Services
{
    public class SapApiClient : ISapApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SapApiClient> _logger;
        private readonly SapApiOptions _options;

        public SapApiClient(
            HttpClient httpClient,
            ILogger<SapApiClient> logger,
            IOptions<SapApiOptions> options)
        {
            _httpClient = httpClient;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<bool> PostMaterialPickedAsync(MaterialPickedEvent evt)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_options.BaseUrl}/api/sap/material-picked",
                evt
            );

            var result = await response.Content.ReadAsStringAsync();

            _logger.LogInformation(
                "SapApi response: {Result}",
                result
            );

            return response.IsSuccessStatusCode;
        }
    }
}
