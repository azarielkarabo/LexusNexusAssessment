using LexusNexusAssessment.Entities;
using LexusNexusAssessment.Models;

namespace LexusNexusAssessment.Tests
{
    [TestFixture]
    public class CartStateTests
    {
        [Test]
        public void Constructor_ShouldCalculateCorrectTotals()
        {
            var items = new List<CartItem>
            {
                new CartItem("product1", 2, 2.52m),
                new CartItem("product2", 1, 9.98m)
            };

            var cartState = new CartState(items);

            Assert.That(cartState.Subtotal, Is.EqualTo(15.02m));
            Assert.That(cartState.Tax, Is.EqualTo(1.88m));
            Assert.That(cartState.Total, Is.EqualTo(16.90m));
        }

        [Test]
        public void Constructor_ShouldHandleEmptyCart()
        {
            var items = new List<CartItem>();
            var cartState = new CartState(items);

            Assert.That(cartState.Items, Is.Empty);
            Assert.That(cartState.Subtotal, Is.EqualTo(0m));
            Assert.That(cartState.Tax, Is.EqualTo(0m));
            Assert.That(cartState.Total, Is.EqualTo(0m));
        }

        [Test]
        public void Constructor_ShouldHandleNullItems()
        {
            var cartState = new CartState(null);

            Assert.That(cartState.Items, Is.Empty);
            Assert.That(cartState.Subtotal, Is.EqualTo(0m));
            Assert.That(cartState.Tax, Is.EqualTo(0m));
            Assert.That(cartState.Total, Is.EqualTo(0m));
        }
    }
}