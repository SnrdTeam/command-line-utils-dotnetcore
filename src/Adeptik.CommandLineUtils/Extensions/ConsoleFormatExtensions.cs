using System;
using System.Collections.Generic;
using System.Linq;

namespace Adeptik.CommandLineUtils.Extensions
{
    /// <summary>
    /// Методы расширения для форматирования данных, отображаемых в консоли
    /// </summary>
    public static class ConsoleFormatExtensions
    {
        /// <summary>
        /// Разделитель сстолбцов таблицы по усолчанию
        /// </summary>
        public const string DefaultColumnDelimeter = "  ";

        /// <summary>
        /// Преобразование коллекции данных в таблицу для вывода в консоль
        /// </summary>
        /// <typeparam name="T">Тип элемента таблицы</typeparam>
        /// <param name="source">Коллекция данных таблицы</param>
        /// <param name="printHeader">Генерировать строку с заголовком таблицы</param>
        /// <param name="columnDelimeter">Разделитель столбцов</param>
        /// <param name="columns">Столбцы таблицы</param>
        /// <returns>Строковое представление таблицы</returns>
        public static IEnumerable<string> ToTable<T>(this IEnumerable<T> source,
            bool printHeader,
            string columnDelimeter,
            params Column<T>[] columns)
        {
            var data = source
                .Select(row => columns.Select(x => x.ValueSelector(row)).ToArray())
                .ToList();

            var rowFormat = string.Join(
                columnDelimeter,
                columns
                    .Take(columns.Length - 1)
                    .Select((column, colIndex) =>
                    {
                        var columnWidth = data.Any() ? data.Max(row => row[colIndex]?.Length ?? 0) : 0;
                        if (printHeader)
                            columnWidth = Math.Max(columnWidth, column.Name.Length);
                        return $"{{{colIndex},-{columnWidth}}}";
                    })
                    .Concat(new[] { $"{{{columns.Length - 1}}}" }));

            if (printHeader)
                yield return string.Format(rowFormat, columns.Select(x => x.Name).ToArray());

            foreach (var row in data)
            {
                yield return string.Format(rowFormat, row);
            }
        }

        /// <summary>
        /// Преобразование коллекции данных в таблицу для вывода в консоль
        /// </summary>
        /// <typeparam name="T">Тип элемента таблицы</typeparam>
        /// <param name="source">Коллекция данных таблицы</param>
        /// <param name="columns">Столбцы таблицы</param>
        /// <returns>Строковое представление таблицы</returns>
        public static IEnumerable<string> ToTable<T>(this IEnumerable<T> source,
            params Column<T>[] columns)
        {
            return source.ToTable(true, DefaultColumnDelimeter, columns);
        }

        /// <summary>
        /// Преобразование коллекции данных в таблицу для вывода в консоль
        /// </summary>
        /// <typeparam name="T">Тип элемента таблицы</typeparam>
        /// <param name="source">Коллекция данных таблицы</param>
        /// <param name="printHeader">Генерировать строку с заголовком таблицы</param>
        /// <param name="columns">Столбцы таблицы</param>
        /// <returns>Строковое представление таблицы</returns>
        public static IEnumerable<string> ToTable<T>(this IEnumerable<T> source,
            bool printHeader,
            params Column<T>[] columns)
        {
            return source.ToTable(printHeader, DefaultColumnDelimeter, columns);
        }

        /// <summary>
        /// Преобразование коллекции данных в таблицу для вывода в консоль
        /// </summary>
        /// <typeparam name="T">Тип элемента таблицы</typeparam>
        /// <param name="source">Коллекция данных таблицы</param>
        /// <param name="columnDelimeter">Разделитель столбцов</param>
        /// <param name="columns">Столбцы таблицы</param>
        /// <returns>Строковое представление таблицы</returns>
        public static IEnumerable<string> ToTable<T>(this IEnumerable<T> source,
            string columnDelimeter,
            params Column<T>[] columns)
        {
            return source.ToTable(true, columnDelimeter, columns);
        }
    }
}
