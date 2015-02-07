using System;
using System.Collections.Generic;

namespace MovieCatalog.DataTypes
{
    [Serializable]
    public class MovieCatalog
    {
        public List<MovieData> Movies { get; set; }

        public MovieCatalog()
        {
            Movies = new List<MovieData>();
        }
    }
}
