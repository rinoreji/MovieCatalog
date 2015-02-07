using HtmlAgilityPack;
using MovieCatalog.DataTypes;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;

namespace MovieCatalog.Logic
{
    public static class MovieHelper
    {
        private static List<string> _blackListedWords = new List<string>
        {
            "DVDRip","XviD","BRRip","720p", "x264","1080p","DvDScr",
            "\\[",
            "\\(\\d{4}\\)"
        };

        static string regexPattern = string.Format("({0})", string.Join("|", _blackListedWords));

        public static string GetSanitizeName(this string rawName)
        {
            var sanitizedData = rawName.Normalize().Replace('.', ' ');// normalized and replaced . with <space>

            RegexOptions regexOptions = RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace;
            Regex regex = new Regex(regexPattern, regexOptions);
            var match = regex.Match(sanitizedData);

            if (match.Success)
                sanitizedData = sanitizedData.Substring(0, match.Index);

            return sanitizedData.Trim();
        }

        public static MovieData SetMovieUrl(MovieData movieData)
        {
            var queryUrl = GetGoogleSearchUrl(movieData.ExtractedName);

            var queryHtmlResult = GetHtmlPageData(queryUrl);

            var imdbPattern = @">(www\.imdb\.com\/title\/.+?\/)<\/";
            //TODO:more pattern to extract from other sources and rank sources. may be IMdb as rank 1

            Regex regex = new Regex(imdbPattern);
            var match = regex.Match(queryHtmlResult);
            if (match.Success)
            {
                var grp = match.Groups;
                movieData.ImdbUrl = grp[1].Value;
            }

            return movieData;
        }

        public static MovieData SetMovieDetails(MovieData movieData)
        {
            if (movieData.ImdbUrl.IsNotNullOrWhiteSpace())
            {
                var queryHtmlResult = GetHtmlPageData(string.Format("http://{0}", movieData.ImdbUrl));
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(queryHtmlResult);

                movieData.ImdbName = doc.DocumentNode.SelectSingleNode(@"//span[@itemprop='name']").InnerText.Trim();
                movieData.Rating = doc.DocumentNode.SelectSingleNode(@"//span[@itemprop='ratingValue']").InnerText.Trim();
                movieData.ImageUrl = doc.DocumentNode.SelectSingleNode(@"//img[@itemprop='image']").Attributes["src"].Value.Trim();
                movieData.Year = doc.DocumentNode.SelectSingleNode(@"//h1[@class='header']/span[@class='nobr']/a").InnerText.Trim();

                doc.DocumentNode.SelectSingleNode(@"//div[@itemprop='description']/p/em").Remove();
                movieData.StoryLine = doc.DocumentNode.SelectSingleNode(@"//div[@itemprop='description']/p").InnerText.Trim();

                var genreNodes = doc.DocumentNode.SelectNodes(@"//div[@itemprop='genre']/a");
                movieData.Genres = new List<string>();
                foreach (var node in genreNodes)
                {
                    movieData.Genres.Add(node.InnerText.Trim());
                }
            }

            return movieData;
        }

        private static string GetGoogleSearchUrl(string searchText)
        {
            return string.Format("https://www.google.co.in/search?ie=UTF-8&q=film+%2B+{0}",
                searchText.Replace(' ', '+'));
        }

        private static string GetHtmlPageData(string queryUrl)
        {
            WebClient webClient = new WebClient();
            webClient.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            webClient.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-GB,en-US;q=0.8,en;q=0.6");
            webClient.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 3.5;)");

            var memStream = new MemoryStream(webClient.DownloadData(queryUrl));

            var pageHtml = string.Empty;

            if (webClient.ResponseHeaders[HttpResponseHeader.ContentEncoding] == "gzip")
                pageHtml = new StreamReader(new GZipStream(memStream, CompressionMode.Decompress)).ReadToEnd();
            else
                pageHtml = new StreamReader(memStream).ReadToEnd();

            return pageHtml;
        }
    }
}
