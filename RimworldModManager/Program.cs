using System;
using System.IO;

namespace RimworldModManager
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length==0 || args[0][0]!='-')
            {
                Console.WriteLine("Error: No operation specified. (use -h for help)");
                return;
            }

            if (args[0].Length == 1)
            {
                Console.WriteLine("Error: Argument '-' specified without input. (use -h for help)");
                return;
            }

            var operation = args[0];
            Console.WriteLine(operation);
        }
    }
}
