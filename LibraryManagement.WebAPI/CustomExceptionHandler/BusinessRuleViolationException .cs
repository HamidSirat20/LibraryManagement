namespace LibraryManagement.WebAPI.CustomExceptionHandler;
public class BusinessRuleViolationException : Exception
{
    public string ErrorCode { get; }
    public BusinessRuleViolationException()
    {
    }
    public BusinessRuleViolationException(string message)
        : base(message)
    {
    }
    public BusinessRuleViolationException(string message, string errorCode = null) 
        : base(message)
    {
        ErrorCode = errorCode;
    }

}
