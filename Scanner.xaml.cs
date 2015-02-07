using MovieCatalog.DataTypes;
using MovieCatalog.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Forms = System.Windows.Forms;

namespace MovieCatalog
{
    public partial class Scanner : Window, INotifyPropertyChanged
    {
        Settings _settings { get; set; }
        bool changesMade = false;//TODO set this to true for the close warning

        public Scanner()
        {
            InitializeComponent();
            InitData();
            this.DataContext = this;
        }

        public ICommand CloseCommand
        {
            get
            {
                return new DelegateCommand(w =>
                {
                    var window = w as Window;
                    if (window != null)
                        if (changesMade)
                        {
                            if (MessageBox.Show("Exit without saving the changed?", "Confirm close", MessageBoxButton.YesNo) == MessageBoxResult.OK)
                            {
                                window.Close();
                            }
                        }
                        else
                            window.Close();
                });
            }
        }

        private ObservableCollection<GenericKeyValuePair<string, bool>> _movieScanPaths;
        public ObservableCollection<GenericKeyValuePair<string, bool>> MovieScanPaths
        {
            get { return _movieScanPaths; }
            set { _movieScanPaths = value; }
        }

        public List<MovieData> PotentialMoviePaths { get; set; }


        void InitData()
        {
            _settings = SettingsHelper.LoadSettings();

            MovieScanPaths = new ObservableCollection<GenericKeyValuePair<string, bool>>(_settings.MovieScanPaths);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaizePropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, args);
        }

        string _lastPath = string.Empty;
        private void OnSelectPath(object sender, RoutedEventArgs e)
        {
            var dialog = new Forms.FolderBrowserDialog();
            dialog.RootFolder = System.Environment.SpecialFolder.MyComputer;

            if (_lastPath != string.Empty) dialog.SelectedPath = _lastPath;

            var result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                var path = dialog.SelectedPath;
                _lastPath = path;
                var existingRecord = MovieScanPaths.FirstOrDefault(p => p.Key == path);
                if (existingRecord != null)
                    MovieScanPaths.Remove(existingRecord);

                MovieScanPaths.Add(new GenericKeyValuePair<string, bool>(path, true));
            }
        }

        public string ProgressMessage { get; set; }

        private async void OnScan(object sender, RoutedEventArgs e)
        {
            var progressIndicator = new Progress<MovieTaskProgress>((p) =>
            {
                ProgressMessage = string.Format("{0}/{1} - {2}", p.Current, p.Total, p.Message);
                RaizePropertyChanged("ProgressMessage");
            });

            var rootPaths = MovieScanPaths.Where(m => m.Value).Select(m => m.Key);

            PotentialMoviePaths = await Task.Run<List<MovieData>>(() =>
            {
                return MovieFactory.GetMoviesData(rootPaths.ToList(), progressIndicator);
            });

            PotentialMoviePaths.Sort(new MovieNameComparer());
            RaizePropertyChanged("PotentialMoviePaths");
        }

        private void OnSaveSettings(object sender, RoutedEventArgs e)
        {
            _settings.MovieScanPaths = MovieScanPaths.ToList();
            SettingsHelper.SaveSettings(_settings);
        }
    }

    public class MovieNameComparer : IComparer<MovieData>
    {
        public int Compare(MovieData x, MovieData y)
        {
            return string.Compare(x.ExtractedName, y.ExtractedName);
        }
    }
}
