namespace Diploma1.Common.Exceptions;

public class DiplomaApiException : Exception
{
    public int StatusCode { get; } = 500;
    public string Text { get; }

    public DiplomaApiException()
    {
        Text = "An unexpected error occurred.";
    }
    public DiplomaApiException(string message) : base(message)
    {
        Text = message;
    }

    public DiplomaApiException(string message, Exception innerException) : base(message, innerException)
    {
        Text = message;
    }
}