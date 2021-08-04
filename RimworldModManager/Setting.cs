using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimworldModManager
{
    class Setting
    {
        private string _modConfigXmlPath;
        private string _modTempDirPath;
        private string _gameModDirPath;

        public string GameModDirPath
        {
            set => _gameModDirPath = value;
            get
            {
                if (!string.IsNullOrEmpty(_gameModDirPath)) return _gameModDirPath;
                var modDirPath = Directory.GetCurrentDirectory();
                modDirPath += "\\Mods";
                return modDirPath;
            }
        }

        public string ModConfigXmlPath
        {
            set => _modConfigXmlPath = value;
            get
            {
                if (!string.IsNullOrEmpty(_modConfigXmlPath)) return _modConfigXmlPath;
                var configPath =
                    Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
                        .ToString();
                configPath += "\\LocalLow\\Ludeon Studios\\RimWorld by Ludeon Studios\\Config";
                return configPath;

            }
        }

        public string ModTempDirPath
        {
            set => _modTempDirPath = value;
            get
            {
                if (!string.IsNullOrEmpty(_modTempDirPath)) return _modTempDirPath;
                var tempDirPath = Directory.GetCurrentDirectory();
                tempDirPath += "\\RimworldModDownloadTemp";
                return tempDirPath;
            }
        }
    }
}
