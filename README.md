# Adeptik Command Line Utils

Набор утилит для создания консольных приложений для .NET Core.

## Использование

Добавьте в проект зависимость NuGet "Adeptik.CommandLineUtils" либо выполните в Package Manager Console команду

    Install-Package Adeptik.CommandLineUtils

## Command Engine

Утилита для интерпретирования аргументов командной строки.

### Принцип действия

Любой аргумент командной строки может интерпретироваться как:

  * команда
  * опция команды

#### Команды

Команда - это аргумент, не имеющий специальных префиксов.

Каждая команда создается в определенном контексте. Контекст команды формируется полной цепочкой команд, предшествующих ей в командной строке.

Каждая команда может иметь опции.

Результатом команды может быть:

  * Контекст, в рамках которого выполняются другие команды.
  * Исполнение определенной логики команды.

Если в цепочке команд, полученной из аргументов командной строки последняя имеет контекст (возвращаемое значение не `void`), то выполнение приложения переходит в режим выполнения команды.
В данном режиме приложение ожидает ввода от пользователя аргументов командной строки, считая эту команду корневой.

#### Опции

Опция - это аргумент, начинающийся с "-" (краткое имя опции) или "--" (полное имя опции).
Опция имеет определенный тип данных. Значение опции задается в командной строке после указания имени опции и разделяется одним из следующих способов:

  * пробел
  * знак "="
  * знак ":"

### Реализация

В коде команда - это метод. Данный метод должен быть отмечен атрибутом `CommandAttribute`. Метод интерпретируется следующим образом:

  * имя метода - это имя команды;
  * параметры метода - это опции команды. При выполнении команды производится преобразование аргументов к типам данных, соответствующим типам параметров метода;
  * возвращаемое значение метода определяет контекст, в рамках которого выполняются дочерние команды. Если возвращаемый тип данных метода `void`, то тело метода содержит всю логику команды.

Вызов консольного приложения приравнивается к вызову корневой команды. Имя метода для данной команды не учитывается. Данный метод должен быть статическим.

#### Контекст выполнения команды

В коде контекст - это класс, удовлетворяющий условиям:

  * класс должен быть `sealed`
  * все методы, являющиеся командами, должны быть методами экземпляра класса (не статическими) и быть помечены атрибутом `CommandAttribute`.

### Пример кода

```csharp
namespace ConsoleApp
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                Command.Run(Root, args);
            }
            catch (CommandExecutionException e)
            {
                // Invalid command line arguments
                Console.WriteLine(e.Message);
                return 1;
            }
            catch (Exception e)
            {
                // Execution error
                Console.WriteLine(e.Message);
                return 2;
            }

            return 0;
        }

        // Корневая команда
        [Command]
        private static RootCommand Root()
        {
            return new RootCommand();
        }

        // Контекст дочерних команд корневой команды 
        private sealed class RootCommand
        {
            [Command(Description = "Connect to the Server")]
            public ConnectCommand connect(
                [CommandOption("s", Description = "Url of the Server")]
                string url,
                [CommandOption("u", Description = "User name")]
                string user,
                [CommandOption("p", Description = "Password")]
            string password)
            {
                return new ConnectCommand(url, user, password);
            }
        }

        // Контекст дочерних команд команд connect 
        private sealed class ConnectCommand
        {
            private string password;
            private string url;
            private string user;

            public ConnectCommand(string url, string user, string password)
            {
                this.url = url;
                this.user = user;
                this.password = password;
            }

            [Command(Description = "Executes some action on the Server")]
            public void SomeServerAction()
            {
                Console.WriteLine($"Executing some action on the Server {url}");
            }
        }
    }
}
```

## Лицензия

Copyright 2017 Adeptik

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.