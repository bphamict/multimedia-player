using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Multimedia_Player
{
    /// <summary>
    /// Interaction logic for PlaylistWindow.xaml
    /// </summary>
    public partial class PlaylistWindow : Window
    {
        private List<MainWindow.PlayList> PlayList;

        public PlaylistWindow(List<MainWindow.PlayList> playList)
        {
            InitializeComponent();

            this.PlayList = playList;

            Playlist_DataGrid.ItemsSource = PlayList;
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
                    PlayList.Add(new MainWindow.PlayList() { Name = System.IO.Path.GetFileNameWithoutExtension(path), Path = path });
                }

                Playlist_DataGrid.Items.Refresh();
            }
        }

        // Path of product file
        private string path = AppDomain.CurrentDomain.BaseDirectory + "/playlist.xml";

        private void Export_Btn_Click(object sender, RoutedEventArgs e)
        {
            XElement doc = new XElement("tracks",
                from el in PlayList
                select
                new XElement("track",
                new XElement("status", el.Status),
                new XElement("name", el.Name),
                new XElement("path", el.Path)
                ));

            doc.Save(path);
        }

        private void Import_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Load XML
            XDocument doc;
            try { doc = XDocument.Load(path); }
            catch (Exception el) { MessageBox.Show(el.Message); return; }

            // Read XML
            var track = from el in doc.Descendants("track")
                        select new
                        {
                            status = el.Element("status").Value,
                            name = el.Element("name").Value,
                            path = el.Element("path").Value
                        };

            // Format & add product to list
            foreach (var el in track)
            {
                PlayList.Add(new MainWindow.PlayList() { Status = el.status, Name = el.name, Path = el.path });
            }

            Playlist_DataGrid.Items.Refresh();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            PlayList.Remove((MainWindow.PlayList)Playlist_DataGrid.SelectedItem);

            Playlist_DataGrid.Items.Refresh();
        }

        private void DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            PlayList.Clear();

            Playlist_DataGrid.Items.Refresh();
        }
    }
}
