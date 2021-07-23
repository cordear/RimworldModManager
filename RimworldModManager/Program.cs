using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
            List<ModInfo> modInfoList = new List<ModInfo>();
            var configPath = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
                .ToString();
            configPath += "\\LocalLow\\Ludeon Studios\\RimWorld by Ludeon Studios\\Config\\ModsConfig.xml";
            var gameModDirPath = "C:\\GOG Games\\RimWorld\\Mods";
            var modDirs=Directory.GetDirectories(gameModDirPath);

            XmlDocument modsConfigXmlDocument = new XmlDocument();
            modsConfigXmlDocument.Load(configPath);
            //Console.WriteLine(path);
            var ModsConfigDataNode = modsConfigXmlDocument.SelectSingleNode("ModsConfigData");
            var activeModsNode = ModsConfigDataNode.SelectSingleNode("activeMods");
            var knownExpansionsNode = ModsConfigDataNode.SelectSingleNode("knownExpansions");
            //Console.WriteLine(activeModsNode.OuterXml);

            foreach (var modDir in modDirs)
            {
                var aboutPath = modDir + "\\About";
                XmlDocument modAboutXmlDocument = new XmlDocument();
                modAboutXmlDocument.Load(aboutPath+"\\About.xml");
                var modMetaDataNode = modAboutXmlDocument.SelectSingleNode("ModMetaData");
                var name = modMetaDataNode.SelectSingleNode("name").InnerText;
                var packageId = modMetaDataNode.SelectSingleNode("packageId").InnerText;
                var idFile = new StreamReader(aboutPath + "\\PublishedFileId.txt");
                string id = idFile.ReadLine();
                idFile.Close();
                modInfoList.Add(new ModInfo(packageId,id,name));
            }

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

            //var maxPackageIdLength = modInfoList.Select(x => x.PackageId.Length).Max();
            //var maxNameLength = modInfoList.Select(x => x.Name.Length).Max();
            //var maxIdLength = modInfoList.Select(x => x.Id.Length).Max();

            Console.WriteLine($"{"Name",-45}{"Package Id",-45}{"Id",-15}");
            Console.WriteLine($"{"----",-45}{"----------",-45}{"--",-15}");
            foreach (var modInfo in modInfoList)
            {
                Console.WriteLine($"{modInfo.Name,-45}{modInfo.PackageId,-45}{modInfo.Id,-15}");
            }
        }
    }
}
