using MovieCatalog.DataTypes;
using System;
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

        public static List<MovieData> GetMoviesData(List<string> rootPaths)
        {
            var data = new List<MovieData>();
            foreach (var path in GetPotentialFilmPaths(rootPaths))
            {
                var movie = new MovieData { FullPath = path };
                movie.ExtractedName = new DirectoryInfo(path).Name.GetSanitizeName();

                data.Add(MovieHelper.SetMovieUrl(movie));
            }

            return data;
        }

        public static List<MovieData> GetMoviesData(List<string> rootPaths, IProgress<MovieTaskProgress> progress)
        {
            var data = new List<MovieData>();
            var moviePaths = GetPotentialFilmPaths(rootPaths);

            foreach (var path in moviePaths)
            {
                var movie = new MovieData { FullPath = path };
                movie.ExtractedName = new DirectoryInfo(path).Name.GetSanitizeName();
                data.Add(movie);
            }

            UpdateMovieData(data, progress);

            return data;
        }

        private static void UpdateMovieData(List<MovieData> movies, IProgress<MovieTaskProgress> progress)
        {
            int processedCount = 0;
            Parallel.ForEach<MovieData>(movies, (m) =>
            {
                MovieHelper.SetMovieUrl(m);
                MovieHelper.SetMovieDetails(m);

                MovieTaskProgress taskProgress = new MovieTaskProgress();
                taskProgress.Total = movies.Count;
                taskProgress.Message = string.Format("Processed movie: {0}", m.ExtractedName);
                taskProgress.Current = ++processedCount;

                progress.Report(taskProgress);
            });
        }
    }
}