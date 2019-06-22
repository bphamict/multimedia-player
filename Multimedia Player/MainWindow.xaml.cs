using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using MessageBox = System.Windows.MessageBox;

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

        private IKeyboardMouseEvents m_GlobalHook;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;

            _playList = new List<PlayList>();

            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.KeyUp += _hook_KeyUp;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            m_GlobalHook.KeyUp -= _hook_KeyUp;
            m_GlobalHook.Dispose();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void _hook_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && (e.KeyCode == Keys.D1))    // random
            {
                Random_Btn_Click(this, new RoutedEventArgs());
            }
            else if (e.Control && (e.KeyCode == Keys.D2))   // prev
            {
                Prev_Btn_Click(this, new RoutedEventArgs());
            }
            else if (e.Control && (e.KeyCode == Keys.D3))   // play/pause
            {
                Play_Btn_Click(this, new RoutedEventArgs());
            }
            else if (e.Control && (e.KeyCode == Keys.D4))   // next
            {
                Next_Btn_Click(this, new RoutedEventArgs());
            }
            else if (e.Control && (e.KeyCode == Keys.D5))   // stop
            {
                Stop_Btn_Click(this, new RoutedEventArgs());
            }
            else if (e.Control && (e.KeyCode == Keys.D6))   // repeat
            {
                Repeat_Btn_Click(this, new RoutedEventArgs());
            }
            else if (e.Control && (e.KeyCode == Keys.O))    // open
            {
                Open_Click(this, new RoutedEventArgs());
            }
            else if (e.Control && (e.KeyCode == Keys.E))    // exit
            {
                Exit_Click(this, new RoutedEventArgs());
            }
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

        private Random rng = new Random();

        private bool playing
        {
            get => _playing;
            set
            {
                _playing = value;

                if (_playing)
                    Play_Image.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(@"Resources/pause.png", UriKind.Relative));
                else
                    Play_Image.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(@"Resources/play.png", UriKind.Relative));
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
            Media_Player.Position = TimeSpan.Zero;
            Media_Player.Play();

            timer.Stop();
            timer.Start();
            Seek_Bar.IsEnabled = true;
            Seek_Bar.Value = 0;

            playing = true;

            Current_File_Name.Text = System.IO.Path.GetFileNameWithoutExtension(path);
        }

        private void Media_Opened(object sender, RoutedEventArgs e)
        {
            Seek_Bar.Maximum = Media_Player.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void Media_Ended(object sender, RoutedEventArgs e)
        {
            Next_Btn_Click(this, new RoutedEventArgs());
            timer.Stop();
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
                    _playList.Add(new PlayList() { Status = "", Name = System.IO.Path.GetFileNameWithoutExtension(path), Path = path });
                }

                Play_Media(_playList[0].Path);

                Prev_Btn.IsEnabled = false;
                Prev_Btn_Menu.IsEnabled = false;

                Play_Btn.IsEnabled = true;
                Play_Btn_Menu.IsEnabled = true;

                Stop_Btn.IsEnabled = true;
                Stop_Btn_Menu.IsEnabled = true;

                Playlist_Btn.IsEnabled = true;
                Playlist_Btn_Menu.IsEnabled = true;

                if (_playList.Count == 1) { Next_Btn.IsEnabled = false; Next_Btn_Menu.IsEnabled = false; }
                else { Next_Btn.IsEnabled = true; Next_Btn_Menu.IsEnabled = true; }

            }
        }

        private void Play_Btn_Click(object sender, RoutedEventArgs e)
        {
            Seek_Bar.IsEnabled = true;

            if (playing)
            {
                Media_Player.Pause();
                playing = false;
                timer.Stop();
            }
            else
            {
                Media_Player.Play();
                playing = true;
                timer.Start();
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
            if (repeat)
            {
                Media_Player.Position = TimeSpan.Zero;
                Media_Player.Play();
                Seek_Bar.Value = 0;
            }
            else if (random)
            {
                _currentFileIndex = rng.Next(0, _playList.Count);

                Play_Media(_playList[_currentFileIndex].Path);
            }
            else
            {
                if (_currentFileIndex > 0)
                {
                    _currentFileIndex--;
                    Play_Media(_playList[_currentFileIndex].Path);

                    Next_Btn.IsEnabled = true;
                    Next_Btn_Menu.IsEnabled = true;

                    if (_currentFileIndex == 0) { Prev_Btn.IsEnabled = false; Prev_Btn_Menu.IsEnabled = false; }
                }
            }
        }

        private void Next_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (repeat)
            {
                Media_Player.Position = TimeSpan.Zero;
                Media_Player.Play();
                Seek_Bar.Value = 0;
            }
            else if (random)
            {
                _currentFileIndex = rng.Next(0, _playList.Count);

                Play_Media(_playList[_currentFileIndex].Path);
            }
            else
            {
                if (_currentFileIndex < _playList.Count - 1)
                {
                    _currentFileIndex++;
                    Play_Media(_playList[_currentFileIndex].Path);

                    Prev_Btn.IsEnabled = true;
                    Prev_Btn_Menu.IsEnabled = true;

                    if (_currentFileIndex == _playList.Count - 1) { Next_Btn.IsEnabled = false; Next_Btn_Menu.IsEnabled = false; }
                }
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

        private void Playlist_Btn_Click(object sender, RoutedEventArgs e)
        {
            _playList[_currentFileIndex].Status = "playing";

            var screen = new PlaylistWindow(_playList);

            if (screen.ShowDialog() == true)
            {
                for (int i = 0; i < _playList.Count; i++)
                {
                    if (_playList[i].Status == "playing")
                    {
                        _currentFileIndex = i;
                        _playList[i].Status = "";
                        Play_Media(_playList[_currentFileIndex].Path);

                        if (_playList.Count == 1) { Next_Btn.IsEnabled = false; Next_Btn_Menu.IsEnabled = false; }
                        else { Next_Btn.IsEnabled = true; Next_Btn_Menu.IsEnabled = true; }
                    }
                }
            }

            _playList[_currentFileIndex].Status = "";
        }

        private void Homepage_Btn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/bphamict/multimedia-player");
        }
    }
}
