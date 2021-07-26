using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualBasic;

namespace RimworldModManager
{
    class Program
    {
        static readonly HttpClient client = new HttpClient();
        static readonly Uri DownloaderUri = new Uri("https://backend-02-prd.steamworkshopdownloader.io/");
        static void Main(string[] args)
        {
            client.BaseAddress = DownloaderUri;
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
            switch (operation.First())
            {
                case 'Q':
                    ListMods(operation);
                    break;
                case 'T':
                    Console.WriteLine("This is a test commend");
                    CreateModConfigXml();
                    break;
                default:
                    Console.WriteLine($"Error: Unknown operation '{operation}'.");
                    break;
            }
        }

        static void ListMods(string operation)
        {
            var currentDir = Environment.CurrentDirectory;
            List<ModInfo> modInfoList = new List<ModInfo>();
            if (operation.Length == 1)
            {
                if (!File.Exists(currentDir + "\\modConfig.xml"))
                {
                    Console.WriteLine("There is no modConfig.xml in current directory. Now creating...");
                    CreateModConfigXml();
                }
                else
                {
                    ModConfigXmlparser(ref modInfoList);
                    Console.WriteLine($"{"Name",-40}{"Id",-15}{"Create Time",-15}{"Active",-10}");
                    Console.WriteLine($"{"----",-40}{"--",-15}{"-----------",-15}{"------",-10}");
                    foreach (var modInfo in modInfoList)
                    {
                        Console.WriteLine($"{modInfo.Name,-40}{modInfo.Id,-15}" +
                                          $"{modInfo.CreateTime.ToShortDateString(),-15}{modInfo.IsActive,-10}");
                    }
                }
            }
            else
            {
                switch (operation[1])
                {
                    case 'y':
                        if (File.Exists(currentDir + "\\modConfig.xml"))
                        {
                            Console.Write("modConfig.xml already exist. Recreate? [Y/n]:");
                            var key= Console.ReadKey();
                            Console.WriteLine();
                            while (key.Key != ConsoleKey.Y && key.Key != ConsoleKey.N && key.Key != ConsoleKey.Enter)
                            {
                                Console.Write("Illegal input, try again [Y/n]:");
                                key = Console.ReadKey();
                                Console.WriteLine();
                            }
                            if (key.Key is ConsoleKey.Y or ConsoleKey.Enter)
                            {
                                CreateModConfigXml();
                            }
                            else
                            {
                                Console.WriteLine("Nothing changed.");
                            }
                        }
                        else
                        {
                            CreateModConfigXml();
                        }
                        break;
                    case 'u':
                        Console.WriteLine("Now checking mod upgrade...");
                        ModConfigXmlparser(ref modInfoList);
                        foreach (var modInfo in modInfoList)
                        {
                             var result= CheckModUpgradeAsync(modInfo.Id);
                            //Console.WriteLine(result.Result.title);

                        }

                        Console.ReadKey();
                        break;
                    default:
                        Console.WriteLine("Unknown operation. Try again.");
                        break;
                }
                return;
            }

            //var maxPackageIdLength = modInfoList.Select(x => x.PackageId.Length).Max();
            //var maxNameLength = modInfoList.Select(x => x.Name.Length).Max();
            //var maxIdLength = modInfoList.Select(x => x.Id.Length).Max();

        }

        static bool CreateModConfigXml()
        {
            List<string> expansionList = new List<string>();
            List<ModInfo> modInfoList = new List<ModInfo>();
            HashSet<string> modHashSet = new HashSet<string>();
            var configPath = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
                .ToString();
            configPath += "\\LocalLow\\Ludeon Studios\\RimWorld by Ludeon Studios\\Config\\ModsConfig.xml";
            var gameModDirPath = "C:\\GOG Games\\RimWorld\\Mods";
            var modDirs = Directory.GetDirectories(gameModDirPath);

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
                modAboutXmlDocument.Load(aboutPath + "\\About.xml");
                var modMetaDataNode = modAboutXmlDocument.SelectSingleNode("ModMetaData");
                var name = modMetaDataNode.SelectSingleNode("name").InnerText;
                var packageId = modMetaDataNode.SelectSingleNode("packageId").InnerText;
                var idFile = new StreamReader(aboutPath + "\\PublishedFileId.txt");
                string id = idFile.ReadLine();
                idFile.Close();
                var createTime = Directory.GetCreationTime(aboutPath + "\\About.xml");
                modInfoList.Add(new ModInfo(packageId, id, name, createTime));
            }

            modInfoList = modInfoList.OrderBy(x => x.Name).ToList();

            foreach (XmlElement childNode in activeModsNode.ChildNodes)
            {
                modHashSet.Add(childNode.InnerText);
            }

            foreach (var t in modInfoList)
            {
                if (modHashSet.Contains(t.PackageId.ToLower()))
                {
                    t.IsActive = true;
                }
            }
            var modConfigXmlPath = Environment.CurrentDirectory + "\\modConfig.xml";

            var modConfigXml = new XmlDocument();
            var rootNode = modConfigXml.CreateElement("Mods");
            modConfigXml.AppendChild(rootNode);
            foreach (var modInfo in modInfoList)
            {
                var modInfoNode = modConfigXml.CreateElement("ModInfo");
                var modNameNode = modConfigXml.CreateElement("Name");
                modNameNode.InnerText = modInfo.Name;
                var modPackageIdNode = modConfigXml.CreateElement("PackageId");
                modPackageIdNode.InnerText = modInfo.PackageId;
                var modIdNode = modConfigXml.CreateElement("Id");
                modIdNode.InnerText = modInfo.Id;
                var modStatusNode = modConfigXml.CreateElement("Status");
                modStatusNode.InnerText = modInfo.IsActive.ToString();
                var modCreateTimeNode = modConfigXml.CreateElement("CreateTime");
                modCreateTimeNode.InnerText = Convert.ToString(((DateTimeOffset) modInfo.CreateTime).ToUnixTimeSeconds());
                modInfoNode.AppendChild(modNameNode);
                modInfoNode.AppendChild(modPackageIdNode);
                modInfoNode.AppendChild(modIdNode);
                modInfoNode.AppendChild(modStatusNode);
                modInfoNode.AppendChild(modCreateTimeNode);
                rootNode.AppendChild(modInfoNode);
            }
            modConfigXml.Save(modConfigXmlPath);
            Console.WriteLine($"modConfig.xml created. Path:{modConfigXmlPath}");
            return true;
        }

        static bool ModConfigXmlparser(ref List<ModInfo> modInfoList)
        {
            var currentDir = Environment.CurrentDirectory;
            var modConfigXml = new XmlDocument();
            modConfigXml.Load(currentDir + "\\modConfig.xml");
            var rootNode = modConfigXml.SelectSingleNode("Mods");
            foreach (XmlElement childNode in rootNode.ChildNodes)
            {
                modInfoList.Add(new ModInfo(childNode.SelectSingleNode("PackageId").InnerText,
                    childNode.SelectSingleNode("Id").InnerText,
                    childNode.SelectSingleNode("Name").InnerText,
                    DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(childNode.SelectSingleNode("CreateTime").InnerText)).UtcDateTime,
                    childNode.SelectSingleNode("Status").InnerText == "True"
                ));
            }
            return true;
        }

        static async Task<Root> CheckModUpgradeAsync(string Id)
        {
            try
            {
                var content = new StringContent($"[{Id}]");
                var response = await client.PostAsync("api/details/file", content);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStreamAsync();
                var modResponse = await JsonSerializer.DeserializeAsync<Root[]>(result);
                Console.WriteLine(modResponse[0].title);
                return modResponse[0];
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }

            return null;
        }
    }
}
