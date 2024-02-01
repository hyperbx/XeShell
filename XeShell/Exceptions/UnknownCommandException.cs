namespace XeShell.Exceptions
{
    public class UnknownCommandException(string in_command) : Exception(string.Format(_message, in_command))
    {
        private const string _message = "'{0}' is not recognised as an internal or external command\nor operable program.";
    }
}
