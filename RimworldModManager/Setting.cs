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
        private string modConfigXmlPath;
        public string GameModDirPath { set; get; }

        public string ModConfigXmlPath
        {
            set => modConfigXmlPath = value;
            get
            {
                if (!string.IsNullOrEmpty(modConfigXmlPath)) return modConfigXmlPath;
                var configPath =
                    Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
                        .ToString();
                configPath += "\\LocalLow\\Ludeon Studios\\RimWorld by Ludeon Studios\\Config";
                return configPath;

            }
        }
    }
}
