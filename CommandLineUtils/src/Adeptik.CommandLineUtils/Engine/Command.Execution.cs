using Adeptik.CommandLineUtils.Exceptions;
using Adeptik.CommandLineUtils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Adeptik.CommandLineUtils.Engine
{
    partial class Command
    {
        /// <summary>
        /// Разделители имени и значения опции
        /// </summary>
        private static readonly char[] NameValueDelimeters = new[] { ' ', '=', ':' };

        /// <summary>
        /// Имя команды выхода из консоли команды
        /// </summary>
        public const string ExitCommandName = "exit";

        /// <summary>
        /// Исполнить команду
        /// </summary>
        /// <param name="args">Аргументы командной строки</param>
        /// <param name="target">Объект, для которого необходимо выполнить метод команды</param>
        /// <param name="scanHelpOption">Искать опцию для показа справки</param>
        /// <param name="allowEnteringCommandConsole">Разрешить входить в консоль команды, если указана неконечная команда</param>
        private void Execute(Queue<string> args, object target, bool scanHelpOption, bool allowEnteringCommandConsole)
        {
            if (scanHelpOption)
            {
                Command commandHelp;
                if (TryFindCommandForHelp(args, out commandHelp))
                {
                    if (commandHelp == null)
                        throw new CommandExecutionException("Unrecognized command");
                    commandHelp.ShowHelp();
                    return;
                }
            }

            var parameters = ResolveOptions(args);
            var result = _commandMethod.Invoke(target, parameters);
            try
            {
                ExecuteChildCommand(args, result, allowEnteringCommandConsole);
            }
            finally
            {
                if (result is IDisposable)
                    ((IDisposable)result).Dispose();
            }
        }

        /// <summary>
        /// Исполнение дочерней команды
        /// </summary>
        /// <param name="args">Аргументы командной строки</param>
        /// <param name="childTarget">Объект, для которого необходимо выполнить метод дочерней команды</param>
        /// <param name="allowEnteringCommandConsole">Разрешить входить в консоль команды, если указана неконечная команда</param>
        private void ExecuteChildCommand(Queue<string> args, object childTarget, bool allowEnteringCommandConsole)
        {
            if (Commands.Any())
            {
                if (childTarget == null)
                    throw new InvalidOperationException($"Target for child command is null");
                if (!args.Any())
                {
                    if (allowEnteringCommandConsole)
                        EnterCommandConsole(this, childTarget);
                    else
                        throw new CommandExecutionException("Command expected");
                    return;
                }

                var nextCommandNameArg = args.Dequeue();
                var nextCommand = Commands
                    .FirstOrDefault(x => x.Name == nextCommandNameArg);
                if (nextCommand == null)
                    throw new CommandExecutionException($"Unknown command \"{nextCommandNameArg}\"");
                nextCommand.Execute(args, childTarget, false, allowEnteringCommandConsole);
            }
            else if (args.Any())
            {
                throw new CommandExecutionException($"Unexpected argument: {args.Peek()}");
            }
        }

        /// <summary>
        /// Разрешение значений параметров метода команды
        /// </summary>
        /// <param name="args">Аргументы коммандной строки</param>
        /// <returns>Значения параметров метода команды</returns>
        private object[] ResolveOptions(Queue<string> args)
        {
            var optionsToResolve = Options.ToList();
            var optionsMap = optionsToResolve
                .Where(x => x.ShortName != null)
                .Select(x => new { Name = $"-{x.ShortName}", Option = x })
                .Concat(optionsToResolve
                            .Where(x => x.LongName != null)
                            .Select(x => new { Name = $"--{x.LongName}", Option = x }))
                .ToDictionary(x => x.Name, x => x.Option);

            var result = optionsToResolve
                .OrderBy(x => x.ParameterInfo.Position)
                .Select(x => x.ParameterInfo.IsOptional ? x.ParameterInfo.DefaultValue : null)
                .ToArray();
            while (args.Any() && args.Peek().StartsWith("-"))
            {
                var optionArg = args.Dequeue();
                var delimeterIndex = optionArg.IndexOfAny(NameValueDelimeters);

                string optionArgName = optionArg.Substring(0, (delimeterIndex > 0 ? delimeterIndex : optionArg.Length));
                if (!optionsMap.ContainsKey(optionArgName))
                    throw new CommandExecutionException($"Option {optionArgName} is not recognized");
                var option = optionsMap[optionArgName];
                if (!optionsToResolve.Remove(option))
                    throw new CommandExecutionException($"Option {optionArgName} is duplicated");

                var parameterType = option.ParameterInfo.ParameterType;
                parameterType = Nullable.GetUnderlyingType(parameterType) ?? parameterType;
                var isBoolParameter = parameterType == typeof(bool);
                var optionArgValue = delimeterIndex > 0
                    ? optionArg.Substring(delimeterIndex + 1)
                    : isBoolParameter ? null : args.Dequeue();
                result[option.ParameterInfo.Position] = isBoolParameter && optionArgValue == null ? true
                    : Convert.ChangeType(optionArgValue, parameterType);
            }

            var requiredOptions = optionsToResolve
                .Where(x => !x.ParameterInfo.IsOptional);
            if (requiredOptions.Any())
                throw new CommandExecutionException($"Options required: {string.Join(",", requiredOptions.Select(x => x.LongName ?? x.ShortName))}");

            return result;
        }

        /// <summary>
        /// Попытка поиска опции показа справки и определения команды, для которой необходимо отобразить справку
        /// </summary>
        /// <param name="args">Аргументы коммандной строки</param>
        /// <param name="command">Определенная команда, для которой необходимо отобразить справку</param>
        /// <returns>true, если обнаружена опция отображения справки</returns>
        private bool TryFindCommandForHelp(IEnumerable<string> args, out Command command)
        {
            command = this;
            foreach (var arg in args)
            {
                if (command != null && !arg.StartsWith("-"))
                {
                    command = command.Commands.FirstOrDefault(x => x.Name == arg) ?? command;
                }

                if (arg == $"-{Option.HelpOption.ShortName}"
                    || arg == $"--{Option.HelpOption.LongName}")
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Запуск консоли команды
        /// </summary>
        /// <param name="command">Команда</param>
        /// <param name="target">Целевой объект команды</param>
        private static void EnterCommandConsole(Command command, object target)
        {
            if (!command._commandMethod.ReturnType.GetTypeInfo().IsAssignableFrom(target.GetType()))
                throw new ArgumentException("command is not applicable for target");

            Console.WriteLine($"You are entering to the \"{string.Join(" ", GetCommandPath(command).Select(x => x.Name))}\" command console");
            Console.WriteLine("Use \"exit\" command to exit from the command console");
            while (true)
            {
                Console.Write($"{command.Name} > ");
                var line = Console.ReadLine().Trim();
                if (!string.IsNullOrEmpty(line))
                {
                    if (ExitCommandName.Equals(line, StringComparison.OrdinalIgnoreCase))
                        break;
                    var args = new Queue<string>(ParseCommandLine(line));

                    Command commandHelp;
                    if (command.TryFindCommandForHelp(args, out commandHelp))
                    {
                        if (commandHelp == null)
                            throw new CommandExecutionException("Unrecognized command");
                        commandHelp.ShowHelp(commandHelp != command);
                    }
                    else
                        try
                        {
                            command.ExecuteChildCommand(args, target, true);
                        }
                        catch (CommandExecutionException e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        catch (Exception e)
                        {
                            Console.Out.Print(e);
                        }
                }
            }
        }

        /// <summary>
        /// Запуск команды из статического метода
        /// </summary>
        /// <typeparam name="R">Тип возвращаемого методом значения</typeparam>
        /// <param name="commandDelegate">Указатель на статический метод корневой команды</param>
        /// <param name="args">Аргументы коммандной строки</param>
        public static void Run<R>(Func<R> commandDelegate, params string[] args)
        {
            var command = new Command(null, commandDelegate.GetMethodInfo(), true);
            if (args.Any())
                command.Execute(new Queue<string>(args), null, true, true);
            else
                command.ShowHelp();
        }

        /// <summary>
        /// Преообразование строки в набор аргументов коммандной строки
        /// </summary>
        /// <param name="commandLine">Входная строка</param>
        /// <returns>Аргументы коммандной строки</returns>
        public static string[] ParseCommandLine(string commandLine)
        {
            return ParseCommandLineInternal(commandLine).ToArray();
        }

        private static IEnumerable<string> ParseCommandLineInternal(string commandLine)
        {
            var arg = new StringBuilder();
            var inQuotes = false;
            commandLine = commandLine.Trim();
            for (int i = 0; i < commandLine.Length; i++)
            {
                var c = commandLine[i];
                switch (c)
                {
                    case ' ':
                        if (inQuotes)
                            arg.Append(c);
                        else
                        {
                            if (arg.Length > 0)
                                yield return arg.ToString();
                            arg.Clear();
                        }
                        break;
                    case '"':
                        if (!inQuotes)
                            inQuotes = true;
                        else
                        {
                            var quoteCount = CountChars(commandLine, i);
                            i += quoteCount - 1;

                            for (int j = 0; j < quoteCount / 2; j++)
                                arg.Append('"');
                            if (quoteCount % 2 != 0)
                                inQuotes = false;
                        }
                        continue;
                    case '\\':
                        var backSlasheCount = CountChars(commandLine, i);
                        i += backSlasheCount - 1;

                        if (i + 1 < commandLine.Length && commandLine[i + 1] == '"')
                        {
                            for (int j = 0; j < backSlasheCount / 2; j++)
                                arg.Append('\\');
                            if (backSlasheCount % 2 != 0)
                            {
                                arg.Append('"');
                                i++;
                            }
                        }
                        else
                        {
                            for (int j = 0; j < backSlasheCount; j++)
                                arg.Append('\\');
                        }
                        continue;
                    default:
                        arg.Append(c);
                        break;
                }
            }

            if (arg.Length > 0)
                yield return arg.ToString();
        }

        private static int CountChars(string s, int index)
        {
            var firstCharIndex = index;
            var firstOtherCharIndex = index + 1;
            for (; firstOtherCharIndex < s.Length && s[firstOtherCharIndex] == s[firstCharIndex]; firstOtherCharIndex++) ;
            return firstOtherCharIndex - firstCharIndex;
        }
    }
}
