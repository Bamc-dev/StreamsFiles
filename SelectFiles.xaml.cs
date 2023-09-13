using StreamsFiles.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StreamsFiles
{
    /// <summary>
    /// Logique d'interaction pour SelectFiles.xaml
    /// </summary>
    public partial class SelectFiles : UserControl
    {
        public List<File> Files { get; set; }
        public File SelectedFile { get; set; }
        private string ApiUrl;

        public SelectFiles(string apiUrl)
        {
            InitializeComponent();
            ApiUrl = apiUrl;
            LoadDataFromApi();
        }
        private async void LoadDataFromApi()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Remplacez l'URL par l'URL réelle de votre API
                    
                    string response = await client.GetStringAsync(ApiUrl+"/allFiles");

                    // Désérialisez les données JSON en liste de personnes
                    Files = Newtonsoft.Json.JsonConvert.DeserializeObject<List<File>>(response);

                    // Définissez le contexte de données de la fenêtre sur la liste de personnes
                    DataContext = this;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Une erreur s'est produite : " + ex.Message);
            }
        }
        private void Selected_Click(object sender, RoutedEventArgs e)
        {
            SelectedFile = (File)fileListBox.SelectedItem;
            // Fermez la fenêtre de paramètres
            Window.GetWindow(this).DialogResult = true;
            Window.GetWindow(this).Close();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            SelectedFile = (File)fileListBox.SelectedItem;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Remplacez l'URL par l'URL réelle de votre API

                    client.DeleteAsync(ApiUrl + "/deleteFile/"+SelectedFile.code);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Une erreur s'est produite : " + ex.Message);
            }
            Files.Remove(SelectedFile);
            fileListBox.Items.Refresh();
            SelectedFile = null;
        }
    }
}
