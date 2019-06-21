using System;
using System.Collections.Generic;
using System.Windows;

namespace Multimedia_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;

            _playList = new List<PlayList>();
        }

        public class PlayList
        {
            public string Status { get; set; }
            public string Name { get; set; }
            public string Path { get; set; }
        }

        // List save opened files
        private List<PlayList> _playList = null;

        // Save index of file is playing
        private int _currentFileIndex = 0;

        // Dragging slider
        private bool _isDragging = false;

        // Status play or pause file
        private bool _playing = false;

        private bool playing
        {
            get => _playing;
            set
            {
                _playing = value;

                if (_playing)
                    Play_Btn.Content = "Pause";
                else
                    Play_Btn.Content = "Play";
            }
        }

        // Status random or queue
        private bool _random = false;

        private bool random
        {
            get => _random;
            set
            {
                _random = value;

                if (_random)
                    Random.IsChecked = true;
                else
                    Random.IsChecked = false;
            }
        }

        // Status repeat
        private bool _repeat = false;

        private bool repeat
        {
            get => _repeat;
            set
            {
                _repeat = value;

                if (_repeat)
                    Repeat.IsChecked = true;
                else
                    Repeat.IsChecked = false;
            }
        }

        System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();

        void timer_Tick(object sender, EventArgs e)
        {
            if (!_isDragging) { Seek_Bar.Value = Media_Player.Position.TotalSeconds; }

            Time_Status.Text = String.Format($"{Media_Player.Position.ToString(@"mm\:ss")} / {Media_Player.NaturalDuration.TimeSpan.ToString(@"mm\:ss")}");
        }

        private void Play_Media(string path)
        {
            Media_Player.Source = new Uri(path);
            Media_Player.Play();
            Current_File_Name.Text = System.IO.Path.GetFileNameWithoutExtension(path);
            playing = true;
            Seek_Bar.Value = 0;
        }

        private void Media_Opened(object sender, RoutedEventArgs e)
        {
            Seek_Bar.IsEnabled = true;
            Seek_Bar.Value = 0;
            Seek_Bar.Maximum = Media_Player.NaturalDuration.TimeSpan.TotalSeconds;

            timer.Start();
        }

        private void Media_Ended(object sender, RoutedEventArgs e)
        {
            if (repeat)
            {
                Media_Player.Position = TimeSpan.Zero;
                Media_Player.Play();
                Seek_Bar.Value = 0;
            }
            else if (random)
            {

            }
            else
            {
                Next_Btn_Click(this, new RoutedEventArgs());
            }
        }

        private void Seek_Bar_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _isDragging = true;
        }

        private void Seek_Bar_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _isDragging = false;
            Media_Player.Position = TimeSpan.FromSeconds(Seek_Bar.Value);
            timer_Tick(this, new EventArgs());
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Video files (*.mp4)|*.mp4|Audio files (*.mp3)|*.mp3|All files (*.*)|*.*",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                var paths = dialog.FileNames;

                foreach (string path in paths)
                {
                    _playList.Add(new PlayList() { Name = System.IO.Path.GetFileNameWithoutExtension(path), Path = path });
                }

                if (_playList.Count == 1) { Play_Media(_playList[0].Path); }
            }
        }

        private void Play_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (playing)
            {
                Media_Player.Pause();
                playing = false;
            }
            else
            {
                Media_Player.Play();
                playing = true;
                timer.Start();
                Seek_Bar.IsEnabled = true;
            }
        }

        private void Stop_Btn_Click(object sender, RoutedEventArgs e)
        {
            Media_Player.Position = TimeSpan.Zero;
            Media_Player.Stop();
            playing = false;
            timer.Stop();
            Seek_Bar.Value = 0;
            Seek_Bar.IsEnabled = false;
        }

        private void About_Btn_Click(object sender, RoutedEventArgs e)
        {
            var screen = new AboutWindow();
            screen.ShowDialog();
        }

        private void Prev_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentFileIndex > 0)
            {
                _currentFileIndex--;
                Play_Media(_playList[_currentFileIndex].Path);
            }
            else
            {
                MessageBox.Show("You are playing the first file");
            }
        }

        private void Next_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentFileIndex < _playList.Count - 1)
            {
                _currentFileIndex++;
                Play_Media(_playList[_currentFileIndex].Path);
            }
            else
            {
                MessageBox.Show("You are playing the last file");
            }
        }

        private void Random_Btn_Click(object sender, RoutedEventArgs e)
        {
            random = !random;
        }

        private void Repeat_Btn_Click(object sender, RoutedEventArgs e)
        {
            repeat = !repeat;
        }

        private void Homepage_Btn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/bphamict");
        }

        private void Playlist_Btn_Click(object sender, RoutedEventArgs e)
        {
            var screen = new PlaylistWindow(_playList);
            screen.ShowDialog();
        }
    }
}
