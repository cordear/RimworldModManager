using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimworldModManager
{
    class ModInfo
    {
        public string PackageId { set; get; }
        public string Id { set; get; }
        public string Name { set; get; }
        public DateTime CreateTime { set; get; }
        public bool IsActive { set; get; }
        public ModInfo(string packageId, string id, string name,DateTime createTime,bool isActive=false)
        {
            PackageId = packageId;
            Id = id;
            Name = name;
            CreateTime = createTime;
            IsActive = isActive;
        }

    }
}
