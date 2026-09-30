using Cubido.Template.Application.Common.Exceptions;
using FluentValidation.Results;
using Shouldly;

namespace Cubido.Template.Application.UnitTests.Common.Exceptions;

public class ValidationExceptionTests
{
    [Test]
    public void DefaultConstructorCreatesAnEmptyErrorDictionary()
    {
        var actual = new ValidationException().Errors;

        actual.Keys.ShouldBe(Array.Empty<string>());
    }

    [Test]
    public void SingleValidationFailureCreatesASingleElementErrorDictionary()
    {
        var failures = new List<ValidationFailure>
            {
                new("Age", "must be over 18"),
            };

        var actual = new ValidationException(failures).Errors;

        actual.Keys.ShouldBe(["Age"], ignoreOrder: true);
        actual["Age"].ShouldBe(["must be over 18"], ignoreOrder: true);
    }

    [Test]
    public void MulitpleValidationFailureForMultiplePropertiesCreatesAMultipleElementErrorDictionaryEachWithMultipleValues()
    {
        var failures = new List<ValidationFailure>
            {
                new("Age", "must be 18 or older"),
                new("Age", "must be 25 or younger"),
                new("Password", "must contain at least 8 characters"),
                new("Password", "must contain a digit"),
                new("Password", "must contain upper case letter"),
                new("Password", "must contain lower case letter"),
            };

        var actual = new ValidationException(failures).Errors;

        actual.Keys.ShouldBe(["Password", "Age"], ignoreOrder: true);

        actual["Age"].ShouldBe(
        [
                "must be 25 or younger",
                "must be 18 or older",
        ], ignoreOrder: true);

        actual["Password"].ShouldBe(
        [
                "must contain lower case letter",
                "must contain upper case letter",
                "must contain at least 8 characters",
                "must contain a digit",
        ], ignoreOrder: true);
    }
}
