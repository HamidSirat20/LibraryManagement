namespace LibraryManagement.WebAPI.CustomExceptionHandler;
public class BusinessRuleViolationException : Exception
{
    public string ErrorCode { get; }
    private const string DefaultMessage = "There was an error while handling the operation.";
    public BusinessRuleViolationException() : base(DefaultMessage) { }
    public BusinessRuleViolationException(string message)
        : base(message)
    {

    }
    public BusinessRuleViolationException(Exception innerException) : base(DefaultMessage, innerException)
    {

    }
    public BusinessRuleViolationException(string message, string errorCode = null)
        : base(message)
    {
        ErrorCode = errorCode;
    }

}
