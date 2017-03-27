# Adeptik Command Line Utils

����� ������ ��� �������� ���������� ���������� ��� .NET Core.

## �������������

�������� � ������ ����������� NuGet "Adeptik.CommandLineUtils" ���� ��������� � Package Manager Console �������

    Install-Package Adeptik.CommandLineUtils

## Command Engine

������� ��� ����������������� ���������� ��������� ������.

### ������� ��������

����� �������� ��������� ������ ����� ������������������ ���:

  * �������
  * ����� �������

#### �������

������� - ��� ��������, �� ������� ����������� ���������.

������ ������� ��������� � ������������ ���������. �������� ������� ����������� ������ �������� ������, �������������� �� � ��������� ������.

������ ������� ����� ����� �����.

����������� ������� ����� ����:

  * ��������, � ������ �������� ����������� ������ �������.
  * ���������� ������������ ������ �������.

���� � ������� ������, ���������� �� ���������� ��������� ������ ��������� ����� �������� (������������ �������� �� `void`), �� ���������� ���������� ��������� � ����� ���������� �������.
� ������ ������ ���������� ������� ����� �� ������������ ���������� ��������� ������, ������ ��� ������� ��������.

#### �����

����� - ��� ��������, ������������ � "-" (������� ��� �����) ��� "--" (������ ��� �����).
����� ����� ������������ ��� ������. �������� ����� �������� � ��������� ������ ����� �������� ����� ����� � ����������� ����� �� ��������� ��������:

  * ������
  * ���� "="
  * ���� ":"

### ����������

� ���� ������� - ��� �����. ������ ����� ������ ���� ������� ��������� `CommandAttribute`. ����� ���������������� ��������� �������:

  * ��� ������ - ��� ��� �������;
  * ��������� ������ - ��� ����� �������. ��� ���������� ������� ������������ �������������� ���������� � ����� ������, ��������������� ����� ���������� ������;
  * ������������ �������� ������ ���������� ��������, � ������ �������� ����������� �������� �������. ���� ������������ ��� ������ ������ `void`, �� ���� ������ �������� ��� ������ �������.

����� ����������� ���������� �������������� � ������ �������� �������. ��� ������ ��� ������ ������� �� �����������. ������ ����� ������ ���� �����������.

#### �������� ���������� �������

� ���� �������� - ��� �����, ��������������� ��������:

  * ����� ������ ���� `sealed`
  * ��� ������, ���������� ���������, ������ ���� �������� ���������� ������ (�� ������������) � ���� �������� ��������� `CommandAttribute`.

### ������ ����

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

        // �������� �������
        [Command]
        private static RootCommand Root()
        {
            return new RootCommand();
        }

        // �������� �������� ������ �������� ������� 
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

        // �������� �������� ������ ������ connect 
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

## ��������

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