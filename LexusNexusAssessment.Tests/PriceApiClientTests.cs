using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LexusNexusAssessment.Services;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace LexusNexusAssessment.Tests
{
    [TestFixture]
    public class PriceApiClientTests
    {
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;
        private IPriceApiClient _priceApiClient;

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://LexusNexusAssessments.github.io/")
            };
            _priceApiClient = new PriceApiClient(_httpClient);
        }

        [TearDown]
        public void Cleanup()
        {
            _httpClient?.Dispose();
        }

        [Test]
        public async Task GetProductPriceAsync_ValidProduct_ReturnsPriceFromApi()
        {
            string productName = "cornflakes";
            decimal expectedPrice = 2.52m;

            SetupMockResponse(productName, HttpStatusCode.OK, $"{{\"price\": {expectedPrice}}}");

            decimal price = await _priceApiClient.GetProductPriceAsync(productName);

            Assert.That(price, Is.EqualTo(expectedPrice));
        }

        [TestCase("")]
        [TestCase("   ")]
        public void GetProductPriceAsync_InvalidProductName_ThrowsArgumentException(string productName)
        {
            Assert.That(async () => await _priceApiClient.GetProductPriceAsync(productName),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GetProductPriceAsync_ApiReturnsError_ThrowsHttpRequestException()
        {
            string productName = "invalidproduct";
            SetupMockResponse(productName, HttpStatusCode.NotFound, "");

            Assert.That(async () => await _priceApiClient.GetProductPriceAsync(productName),
                Throws.TypeOf<HttpRequestException>());
        }

        [Test]
        public void GetProductPriceAsync_ApiReturnsInvalidJson_ThrowsJsonException()
        {
            string productName = "cornflakes";
            SetupMockResponse(productName, HttpStatusCode.OK, "invalid json");

            Assert.That(async () => await _priceApiClient.GetProductPriceAsync(productName),
                Throws.TypeOf<JsonException>());
        }

        private void SetupMockResponse(string productName, HttpStatusCode statusCode, string content)
        {
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString().Contains(productName.ToLowerInvariant())),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content)
                });
        }
    }
}