using Microsoft.Extensions.Logging;
using Moq;
using Moq.Contrib.HttpClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kepware.Api.Test.ApiClient
{
    public class TestApiClientBase
    {
        protected const string TEST_ENDPOINT = "https://example.com";
        protected readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        protected readonly HttpClient _httpClient;
        protected readonly Mock<ILogger<KepwareApiClient>> _loggerMock;
        protected readonly KepwareApiClient _kepwareApiClient;

        public TestApiClientBase()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = _httpMessageHandlerMock.CreateClient();
            _httpClient.BaseAddress = new Uri(TEST_ENDPOINT);

            _loggerMock = new Mock<ILogger<KepwareApiClient>>();
            _kepwareApiClient = new KepwareApiClient(new KepwareApiClientOptions { HostUri = _httpClient.BaseAddress }, _loggerMock.Object, _httpClient);
        }

        #region ConfigureConnectedClient
        protected void ConfigureConnectedClient(string productName = "KEPServerEX", string productId = "012",
            int majorVersion = 6, int minorVersion = 17, int buildVersion = 240, int patchVersion = 0)
        {
            // Arrange: Mock for the status endpoint
            var statusResponse = "[{\"Name\": \"ConfigAPI REST Service\", \"Healthy\": true}]";
            _httpMessageHandlerMock.SetupRequest(HttpMethod.Get, TEST_ENDPOINT + "/config/v1/status")
                                   .ReturnsResponse(statusResponse, "application/json");

            // Arrange: Mock for the product info endpoint
            var productResponse = $$"""
        {
            "product_id": "{{productId}}",
            "product_name": "{{productName}}",
            "product_version": "V{{majorVersion}}.{{minorVersion}}.{{buildVersion}}.{{patchVersion}}",
            "product_version_major": {{majorVersion}},
            "product_version_minor": {{minorVersion}},
            "product_version_build": {{buildVersion}},
            "product_version_patch": {{patchVersion}}
        }
        """;
            _httpMessageHandlerMock.SetupRequest(HttpMethod.Get, TEST_ENDPOINT + "/config/v1/about")
                                   .ReturnsResponse(productResponse, "application/json");
        }
        #endregion

    }
}
