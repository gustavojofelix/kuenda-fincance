namespace KuendaFinance.Shared.Results;

public sealed class ValidationResult : Result
{
    private ValidationResult(Error[] errors)
        : base(false, new Error("Validation.Error", "A validation error occurred."))
    {
        ValidationErrors = errors;
    }

    public Error[] ValidationErrors { get; }

    public static ValidationResult WithErrors(Error[] errors) => new(errors);
}

public sealed class ValidationResult<TValue> : Result<TValue>
{
    private ValidationResult(Error[] errors)
        : base(default, false, new Error("Validation.Error", "A validation error occurred."))
    {
        ValidationErrors = errors;
    }

    public Error[] ValidationErrors { get; }

    public static ValidationResult<TValue> WithErrors(Error[] errors) => new(errors);
}
