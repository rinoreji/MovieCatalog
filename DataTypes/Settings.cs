using System;
using System.Collections.Generic;

namespace MovieCatalog.DataTypes
{
    [Serializable]
    public class Settings
    {
        public List<GenericKeyValuePair<string, bool>> MovieScanPaths { get; set; }
    }
}
