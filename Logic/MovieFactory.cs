using MovieCatalog.Algorithms;
using MovieCatalog.DataTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MovieCatalog.Logic
{
    public class MovieFactory
    {
        private static List<string> GetPotentialFilmPaths(string rootPath)
        {
            var childDirectories = new List<string>();
            var rootDirectory = new DirectoryInfo(rootPath);
            if (rootDirectory.Exists)
            {
                childDirectories = rootDirectory.EnumerateDirectories().Select(d => d.FullName).ToList();
            }

            return childDirectories;
        }

        private static List<string> GetPotentialFilmPaths(List<string> rootPaths)
        {
            var childDirectories = new List<string>();
            foreach (var path in rootPaths)
            {
                var potentialPaths = GetPotentialFilmPaths(path);

                potentialPaths.RemoveAll(p => rootPaths.Contains(p));
                childDirectories.AddRange(potentialPaths);
            }

            return childDirectories;
        }

        public static List<MovieData> GetMoviesData(List<string> rootPaths, IProgress<MovieTaskProgress> progress)
        {
            var data = new List<MovieData>();
            var moviePaths = GetPotentialFilmPaths(rootPaths);

            foreach (var path in moviePaths)
            {
                var movie = new MovieData();
                movie.PathInfo = GetPathInfo(path);
                movie.ExtractedName = new DirectoryInfo(path).Name.GetSanitizeName();
                data.Add(movie);
            }

            //UpdateMovieData(data, progress);

            return data;
        }

        private static MediaPathInfo GetPathInfo(string folderFullPath)
        {
            var pathInfo = new MediaPathInfo();
            pathInfo.FullPath = folderFullPath;
            Helpers.TryCatch(() =>
            {
                var driveInfo = new DriveInfo(folderFullPath);
                pathInfo.MachineName = Environment.MachineName;
                pathInfo.DriveFormat = driveInfo.DriveFormat;
                pathInfo.DriveType = driveInfo.DriveType;
                pathInfo.Name = driveInfo.Name;
                pathInfo.VolumnLabel = driveInfo.VolumeLabel;
            });
            return pathInfo;
        }

        private static void UpdateMovieData(List<MovieData> movies, IProgress<MovieTaskProgress> progress)
        {
            var items = new ConcurrentBag<string>();
            Parallel.ForEach<MovieData>(movies, (m) =>
            {
                MovieTaskProgress taskProgress = new MovieTaskProgress();
                taskProgress.Total = movies.Count;
                taskProgress.Message = string.Format("Processing movie: {0}", m.ExtractedName);
                Helpers.TryCatch(() => MovieHelper.SetMovieUrl(m));
                if (items.FirstOrDefault((i) => i == m.PathInfo.FullPath).IsNull())
                {
                    items.Add(m.PathInfo.FullPath);
                }
                taskProgress.Current = items.Count;
                progress.Report(taskProgress);
                Helpers.TryCatch(() => MovieHelper.SetMovieDetails(m));

                taskProgress.Message = string.Format("Processed movie: {0}", m.ExtractedName);

                progress.Report(taskProgress);
            });
        }

        public static List<List<MovieDuplicateInfo>> PotentialDuplicates(List<MovieData> input)
        {
            var data = new List<List<MovieDuplicateInfo>>();
            for (int i = 0; i < input.Count; i++)
            {
                var currentItem = input[i];
                var duplicates = new List<MovieDuplicateInfo>();

                if (!data.Any(d => d.Any(dd => dd.Movie.PathInfo.FullPath == currentItem.PathInfo.FullPath)))
                {
                    for (int j = i + 1; j < input.Count; j++)
                    {
                        duplicates.Add(new MovieDuplicateInfo()
                        {
                            Movie = input[j],
                            ChangeCost = LevenshteinDistance.Compute(currentItem.ExtractedName, input[j].ExtractedName)
                        });
                    }
                    duplicates.Insert(0, new MovieDuplicateInfo() { Movie = currentItem, ChangeCost = 0 });
                    duplicates.RemoveAll(d => d.ChangeCost > 4);
                    if (duplicates.Count > 1)
                        data.Add(duplicates);
                }
            }

            return data;
        }
    }
}