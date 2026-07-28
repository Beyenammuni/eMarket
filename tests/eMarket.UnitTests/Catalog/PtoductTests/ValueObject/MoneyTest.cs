using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Common;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.UnitTests.Catalog.PtoductTests.ValueObject
{
    public class MoneyTest
    {
        [Fact]
        public void Create_Should_Create_Money()
        {
            var money = Money.Create(100, Currency.USD);

            money.Amount.Should().Be(100);
            money.Currency.Should().Be(Currency.USD);
        }

        [Fact]
        public void Two_Money_With_Same_Value_Should_Be_Equal()
        {
            var first = Money.Create(100, Currency.USD);
            var second = Money.Create(100, Currency.USD);

            first.Should().Be(second);
        }
        [Fact]
        public void Two_Money_With_Different_Currency_Should_Not_Be_Equal()
        {
            // Arrange
            var usd = Money.Create(100, Currency.USD);
            var eur = Money.Create(100, Currency.EUR);

            // Assert
            usd.Should().NotBe(eur);
        }
        [Fact]
        public void Two_Money_With_Different_Amount_Should_Not_Be_Equal()
        {
            // Arrange
            var first = Money.Create(100, Currency.USD);
            var second = Money.Create(200, Currency.USD);

            // Assert
            first.Should().NotBe(second);
        }
    }
}
