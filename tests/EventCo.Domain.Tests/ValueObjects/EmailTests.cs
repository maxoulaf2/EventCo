using EventCo.Domain.ValueObjects;
using EventCo.Domain.ValueObjects.Exceptions;

namespace EventCo.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyValue_ThrowsEmailEmptyException(string value)
    {
        Assert.Throws<EmailEmptyException>(() => Email.From(value));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing-domain@")]
    [InlineData("missing-extension@gmail")]
    [InlineData("missing-domain@.com")]
    public void Create_InvalidFormat_ThrowsEmailInvalidFormatException(string value)
    {
        Assert.Throws<EmailInvalidFormatException>(() => Email.From(value));
    }

    [Fact]
    public void Create_ValidValue_NormalizesToLowerCaseTrimmed()
    {
        var email = Email.From("  Test@Example.COM  ");

        Assert.Equal("test@example.com", email.Value);
    }

    [Fact]
    public void Equals_SameValueDifferentCase_ReturnsTrue()
    {
        var first = Email.From("test@example.com");
        var second = Email.From("TEST@EXAMPLE.COM");

        Assert.Equal(first, second);
    }
}
