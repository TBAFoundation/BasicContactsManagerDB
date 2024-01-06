namespace ContactManager;

public class ContactException : Exception
{
    public ContactException(string? message) : base(message)
    {
    }

    public ContactException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
