using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace Multimedia_Player
{
    /// <summary>
    /// Interaction logic for PlaylistWindow.xaml
    /// </summary>
    public partial class PlaylistWindow : Window
    {
        private List<MainWindow.PlayList> _playList;

        public PlaylistWindow(List<MainWindow.PlayList> playList)
        {
            InitializeComponent();

            this._playList = playList;

            Playlist_DataGrid.ItemsSource = _playList;
        }

        private void Add_Btn_Click(object sender, RoutedEventArgs e)
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
                    _playList.Add(new MainWindow.PlayList() { Name = System.IO.Path.GetFileNameWithoutExtension(path), Path = path });
                }

                Playlist_DataGrid.Items.Refresh();

                _playList[0].Status = "playing";
                this.DialogResult = true;
            }
        }

        private string path = AppDomain.CurrentDomain.BaseDirectory + "/playlist.xml";

        private void Export_Btn_Click(object sender, RoutedEventArgs e)
        {
            XElement doc = new XElement("tracks",
                from el in _playList
                select
                new XElement("entry",
                new XAttribute("status", el.Status),
                new XAttribute("name", el.Name),
                new XAttribute("path", el.Path)
                ));

            doc.Save(path);
        }

        private void Import_Btn_Click(object sender, RoutedEventArgs e)
        {
            _playList.Clear();

            XDocument doc;
            try { doc = XDocument.Load(path); }
            catch (Exception el) { MessageBox.Show(el.Message); return; }

            var track = from el in doc.Descendants("entry")
                        select new
                        {
                            status = el.Attribute("status").Value,
                            name = el.Attribute("name").Value,
                            path = el.Attribute("path").Value
                        };

            foreach (var el in track)
            {
                _playList.Add(new MainWindow.PlayList() { Status = el.status, Name = el.name, Path = el.path });
            }

            Playlist_DataGrid.Items.Refresh();

            this.DialogResult = true;
            this.Close();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Playlist_DataGrid.SelectedItems.Count; i++)
            {
                if (((MainWindow.PlayList)Playlist_DataGrid.SelectedItems[i]).Status == "playing") { MessageBox.Show("File is playing, can't delete!"); return; }

                _playList.Remove((MainWindow.PlayList)Playlist_DataGrid.SelectedItems[i]);
            }

            Playlist_DataGrid.Items.Refresh();
        }

        private void DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < _playList.Count; i++)
            {
                if (_playList[i].Status != "playing") { _playList.Remove(_playList[i]); i--; }
            }

            Playlist_DataGrid.Items.Refresh();
        }
    }
}
