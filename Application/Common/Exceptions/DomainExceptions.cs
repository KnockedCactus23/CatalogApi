namespace Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

public class UnprocessableEntityException : Exception
{
    public UnprocessableEntityException(string message) : base(message) { }
}