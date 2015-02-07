
using System;
using System.Collections.Generic;
namespace MovieCatalog.DataTypes
{
    [Serializable]
    public class MovieData
    {
        public string FullPath { get; set; }

        public string ExtractedName { get; set; }//Sanitized data
        public int MatchRank { get; set; }
        public string Exception { get; set; }
        public string ImdbUrl { get; set; }

        public string ImdbName { get; set; }
        public string Rating { get; set; }
        public string StoryLine { get; set; }
        public string ImageUrl { get; set; }
        public string Year { get; set; }
        public List<string> Genres { get; set; }
    }
}
