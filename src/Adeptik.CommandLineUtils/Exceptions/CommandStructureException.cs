using System;

namespace Adeptik.CommandLineUtils.Exceptions
{
    /// <summary>
    /// Ошибка формированияя структуры команд
    /// </summary>
    public class CommandStructureException : Exception
    {
        /// <summary>
        /// Создание экземпляра класса <see cref="CommandStructureException"/>
        /// </summary>
        /// <param name="message">Сообшение об ошибке</param>
        public CommandStructureException(string message)
            : base(message)
        { }

        /// <summary>
        /// Создание экземпляра класса <see cref="CommandStructureException"/>
        /// </summary>
        /// <param name="message">Сообшение об ошибке</param>
        /// <param name="innerException">>Исключение, являющееся причиной данной ошибки</param>
        public CommandStructureException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
