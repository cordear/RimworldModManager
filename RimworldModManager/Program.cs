using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualBasic;

namespace RimworldModManager
{
    class Program
    {
        static readonly HttpClient client = new HttpClient();
        static readonly Uri CheckInfoUri = new Uri("https://backend-02-prd.steamworkshopdownloader.io/");
        private static Setting setting = new Setting();
        static void Main(string[] args)
        {
            if (!InitSetting())
            {
                Console.WriteLine("RimworldModManagerSetting.json missing or having wrong value.");
                return;
            }
            client.BaseAddress = CheckInfoUri;
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
                case 'S':
                    SetupMods(operation,args);
                    break;
                default:
                    Console.WriteLine($"Error: Unknown operation '{operation}'.");
                    break;
            }
        }

        static void SetupMods(string operation,string[] args)
        {
            if (operation.Length == 1)
            {
                if (args.Length == 1)
                {
                    Console.WriteLine("No mod id specified.");
                }
                else
                {
                    List<long> modIdList = new List<long>();
                    for (int i = 1; i < args.Length; ++i)
                    {
                        if (!Int64.TryParse(args[i], out var modId))
                        {
                            Console.WriteLine($"Mod id in wrong format (must be a number): {args[i]}. Try again.");
                            return;
                        }
                        modIdList.Add(modId);
                    }

                    InstallMods(modIdList);

                }
            }
            else
            {
                switch (operation[1])
                {
                    case 'u':
                        var modInfoList = new List<ModInfo>();
                        Console.WriteLine("Now checking mod upgrade...");
                        ModConfigXmlparser(ref modInfoList);
                        var resultList = GetModUpdateList(modInfoList);
                        InstallMods(resultList.Select(x=>Convert.ToInt64(x.Id)).ToList());
                        break;
                    default:
                        Console.WriteLine("Unknown operation. Try again.");
                        break;
                }
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
                    Console.WriteLine($"{"Id",-15}{"Create Time",-15}{"Active",-10}{"Name",-40}");
                    Console.WriteLine($"{"--",-15}{"-----------",-15}{"------",-10}{"----",-40}");
                    foreach (var modInfo in modInfoList)
                    {
                        Console.WriteLine($"{modInfo.Id,-15}" +
                                          $"{modInfo.CreateTime.ToShortDateString(),-15}" +
                                          $"{modInfo.IsActive,-10}{modInfo.Name,-40}");
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
                        var resultList = GetModUpdateList(modInfoList);
                        Dictionary<string, ModInfo> modInfoDict = new Dictionary<string, ModInfo>();
                        foreach (var modInfo in modInfoList)
                        {
                            modInfoDict.Add(modInfo.Id, modInfo);
                        }

                        if (resultList.Count == 0)
                        {
                            Console.WriteLine("No mod can be upgrade.");
                            return;
                        }
                        Console.WriteLine($"{resultList.Count} mods can be upgrade:");
                        Console.WriteLine($"{"Id",-15}{"Version",-40}{"Name",-40}");
                        Console.WriteLine($"{"--",-15}{"-------",-40}{"----",-40}");
                        foreach (var mod in resultList)
                        {
                            var modInfo = modInfoDict[mod.Id];
                            var modUpgradeTime = UnixTimeStampToDateTime(mod.TimeStamp);
                            Console.WriteLine($"{modInfo.Id,-15}" +
                                              $"{modInfo.CreateTime.ToShortDateString()+" -> "+modUpgradeTime.ToShortDateString(),-40}" +
                                              $"{modInfo.Name,-40}");
                        }
                        
                        //Console.ReadKey();
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
            var configPath = setting.ModConfigXmlPath + "\\ModsConfig.xml";
            var modDirs = Directory.GetDirectories(setting.GameModDirPath);

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
                modCreateTimeNode.InnerText = 
                    Convert.ToString(((DateTimeOffset) modInfo.CreateTime).ToUnixTimeSeconds());
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
                    DateTimeOffset.FromUnixTimeSeconds(
                        Convert.ToInt64(childNode.SelectSingleNode("CreateTime").InnerText)).UtcDateTime,
                    childNode.SelectSingleNode("Status").InnerText == "True"
                ));
            }
            return true;
        }
        
        static async Task<modUpgradeInfo> CheckModUpgradeAsync(string id,DateTime localModeCreateTime)
        {
            try
            {
                var content = new StringContent($"[{id}]");
                var response = await client.PostAsync("api/details/file", content);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStreamAsync();
                var modResponse = await JsonSerializer.DeserializeAsync<Root[]>(result);
                if (modResponse[0].time_updated > ((DateTimeOffset) localModeCreateTime).ToUnixTimeSeconds())
                {
                    return new modUpgradeInfo(id, true, modResponse[0].time_updated);
                }

                return new modUpgradeInfo(id, false, modResponse[0].time_updated);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }

            return new modUpgradeInfo(id, false, 0);
        }

        public static async Task<Root> GetModInfoAsync(long id)
        {
            try
            {
                var content = new StringContent($"[{id}]");
                var response = await client.PostAsync("api/details/file", content);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStreamAsync();
                var modResponse = await JsonSerializer.DeserializeAsync<Root[]>(result);

                return modResponse[0];
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }

            return null;
        }

        public static async Task DownloadModAsync(Root modInfo)
        {
            var content = new StringContent($"{{\"publishedFileId\":{Convert.ToUInt64(modInfo.publishedfileid)}," +
                                            $"\"collectionId\":null,\"extract\":true," +
                                            $"\"hidden\":false,\"direct\":false}}");
            var response =await client.PostAsync("api/download/request", content);
            var uuid = await JsonSerializer.DeserializeAsync<UUID>(await response.Content.ReadAsStreamAsync());
            var downloadContent = await client.GetAsync($"api/download/transmit?uuid={uuid.uuid}");
            while (!downloadContent.IsSuccessStatusCode)
            {
                content = new StringContent($"{{\"publishedFileId\":{Convert.ToUInt64(modInfo.publishedfileid)}," +
                                            $"\"collectionId\":null,\"extract\":true," +
                                            $"\"hidden\":false,\"direct\":false}}");
                response = await client.PostAsync("api/download/request", content);
                uuid = await JsonSerializer.DeserializeAsync<UUID>(await response.Content.ReadAsStreamAsync());
                Console.WriteLine($"Trying to re-download:{modInfo.title} {uuid.uuid}");
                downloadContent = await client.GetAsync($"api/download/transmit?uuid={uuid.uuid}");

            }
            var fs = new FileStream(setting.ModTempDirPath+$"{modInfo.title_disk_safe}.zip", FileMode.Create);
            await downloadContent.Content.CopyToAsync(fs);

            fs.Close();
        }
        public static List<modUpgradeInfo> GetModUpdateList(List<ModInfo> modInfoList)
        {
            var taskList = new List<Task<modUpgradeInfo>>();
            foreach (var modInfo in modInfoList)
            {
                var result = CheckModUpgradeAsync(modInfo.Id, modInfo.CreateTime);
                //Console.WriteLine(result.Result.title);
                taskList.Add(result);

            }

            Task.WaitAll(taskList.ToArray());
            var resultList = taskList.Where(x => x.Result.CanUpgrade).
                Select(x => x.Result).ToList();
            return resultList;
        }

        public static void InstallMods(List<long> modIdList)
        {
            List<Task<Root>> modInfoList = new List<Task<Root>>();
            Console.WriteLine("Now checking mod information...");
            foreach (var id in modIdList)
            {
                modInfoList.Add(GetModInfoAsync(id));
            }

            Task.WaitAll(modInfoList.ToArray());
            double totalSize = 0;
            Console.WriteLine("Install mod list:");
            foreach (var modInfo in modInfoList)
            {
                Console.WriteLine($"{modInfo.Result.title}" +
                                  $" - {UnixTimeStampToDateTime(modInfo.Result.time_updated).ToShortDateString()}");
                totalSize += Convert.ToInt64(modInfo.Result.file_size);
            }

            Console.WriteLine($"Total file size: {totalSize / 1024 / 1024:0.00}M");
            Console.Write("Continue to install? [Y/n]:");
            var key = Console.ReadKey();
            Console.WriteLine();
            while (key.Key != ConsoleKey.Y && key.Key != ConsoleKey.N && key.Key != ConsoleKey.Enter)
            {
                Console.Write("Illegal input, try again [Y/n]:");
                key = Console.ReadKey();
                Console.WriteLine();
            }
            if (key.Key is ConsoleKey.Y or ConsoleKey.Enter)
            {
                Console.WriteLine("Now downloading...");
                List<Task> downloadTaskList = new List<Task>();
                foreach (var mod in modInfoList)
                {
                    downloadTaskList.Add(DownloadModAsync(mod.Result));
                }

                Task.WaitAll(downloadTaskList.ToArray());
                Console.WriteLine("Now unzip file...");
                foreach (var mod in modInfoList)
                {
                    var modDir = setting.GameModDirPath +
                                 $"\\{mod.Result.publishedfileid}_{mod.Result.title_disk_safe}";
                    if (Directory.Exists(modDir))
                    {
                        Console.WriteLine($"Removing exist directory: {modDir}");
                        Directory.Delete(modDir, true);
                    }
                    ZipFile.ExtractToDirectory($"{mod.Result.title_disk_safe}.zip",
                        modDir);
                }

                Console.WriteLine("Now recreating modConfig.xml...");
                CreateModConfigXml();
                Console.WriteLine("Done.");
            }
            else
            {
                Console.WriteLine("Installation canceled.");
            }
        }
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = 
                new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private static bool InitSetting()
        {
            if (!File.Exists("./RimworldModManagerSetting.json"))
            {
                Console.Write("RimworldModManagerSetting.json seems missing, create now? [Y/n]:");
                var key = Console.ReadKey();
                Console.WriteLine();
                while (key.Key != ConsoleKey.Y && key.Key != ConsoleKey.N && key.Key != ConsoleKey.Enter)
                {
                    Console.Write("Illegal input, try again [Y/n]:");
                    key = Console.ReadKey();
                    Console.WriteLine();
                }
                if (key.Key is ConsoleKey.Y or ConsoleKey.Enter)
                {
                    File.WriteAllText("./RimworldModManagerSetting.json",JsonSerializer.Serialize(setting));
                    Console.WriteLine("RimworldModManagerSetting.json created. Modify the file before using other command.");
                    return false;
                }
                Console.WriteLine("Nothing changed.");
                return false;
            }

            var fs = new StreamReader("./RimworldModManagerSetting.json");
            setting = JsonSerializer.Deserialize<Setting>(fs.ReadToEnd());
            fs.Close();
            if (!Directory.Exists("./RimworldModDownloadTemp"))
            {
                Console.WriteLine("Now creating temp directory for mod download.");
                Directory.CreateDirectory("./RimworldModDownloadTemp");
            }
            //Console.WriteLine(setting.GameModDirPath);
            //Path Check
            var rimworldRootDir = Directory.GetParent(setting.GameModDirPath);
            if (!File.Exists(rimworldRootDir + "\\RimWorldWin64.exe"))
            {
                Console.WriteLine($"Path seems error: {setting.GameModDirPath}");
                return false;
            }

            if (!File.Exists(setting.ModConfigXmlPath + "\\ModsConfig.xml"))
            {
                Console.WriteLine($"Path seems error: {setting.ModConfigXmlPath}");
                return false;
            }
            return true;
        }
    }
}
