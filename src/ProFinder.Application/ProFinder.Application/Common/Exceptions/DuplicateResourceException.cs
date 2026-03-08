namespace ProFinder.Application.Common.Exceptions;

public class DuplicateResourceException : AppException
{
    public DuplicateResourceException(string message)
        : base(message)
    {
    }
}
