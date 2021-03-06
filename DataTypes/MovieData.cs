﻿
using System;
using System.Collections.Generic;
using System.IO;

namespace MovieCatalog.DataTypes
{
    [Serializable]
    public class MovieData : UIDataBase
    {
        public MediaPathInfo PathInfo { get; set; }

        private string _extractedName;
        public string ExtractedName //Sanitized data
        {
            get { return _extractedName; }
            set { _extractedName = value; }
        }

        public int MatchRank { get; set; }
        public string Exception { get; set; }
        public string ImdbUrl { get; set; }

        private string _imdbName;

        public string ImdbName
        {
            get { return _imdbName; }
            set { _imdbName = value; }
        }

        public string Rating { get; set; }
        public string StoryLine { get; set; }
        public string ImageUrl { get; set; }
        public string Year { get; set; }
        public List<string> Genres { get; set; }

        public MovieData()
        {
            PathInfo = new MediaPathInfo();
        }
    }
}
