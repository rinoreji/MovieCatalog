using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MovieCatalog.DataTypes
{
    [Serializable]
    public class MediaPathInfo
    {
        public string MachineName { get; set; }
        public DriveType DriveType { get; set; }
        public string DriveFormat { get; set; }
        public string Name { get; set; }
        public string VolumnLabel { get; set; }

        public string FullPath { get; set; }
        public long Size { get; set; }
    }
}
