using Adeptik.CommandLineUtils.Attributes;
using System;
using System.Reflection;

namespace Adeptik.CommandLineUtils.Engine
{
    /// <summary>
    /// Опция команды
    /// </summary>
    public class Option
    {
        /// <summary>
        /// Опция справки
        /// </summary>
        public static Option HelpOption { get; } = new Option("help", "h", "Show help");

        private Option(string longName, string shortName, string description)
        {
            Initialize(longName, shortName, description);
        }

        internal Option(ParameterInfo commandMethodParameter)
        {
            ParameterInfo = commandMethodParameter;
            var optionAttribute = commandMethodParameter.GetCustomAttribute<CommandOptionAttribute>();

            Initialize(
                commandMethodParameter.Name,
                optionAttribute?.ShortName,
                optionAttribute?.Description);
        }

        private void Initialize(string longName, string shortName, string description)
        {
            if (longName == null && shortName == null)
                throw new ArgumentException("longName and shortName cannot be both null");
            if (longName?.StartsWith("-") ?? false)
                throw new ArgumentException("cannot start with '-'", nameof(longName));
            if (shortName?.StartsWith("-") ?? false)
                throw new ArgumentException("cannot start with '-'", nameof(shortName));

            LongName = longName;
            ShortName = shortName;
            Description = description;
        }

        /// <summary>
        /// Полное имя опции команды (в коммандной строке предваряется "--")
        /// </summary>
        public string LongName { get; private set; }

        /// <summary>
        /// Краткое имя опции команды (в коммандной строке предваряется "-")
        /// </summary>
        public string ShortName { get; private set; }

        /// <summary>
        /// Описание опции команды
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Признак того, что данная опция обязательна для ввода
        /// </summary>
        public bool IsRequired
        {
            get
            {
                return !ParameterInfo.IsOptional;
            }
        }

        internal ParameterInfo ParameterInfo { get; }
    }
}
