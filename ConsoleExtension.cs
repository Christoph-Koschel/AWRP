using System;
using System.IO;

namespace AWRP {
    public static class ConsoleExtension {
        public static string ReadPassword(this TextReader reader) {
            if (Console.In != reader) {
                throw new Exception("'ReadPassword' can only be used on the console interface");
            }

            string password = string.Empty;
            ConsoleKey key;

            do {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && password.Length > 0) {
                    Console.Write("\b \b");
                    password = password.Substring(0, password.Length - 1);
                } else if (!char.IsControl(keyInfo.KeyChar)) {
                    Console.Write("*");
                    password += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);

            Console.WriteLine();
            return password;
        }

        public static bool Decision(this TextReader reader, string question) {
            if (Console.In != reader) {
                throw new Exception("'Decision' can only be used on the console interface");
            }

            Console.Write(question);
            Console.Write(" (y|n): ");
            ConsoleKeyInfo key = Console.ReadKey();            
            switch(key.KeyChar) {
                case 'y':
                case 'j':
                    return true;
                default:
                    return false;
            }
        }
    }
}