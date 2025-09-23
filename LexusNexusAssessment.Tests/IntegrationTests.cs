using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LexusNexusAssessment.Repositories;
using LexusNexusAssessment.Services;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace LexusNexusAssessment.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;
        private IPriceApiClient _priceApiClient;
        private ICartItemRepository _cartRepository;

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://LexusNexusAssessments.github.io/")
            };
            _priceApiClient = new PriceApiClient(_httpClient);
            _cartRepository = new CartItemRepository(_priceApiClient);

            SetupMockResponse("cornflakes", HttpStatusCode.OK, "{\"price\": 2.52}");
            SetupMockResponse("weetabix", HttpStatusCode.OK, "{\"price\": 9.98}");
        }

        [TearDown]
        public void Cleanup()
        {
            _httpClient?.Dispose();
        }

        [Test]
        public async Task ExampleScenario_MatchesExpectedResults()
        {
            await _cartRepository.AddProductAsync("cornflakes", 1);
            await _cartRepository.AddProductAsync("cornflakes", 1);
            await _cartRepository.AddProductAsync("weetabix", 1);

            var cartState = _cartRepository.CalculateCartState();

            Assert.That(_cartRepository.GetProductQuantity("cornflakes"), Is.EqualTo(2));
            Assert.That(_cartRepository.GetProductQuantity("weetabix"), Is.EqualTo(1));
            Assert.That(cartState.Subtotal, Is.EqualTo(15.02m));
            Assert.That(cartState.Tax, Is.EqualTo(1.88m));
            Assert.That(cartState.Total, Is.EqualTo(16.90m));
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