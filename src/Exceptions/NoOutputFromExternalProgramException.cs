namespace PatTool.Exceptions;

public class NoOutputFromExternalProgramException : Exception
{
    public NoOutputFromExternalProgramException()
    {
    }

    public NoOutputFromExternalProgramException(string? message) : base(message)
    {
    }

    public NoOutputFromExternalProgramException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}