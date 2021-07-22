using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;

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

            var operation = args[0][1..]; 
            //Console.WriteLine(operation);
            switch (operation)
            {
                case "Q":
                    ListMods();
                    break;
                default:
                    Console.WriteLine($"Error: Unknown operation '{operation}'.");
                    break;
            }
        }

        static void ListMods()
        {
            List<string> modList = new List<string>();
            List<string> expansionList = new List<string>();
            var path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
                .ToString();
            path += "\\LocalLow\\Ludeon Studios\\RimWorld by Ludeon Studios\\Config\\ModsConfig.xml";
            XmlDocument modsConfigXmlDocument = new XmlDocument();
            modsConfigXmlDocument.Load(path);
            //Console.WriteLine(path);
            var ModsConfigDataNode = modsConfigXmlDocument.SelectSingleNode("ModsConfigData");
            var activeModsNode = ModsConfigDataNode.SelectSingleNode("activeMods");
            var knownExpansionsNode = ModsConfigDataNode.SelectSingleNode("knownExpansions");
            //Console.WriteLine(activeModsNode.OuterXml);
            foreach (XmlElement childNode in activeModsNode.ChildNodes)
            {
                modList.Add(childNode.InnerText);
            }

            foreach (XmlElement element in knownExpansionsNode)
            {
                expansionList.Add(element.InnerText);
            }

            Console.WriteLine("Active Mods:");
            foreach (var modName in modList)
            {
                Console.WriteLine($"\u001b[32m{modName}\u001b[0m");
            }

            Console.WriteLine();
            Console.WriteLine("Known expansions:");
            foreach (var expansionName in expansionList)
            {
                Console.WriteLine($"\u001b[33m{expansionName}\u001b[0m");
            }

            Console.WriteLine();
            Console.WriteLine($"Total: {modList.Count} " +
                              $"Expansions: {expansionList.Count}");
        }
    }
}
