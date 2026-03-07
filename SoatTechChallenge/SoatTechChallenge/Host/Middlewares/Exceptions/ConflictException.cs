namespace SoatTechChallenge.Middlewares.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}