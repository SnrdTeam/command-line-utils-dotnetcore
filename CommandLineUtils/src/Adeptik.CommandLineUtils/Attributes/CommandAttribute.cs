using System;

namespace Adeptik.CommandLineUtils.Attributes
{
    /// <summary>
    /// Атрибут команды
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class CommandAttribute : Attribute
    {
        /// <summary>
        /// Описание команды
        /// </summary>
        public string Description { get; set; }
    }
}
