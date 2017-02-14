using System;

namespace Adeptik.CommandLineUtils.Exceptions
{
    /// <summary>
    /// Ошибка формированияя структуры команд
    /// </summary>
    public class CommandStructureException : Exception
    {
        public CommandStructureException(string message)
            : base(message)
        { }

        public CommandStructureException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
