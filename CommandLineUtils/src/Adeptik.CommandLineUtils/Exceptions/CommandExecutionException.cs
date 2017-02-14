using System;

namespace Adeptik.CommandLineUtils.Exceptions
{
    /// <summary>
    /// Ошибка при исполнении команды
    /// </summary>
    public class CommandExecutionException : Exception
    {
        public CommandExecutionException(string message)
            : base(message)
        { }

        public CommandExecutionException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
