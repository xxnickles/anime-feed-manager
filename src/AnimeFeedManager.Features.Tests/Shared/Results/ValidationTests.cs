using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Features.Tests.Shared.Results;

public class ValidationTests
{
    [Fact]
    public void Should_Create_Valid_Validation()
    {
        var validation = Validation<int>.Valid(42);
        var result = validation.AsResult();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Should_Create_Invalid_Validation()
    {
        var error = DomainValidationErrors.Create([
            DomainValidationError.Create("field", "error message")
        ]);
        var validation = Validation<int>.Invalid(error);
        var result = validation.AsResult();

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Should_Map_Valid_Validation_To_New_Type()
    {
        var validation = Validation<int>.Valid(42)
            .Map(x => x.ToString());

        var result = validation.AsResult();
        Assert.True(result.IsSuccess);
        result.AssertOnSuccess(value => Assert.Equal("42", value));
    }

    [Fact]
    public void Should_Not_Map_Invalid_Validation()
    {
        var error = DomainValidationErrors.Create([
            DomainValidationError.Create("field", "error message")
        ]);
        var validation = Validation<int>.Invalid(error)
            .Map(x => x.ToString());

        var result = validation.AsResult();
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Should_Convert_Validation_To_Result_Explicitly()
    {
        var validation = Validation<int>.Valid(42);
        var result = validation.AsResult();

        Assert.True(result.IsSuccess);
        result.AssertOnSuccess(value => Assert.Equal(42, value));
    }

    [Fact]
    public void Should_Convert_Validation_To_Result_Implicitly()
    {
        Result<int> result = Validation<int>.Valid(42);

        Assert.True(result.IsSuccess);
        result.AssertOnSuccess(value => Assert.Equal(42, value));
    }

    [Fact]
    public void Should_Combine_Two_Valid_Validations()
    {
        var validation1 = Validation<int>.Valid(10);
        var validation2 = Validation<string>.Valid("test");

        var combined = validation1.And(validation2);
        var result = combined.AsResult();

        Assert.True(result.IsSuccess);
        result.AssertOnSuccess(tuple =>
        {
            Assert.Equal(10, tuple.Item1);
            Assert.Equal("test", tuple.Item2);
        });
    }

    [Fact]
    public void Should_Aggregate_Errors_When_Both_Validations_Invalid()
    {
        var error1 = DomainValidationErrors.Create([
            DomainValidationError.Create("field1", "error 1")
        ]);
        var error2 = DomainValidationErrors.Create([
            DomainValidationError.Create("field2", "error 2")
        ]);

        var validation1 = Validation<int>.Invalid(error1);
        var validation2 = Validation<string>.Invalid(error2);

        var combined = validation1.And(validation2);
        var result = combined.AsResult();

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Should_Fail_When_First_Validation_Invalid()
    {
        var error = DomainValidationErrors.Create([
            DomainValidationError.Create("field", "error")
        ]);

        var validation1 = Validation<int>.Invalid(error);
        var validation2 = Validation<string>.Valid("test");

        var combined = validation1.And(validation2);
        var result = combined.AsResult();

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Should_Fail_When_Second_Validation_Invalid()
    {
        var error = DomainValidationErrors.Create([
            DomainValidationError.Create("field", "error")
        ]);

        var validation1 = Validation<int>.Valid(42);
        var validation2 = Validation<string>.Invalid(error);

        var combined = validation1.And(validation2);
        var result = combined.AsResult();

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Should_Combine_Three_Valid_Validations()
    {
        var validation1 = Validation<int>.Valid(10);
        var validation2 = Validation<string>.Valid("test");
        var validation3 = Validation<bool>.Valid(true);

        var combined = validation1
            .And(validation2)
            .And(validation3);

        var result = combined.AsResult();
        Assert.True(result.IsSuccess);
        result.AssertOnSuccess(tuple =>
        {
            Assert.Equal(10, tuple.Item1);
            Assert.Equal("test", tuple.Item2);
            Assert.True(tuple.Item3);
        });
    }

    [Fact]
    public void Should_Combine_Four_Valid_Validations()
    {
        var validation1 = Validation<int>.Valid(1);
        var validation2 = Validation<int>.Valid(2);
        var validation3 = Validation<int>.Valid(3);
        var validation4 = Validation<int>.Valid(4);

        var combined = validation1
            .And(validation2)
            .And(validation3)
            .And(validation4);

        var result = combined.AsResult();
        Assert.True(result.IsSuccess);
        result.AssertOnSuccess(tuple =>
        {
            Assert.Equal(1, tuple.Item1);
            Assert.Equal(2, tuple.Item2);
            Assert.Equal(3, tuple.Item3);
            Assert.Equal(4, tuple.Item4);
        });
    }

    [Fact]
    public void Should_Combine_Five_Valid_Validations()
    {
        var validations = new[]
        {
            Validation<int>.Valid(1),
            Validation<int>.Valid(2),
            Validation<int>.Valid(3),
            Validation<int>.Valid(4),
            Validation<int>.Valid(5)
        };

        var combined = validations[0]
            .And(validations[1])
            .And(validations[2])
            .And(validations[3])
            .And(validations[4]);

        var result = combined.AsResult();
        Assert.True(result.IsSuccess);
        result.AssertOnSuccess(tuple =>
        {
            Assert.Equal(1, tuple.Item1);
            Assert.Equal(2, tuple.Item2);
            Assert.Equal(3, tuple.Item3);
            Assert.Equal(4, tuple.Item4);
            Assert.Equal(5, tuple.Item5);
        });
    }

    [Fact]
    public void Should_Combine_Six_Valid_Validations()
    {
        var validations = new[]
        {
            Validation<int>.Valid(1),
            Validation<int>.Valid(2),
            Validation<int>.Valid(3),
            Validation<int>.Valid(4),
            Validation<int>.Valid(5),
            Validation<int>.Valid(6)
        };

        var combined = validations[0]
            .And(validations[1])
            .And(validations[2])
            .And(validations[3])
            .And(validations[4])
            .And(validations[5]);

        var result = combined.AsResult();
        Assert.True(result.IsSuccess);
        result.AssertOnSuccess(tuple =>
        {
            Assert.Equal(1, tuple.Item1);
            Assert.Equal(2, tuple.Item2);
            Assert.Equal(3, tuple.Item3);
            Assert.Equal(4, tuple.Item4);
            Assert.Equal(5, tuple.Item5);
            Assert.Equal(6, tuple.Item6);
        });
    }

    [Fact]
    public void Should_Combine_Seven_Valid_Validations()
    {
        var validations = new[]
        {
            Validation<int>.Valid(1),
            Validation<int>.Valid(2),
            Validation<int>.Valid(3),
            Validation<int>.Valid(4),
            Validation<int>.Valid(5),
            Validation<int>.Valid(6),
            Validation<int>.Valid(7)
        };

        var combined = validations[0]
            .And(validations[1])
            .And(validations[2])
            .And(validations[3])
            .And(validations[4])
            .And(validations[5])
            .And(validations[6]);

        var result = combined.AsResult();
        Assert.True(result.IsSuccess);
        result.AssertOnSuccess(tuple =>
        {
            Assert.Equal(1, tuple.Item1);
            Assert.Equal(2, tuple.Item2);
            Assert.Equal(3, tuple.Item3);
            Assert.Equal(4, tuple.Item4);
            Assert.Equal(5, tuple.Item5);
            Assert.Equal(6, tuple.Item6);
            Assert.Equal(7, tuple.Item7);
        });
    }

    [Fact]
    public void Should_Aggregate_Multiple_Validation_Errors()
    {
        var error1 = DomainValidationErrors.Create([
            DomainValidationError.Create("field1", "error 1")
        ]);
        var error2 = DomainValidationErrors.Create([
            DomainValidationError.Create("field2", "error 2")
        ]);
        var error3 = DomainValidationErrors.Create([
            DomainValidationError.Create("field3", "error 3")
        ]);

        var combined = Validation<int>.Invalid(error1)
            .And(Validation<int>.Invalid(error2))
            .And(Validation<int>.Invalid(error3));

        var result = combined.AsResult();
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Should_Use_Validation_For_Form_Data_Validation()
    {
        var ValidateName = (string name) =>
            string.IsNullOrWhiteSpace(name)
                ? Validation<string>.Invalid(DomainValidationErrors.Create([
                    DomainValidationError.Create("name", "Name is required")
                ]))
                : Validation<string>.Valid(name);

        var ValidateAge = (int age) =>
            age < 0 || age > 150
                ? Validation<int>.Invalid(DomainValidationErrors.Create([
                    DomainValidationError.Create("age", "Age must be between 0 and 150")
                ]))
                : Validation<int>.Valid(age);

        var validResult = ValidateName("John")
            .And(ValidateAge(30))
            .AsResult();

        Assert.True(validResult.IsSuccess);
        validResult.AssertOnSuccess(tuple =>
        {
            Assert.Equal("John", tuple.Item1);
            Assert.Equal(30, tuple.Item2);
        });
    }

    [Fact]
    public void Should_Collect_All_Validation_Errors_In_Form()
    {
        var ValidateName = (string name) =>
            string.IsNullOrWhiteSpace(name)
                ? Validation<string>.Invalid(DomainValidationErrors.Create([
                    DomainValidationError.Create("name", "Name is required")
                ]))
                : Validation<string>.Valid(name);

        var ValidateAge = (int age) =>
            age < 0 || age > 150
                ? Validation<int>.Invalid(DomainValidationErrors.Create([
                    DomainValidationError.Create("age", "Age must be between 0 and 150")
                ]))
                : Validation<int>.Valid(age);

        var invalidResult = ValidateName("")
            .And(ValidateAge(-5))
            .AsResult();

        Assert.False(invalidResult.IsSuccess);
    }
}
