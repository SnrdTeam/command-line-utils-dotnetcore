using System;

namespace Adeptik.CommandLineUtils.Exceptions
{
    /// <summary>
    /// Ошибка при исполнении команды
    /// </summary>
    public class CommandExecutionException : Exception
    {
        /// <summary>
        /// Создание экземпляра класса <see cref="CommandExecutionException"/>
        /// </summary>
        /// <param name="message">Сообшение об ошибке</param>
        public CommandExecutionException(string message)
            : base(message)
        { }

        /// <summary>
        /// Создание экземпляра класса <see cref="CommandExecutionException"/>
        /// </summary>
        /// <param name="message">Сообшение об ошибке</param>
        /// <param name="innerException">Исключение, являющееся причиной данной ошибки</param>
        public CommandExecutionException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
