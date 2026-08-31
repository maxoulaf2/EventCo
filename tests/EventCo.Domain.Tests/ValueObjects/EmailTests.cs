using EventCo.Domain.Common;
using EventCo.Domain.ValueObjects;

namespace EventCo.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("missing-domain@")]
    [InlineData("missing-extension@gmail")]
    [InlineData("missing-domain@.com")]
    public void Create_InvalidValue_ThrowsDomainException(string value)
    {
        Assert.Throws<DomainException>(() => Email.Create(value));
    }

    [Fact]
    public void Create_ValidValue_NormalizesToLowerCaseTrimmed()
    {
        var email = Email.Create("  Test@Example.COM  ");

        Assert.Equal("test@example.com", email.Value);
    }

    [Fact]
    public void Equals_SameValueDifferentCase_ReturnsTrue()
    {
        var first = Email.Create("test@example.com");
        var second = Email.Create("TEST@EXAMPLE.COM");

        Assert.Equal(first, second);
    }
}
