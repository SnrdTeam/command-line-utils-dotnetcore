using Adeptik.CommandLineUtils.Attributes;
using Adeptik.CommandLineUtils.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Adeptik.CommandLineUtils.Engine
{
    /// <summary>
    /// Команда
    /// </summary>
    public partial class Command
    {
        private readonly MethodInfo _commandMethod;

        /// <summary>
        /// Создание экземпляра класса <see cref="Command"/>
        /// </summary>
        /// <param name="parent">Родительская команда</param>
        /// <param name="commandMethod">Метод команды</param>
        /// <param name="allowStatic">Разрешить, чтобы метод команды был статическим методом</param>
        private Command(Command parent, MethodInfo commandMethod, bool allowStatic = false)
        {
            Parent = parent;

            if (commandMethod.Name.StartsWith("-"))
                throw new CommandStructureException("Command name cannot start with '-'");
            if (!commandMethod.IsDefined(typeof(CommandAttribute)))
                throw new CommandStructureException("CommandAttribute is not defined");
            if (commandMethod.IsSpecialName
                || (commandMethod.IsStatic && !allowStatic)
                || commandMethod.IsGenericMethod
                || commandMethod.IsAbstract
                || commandMethod.IsConstructor)
                throw new CommandStructureException("Command method cannot be neither static nor generic nor abstract nor constructor");

            _commandMethod = commandMethod;

            if (ExitCommandName.Equals(commandMethod.Name, StringComparison.OrdinalIgnoreCase))
                throw new CommandStructureException($"Command name \"{ExitCommandName}\" is reserved");

            Name = commandMethod.Name;
            var commandAttribute = commandMethod.GetCustomAttribute<CommandAttribute>();
            Description = commandAttribute.Description;

            var options = LoadOptions(commandMethod)
                .ToList()
                .AsReadOnly();
            var optionNames = options
                .SelectMany(x => new[] { x.ShortName, x.LongName })
                .Where(x => x != null);
            if (optionNames.Count() != optionNames.Distinct().Count())
                throw new CommandStructureException("Option name must be unique");
            string[] helpOptionNames = { Option.HelpOption.ShortName, Option.HelpOption.LongName };
            if (options.Any(option => helpOptionNames.Contains(option.ShortName) || helpOptionNames.Contains(option.LongName)))
                throw new CommandStructureException($"Option name cannot be the same as the help option name '-{Option.HelpOption.ShortName}' or '--{Option.HelpOption.LongName}'.");
            Options = options;

            var commandsClassType = commandMethod.ReturnType;
            if (commandsClassType != typeof(void))
            {
                var commandClassTypeInfo = commandsClassType.GetTypeInfo();
                if (!commandClassTypeInfo.IsClass
                    || !commandClassTypeInfo.IsSealed)
                    throw new CommandStructureException("Command class must be a sealed class");
                var commands = LoadCommands(commandsClassType)
                    .ToList()
                    .AsReadOnly();
                var commandNames = commands
                    .Select(x => x.Name);
                if (commandNames.Count() != commandNames.Distinct().Count())
                    throw new CommandStructureException("Command name must be unique");
                Commands = commands;
            }
            else
            {
                Commands = Enumerable.Empty<Command>();
            }
        }

        /// <summary>
        /// Родительская команда
        /// </summary>
        public Command Parent { get; }

        /// <summary>
        /// Имя команды
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Описание команды
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Дочерние команды
        /// </summary>
        public IEnumerable<Command> Commands { get; }

        /// <summary>
        /// Опции команды
        /// </summary>
        public IEnumerable<Option> Options { get; }

        /// <summary>
        /// Отобразить справку для данной команды
        /// </summary>
        public void ShowHelp()
        {
            ShowHelp(true);
        }

        /// <summary>
        /// Отобразить справку для данной команды
        /// </summary>
        /// <param name="showSelfOptions">Признак отображения справки по собственным опциям</param>
        private void ShowHelp(bool showSelfOptions)
        {
            if (!string.IsNullOrEmpty(Description))
            {
                Console.WriteLine(Description);
                Console.WriteLine();
            }

            var commandPath = string.Join(" ", GetCommandPath(this).Select(x => x.Name));
            Console.WriteLine($"Usage: {commandPath}{(Options.Any() ? " [options]" : "")}{(Commands.Any() ? " [command]" : "")}");

            Console.WriteLine();
            Console.WriteLine("Options:");
            var optionHelpInfos = (showSelfOptions ? Options : Enumerable.Empty<Option>())
                .Concat(new[] { Option.HelpOption })
                .Select(x => new
                {
                    Template = $"{(x.ShortName != null ? $"-{x.ShortName} | " : "")}--{(x.LongName ?? "")}",
                    x.Description
                });
            var maxOptionTemplateWidth = optionHelpInfos.Max(x => x.Template.Length);
            foreach (var option in optionHelpInfos)
            {
                Console.WriteLine($"  {option.Template.PadRight(maxOptionTemplateWidth)}  {option.Description}");
            }

            if (Commands.Any())
            {
                Console.WriteLine();
                Console.WriteLine("Commands:");
                var maxCommandNameWidth = Commands.Max(x => x.Name.Length);
                foreach (var command in Commands)
                {
                    Console.WriteLine($"  {command.Name.PadRight(maxCommandNameWidth)}  {command.Description}");
                }
            }

            if (Commands.Any())
            {
                Console.WriteLine();
                Console.WriteLine($"Use \"{commandPath} [command] --help\" for more information about a command.");
            }
        }

        private IEnumerable<Command> LoadCommands(Type commandsClassType)
        {
            var commandMethods = commandsClassType
                .GetRuntimeMethods()
                .Where(x => x.IsDefined(typeof(CommandAttribute)));

            foreach (var commandMethod in commandMethods)
            {
                yield return new Command(this, commandMethod);
            }
        }

        private static IEnumerable<Option> LoadOptions(MethodInfo commandMethod)
        {
            foreach (var parameter in commandMethod.GetParameters())
            {
                yield return new Option(parameter);
            }
        }

        /// <summary>
        /// Формирования пути до указанной команды
        /// </summary>
        /// <param name="command">Команда</param>
        /// <returns>Коллекция команд, образующая путь до команды <paramref name="command"/></returns>
        private static IEnumerable<Command> GetCommandPath(Command command)
        {
            var commandPath = new List<Command>();
            for (; command.Parent != null; command = command.Parent)
                commandPath.Add(command);
            return Enumerable.Reverse(commandPath);
        }
    }
}
