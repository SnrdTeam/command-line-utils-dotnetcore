using System;

namespace Adeptik.CommandLineUtils.Extensions
{
    /// <summary>
    /// Описание столбца таблицы данных
    /// </summary>
    /// <typeparam name="T">Тип элемента таблицы</typeparam>
    public class Column<T>
    {
        public Column(string name, Func<T, string> valueSelector)
        {
            Name = name;
            ValueSelector = valueSelector;
        }

        /// <summary>
        /// Имя столбца таблицы
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Функция получения значения колонки
        /// </summary>
        public Func<T, string> ValueSelector { get; }
    }
}
