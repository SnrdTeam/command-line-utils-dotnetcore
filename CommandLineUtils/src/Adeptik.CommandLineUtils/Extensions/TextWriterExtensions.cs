using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Adeptik.CommandLineUtils.Extensions
{
    public static class TextWriterExtensions
    {
        /// <summary>
        /// Вывод в консоль коллекции строк
        /// </summary>
        /// <param name="textWriter"><see cref="TextWriter"/>, в который необходимо вывести данные</param>
        /// <param name="table">Данные</param>
        public static void Print(this TextWriter textWriter, IEnumerable<string> table)
        {
            if (textWriter == null)
                throw new ArgumentNullException(nameof(textWriter));
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            foreach (var row in table)
                textWriter.WriteLine(row);
        }

        /// <summary>
        /// Вывод в консоль информацию об ошибке
        /// </summary>
        /// <param name="textWriter"><see cref="TextWriter"/>, в который необходимо вывести данные</param>
        /// <param name="exception">Исключение, описывающее ошибку</param>
        /// <param name="innerExceptionIndent">Отступ для вложенных исключений</param>
        public static void Print(this TextWriter textWriter,
            Exception exception,
            string innerExceptionIndent = " ")
        {
            if (textWriter == null)
                throw new ArgumentNullException(nameof(textWriter));
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            if (exception is TargetInvocationException && exception.InnerException != null)
                textWriter.Print(exception.InnerException);
            else if (exception is AggregateException)
            {
                foreach (var e in ((AggregateException)exception).InnerExceptions.Where(x => x != null))
                {
                    textWriter.Print(e);
                }
            }
            else
            {
                var indent = "";
                for (var e = exception; e != null; e = e.InnerException)
                {
                    Console.Write(indent);
                    Console.WriteLine(e.Message);
                    indent += innerExceptionIndent;
                }
            }
        }
    }
}
