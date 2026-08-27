namespace human_recognition.Domain.Exceptions;

public abstract class human_recognitionException : Exception
{
    protected human_recognitionException(string message) : base(message) { }

    protected human_recognitionException(string message, Exception innerException) : base(message, innerException) { }
}

// Data Conflict exception
public class ConflictException : human_recognitionException
{
    public ConflictException(string message) : base(message) { }
    protected ConflictException(string message, Exception innerException) : base(message, innerException) { }
}

// Data Not Found exeption
public class NotFoundException : human_recognitionException
{
    public NotFoundException(string message) : base(message) { }
    protected NotFoundException(string message, Exception innerException) : base(message, innerException) { }
}

// Unauthorized User exception
public class UnauthorizedException : human_recognitionException
{
    public UnauthorizedException(string message) : base(message) { }
    protected UnauthorizedException(string message, Exception innerException) : base(message, innerException) { }
}