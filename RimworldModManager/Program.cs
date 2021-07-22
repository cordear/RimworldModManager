using System;
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
            var path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
                .ToString();
            path += "\\LocalLow\\Ludeon Studios\\RimWorld by Ludeon Studios\\Config\\ModsConfig.xml";
            XmlDocument modsConfigXmlDocument = new XmlDocument();
            modsConfigXmlDocument.Load(path);
            //Console.WriteLine(path);
            var ModsConfigDataNode = modsConfigXmlDocument.SelectSingleNode("ModsConfigData");
            var activeModsNode = ModsConfigDataNode.SelectSingleNode("activeMods");
            //Console.WriteLine(activeModsNode.OuterXml);
            foreach (XmlElement childNode in activeModsNode.ChildNodes)
            {
                Console.WriteLine(childNode.InnerText);
            }

            Console.WriteLine($"Total: {activeModsNode.ChildNodes.Count}");
        }
    }
}
