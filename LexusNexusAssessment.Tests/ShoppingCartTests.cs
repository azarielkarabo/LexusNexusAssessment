using Moq;
using LexusNexusAssessment.Services;
using LexusNexusAssessment.Entities;
using LexusNexusAssessment.Repositories;

namespace LexusNexusAssessment.Tests
{
    [TestFixture]
    public class ShoppingCartTests
    {
        private Mock<IPriceApiClient> _mockPriceApiClient;
        private ICartItemRepository _shoppingCart;

        [SetUp]
        public void Setup()
        {
            _mockPriceApiClient = new Mock<IPriceApiClient>();
            _shoppingCart = new CartItemRepository(_mockPriceApiClient.Object);
        }

        [Test]
        public async Task AddProduct_ShouldAddNewProduct_WhenProductDoesNotExist()
        {
            _mockPriceApiClient.Setup(client => client.GetProductPriceAsync("cornflakes"))
                .ReturnsAsync(2.52m);

            await _shoppingCart.AddProductAsync("cornflakes", 1);

            var cartItems = _shoppingCart.GetCartItems();
            Assert.That(cartItems.Count, Is.EqualTo(1));
            Assert.That(cartItems[0].ProductName, Is.EqualTo("cornflakes"));
            Assert.That(cartItems[0].Quantity, Is.EqualTo(1));
            Assert.That(cartItems[0].Price, Is.EqualTo(2.52m));
        }

        [Test]
        public async Task AddProduct_ShouldUpdateQuantity_WhenProductAlreadyExists()
        {
            _mockPriceApiClient.Setup(client => client.GetProductPriceAsync("cornflakes"))
                .ReturnsAsync(2.52m);

            await _shoppingCart.AddProductAsync("cornflakes", 1);

            await _shoppingCart.AddProductAsync("cornflakes", 1);

            var cartItems = _shoppingCart.GetCartItems();
            Assert.That(cartItems.Count, Is.EqualTo(1));
            Assert.That(cartItems[0].ProductName, Is.EqualTo("cornflakes"));
            Assert.That(cartItems[0].Quantity, Is.EqualTo(2));
            Assert.That(cartItems[0].Price, Is.EqualTo(2.52m));
        }

        [Test]
        public async Task GetProductQuantity_ShouldReturnCorrectQuantity()
        {
            _mockPriceApiClient.Setup(client => client.GetProductPriceAsync("cornflakes"))
                .ReturnsAsync(2.52m);

            await _shoppingCart.AddProductAsync("cornflakes", 3);

            int quantity = _shoppingCart.GetProductQuantity("cornflakes");

            Assert.That(quantity, Is.EqualTo(3));
        }

        [Test]
        public void GetProductQuantity_ShouldReturnZero_WhenProductDoesNotExist()
        {
            int quantity = _shoppingCart.GetProductQuantity("nonexistent");

            Assert.That(quantity, Is.EqualTo(0));
        }

        [Test]
        public async Task CalculateCartState_ShouldReturnCorrectTotals_ForExampleScenario()
        {
            _mockPriceApiClient.Setup(client => client.GetProductPriceAsync("cornflakes"))
                .ReturnsAsync(2.52m);
            _mockPriceApiClient.Setup(client => client.GetProductPriceAsync("weetabix"))
                .ReturnsAsync(9.98m);

            await _shoppingCart.AddProductAsync("cornflakes", 1);
            await _shoppingCart.AddProductAsync("cornflakes", 1);
            await _shoppingCart.AddProductAsync("weetabix", 1);

            var cartState = _shoppingCart.CalculateCartState();

            Assert.That(cartState.Subtotal, Is.EqualTo(15.02m));
            Assert.That(cartState.Tax, Is.EqualTo(1.88m));
            Assert.That(cartState.Total, Is.EqualTo(16.90m));
        }

        [Test]
        public void CalculateCartState_ShouldRoundCorrectly()
        {
            var items = new List<CartItem>
            {
                new CartItem("testProduct1", 1, 10.125m)
            };

            var cartStateField = typeof(CartItem).GetField("_items",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var cartItems = new Dictionary<string, CartItem>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in items)
            {
                cartItems[item.ProductName] = item;
            }

            cartStateField.SetValue(_shoppingCart, cartItems);

            var cartState = _shoppingCart.CalculateCartState();

            Assert.That(cartState.Subtotal, Is.EqualTo(10.13m));
            Assert.That(cartState.Tax, Is.EqualTo(1.27m));
            Assert.That(cartState.Total, Is.EqualTo(11.40m));
        }

        [TestCase("", 1)]
        [TestCase("product", 0)]
        [TestCase("product", -1)]
        public void AddProduct_ShouldThrowArgumentException_WhenInvalidArguments(string productName, int quantity)
        {
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _shoppingCart.AddProductAsync(productName, quantity));
        }
    }
}