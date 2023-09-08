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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StreamsFiles
{
    /// <summary>
    /// Logique d'interaction pour Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        public string websocketUrlSetting { get; set; }
        public string apiUrlSetting { get; set; }

        public Settings(string currentWebsocketUrl, string currentApiUrl)
        {
            InitializeComponent();
            websocketUrl.Text = currentWebsocketUrl;
            apiUrl.Text = currentApiUrl;
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Récupérez la nouvelle valeur de l'URL de la vidéo depuis le champ de texte
            websocketUrlSetting = websocketUrl.Text;
            apiUrlSetting = apiUrl.Text;
            // Fermez la fenêtre de paramètres
            Window.GetWindow(this).DialogResult = true;
            Window.GetWindow(this).Close();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Fermez la fenêtre de paramètres sans enregistrer les modifications
            Window.GetWindow(this).DialogResult = false;
            Window.GetWindow(this).Close();
        }
    }
}
