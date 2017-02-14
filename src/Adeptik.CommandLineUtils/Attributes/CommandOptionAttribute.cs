using System;

namespace Adeptik.CommandLineUtils.Attributes
{
    /// <summary>
    /// Опция команды
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class CommandOptionAttribute : Attribute
    {
        /// <param name="shortName">Краткое имя опции команды</param>
        public CommandOptionAttribute(string shortName = null)
        {
            ShortName = shortName;
        }

        /// <summary>
        /// Краткое имя опции команды
        /// </summary>
        public string ShortName { get; }

        /// <summary>
        /// Описание опции команды
        /// </summary>
        public string Description { get; set; }
    }
}
