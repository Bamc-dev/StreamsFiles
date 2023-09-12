using LibVLCSharp.Shared;
using LibVLCSharp.WPF;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
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
using System.Windows.Threading;
using System.Xml.Linq;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;
using Settings = StreamsFiles.Settings;
using System.Net.Http;
using Path = System.IO.Path;
using System.Diagnostics;
using System.IO.Pipes;

namespace StreamsFiles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LibVLC libVLC;
        private MediaPlayer mediaPlayer;
        private Media media;
        private DispatcherTimer _timer;
        private List<TrackInformation> subtitleTracks;
        private List<TrackInformation> audioTracks;
        private AppSetting settings;



        private bool isSubtitlesLoaded = false;
        private bool isFullscreen = false;
        private int originalColumnSpan;
        private int originalRowSpan;
        public MainWindow()
        {
            InitializeComponent();
            Core.Initialize();
            settings = new AppSetting("", "");
            LoadConfiguration();
            libVLC = new LibVLC("--no-xlib");
            mediaPlayer = new MediaPlayer(libVLC);
            videoView.MediaPlayer = mediaPlayer;
            volumeSlider.Value = mediaPlayer.Volume;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            mediaPlayer.Playing += MediaPlayer_LoadTracks;
            subtitleTracks = new List<TrackInformation>();
            audioTracks = new List<TrackInformation>();

        }
        #region CONTROLS
        private void Fullscreen(object sender, RoutedEventArgs e)
        {
            if (!isFullscreen)
            {
                // Activer le mode plein écran
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                ResizeMode = ResizeMode.NoResize;
                isFullscreen = true;
                // Conservez les valeurs d'origine de RowSpan et ColumnSpan
                originalColumnSpan = Grid.GetColumnSpan(videoView);
                originalRowSpan = Grid.GetRowSpan(videoView);
                Grid.SetColumnSpan(videoView, videoGrid.ColumnDefinitions.Count);
                Grid.SetRowSpan(videoView, videoGrid.RowDefinitions.Count);

            }
            else
            {
                // Désactiver le mode plein écran
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
                ResizeMode = ResizeMode.CanResize;
                isFullscreen = false;

                // Restaurez les valeurs d'origine de RowSpan et ColumnSpan
                Grid.SetColumnSpan(videoView, originalColumnSpan);
                Grid.SetRowSpan(videoView, originalRowSpan);

            }
        }
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            // Code pour démarrer la lecture de la vidéo
            if (mediaPlayer.Media != null)
            {
                if (mediaPlayer.IsPlaying)
                {
                    _timer.Stop();
                    mediaPlayer.Pause();
                }
                else
                {
                    _timer.Start();
                    mediaPlayer.Pause();
                }
            }
            else
            {
                _timer.Start();
                mediaPlayer.Play(media);
            }


        }
        private void SeekForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.IsPlaying)
            {
                // Avancez de 5 secondes dans la vidéo
                long currentPosition = mediaPlayer.Time;
                long newPosition = currentPosition + (5000); // 5 secondes en microsecondes
                                                             // Vérifiez si la nouvelle position est inférieure à la durée totale de la vidéo
                if (newPosition < mediaPlayer.Media.Duration)
                {
                    mediaPlayer.Time = newPosition;
                }
                else
                {
                    // Assurez-vous de ne pas dépasser la durée totale de la vidéo
                    mediaPlayer.Time = mediaPlayer.Media.Duration;
                }
            }

        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            // Code pour arrêter la vidéo
            mediaPlayer.Stop();
        }
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Réglez le volume du lecteur vidéo en fonction de la valeur du Slider
            mediaPlayer.Volume = (int)volumeSlider.Value;
        }
        private void TimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Réglez la position de la vidéo en fonction de la valeur du Slider

            mediaPlayer.Time = (long)timeSlider.Value * 1000;
        }
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // Vérifiez si vous êtes en mode plein écran
                if (isFullscreen)
                {
                    // Désactivez le mode plein écran
                    Fullscreen(null, null);
                }
            }
        }

        private void OpenFile_Button(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true; // Permet de sélectionner plusieurs fichiers

            // Définir les filtres de fichier (facultatif)
            openFileDialog.Filter = "Fichiers MKV (*.mkv)|*.mkv|Tous les fichiers (*.*)|*.*";

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                // Obtenez les chemins des fichiers sélectionnés
                string[] fichiersSelectionnes = openFileDialog.FileNames;

                // Traitez les fichiers sélectionnés ici
                foreach (string fichier in fichiersSelectionnes)
                {
                   UploadFileInChunks(fichier);
                }
 
            }


        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.IsPlaying)
            {
                long totalTime = mediaPlayer.Length;
                long currentTime = mediaPlayer.Time;
                TimeSpan timeSpan = TimeSpan.FromMilliseconds(media.Duration);
                TimeSpan actualTime = TimeSpan.FromMilliseconds(currentTime);
                string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                actualTime.Hours, actualTime.Minutes, actualTime.Seconds) + " | " + string.Format("{0:D2}:{1:D2}:{2:D2}",
                timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
                this.currentTime.Text = formattedTime;
                // Mise à jour de la position du slider
                timeSlider.Maximum = totalTime;
                timeSlider.Value = currentTime;
            }
        }
        private void MediaPlayer_LoadTracks(object sender, EventArgs e)
        {
            // Le média est en cours de lecture, vous pouvez obtenir les sous-titres
            var tracks = media.Tracks;

            if (!isSubtitlesLoaded)
            {
                Dispatcher.Invoke(() =>
                {
                    foreach (var track in tracks)
                    {
                        if (track.TrackType == TrackType.Text)
                            // Ajoutez le nom du sous-titre à une ListBox (ou toute autre logique que vous souhaitez)
                            subtitleTracks.Add(new TrackInformation(track.Id, track.Language));
                        if (track.TrackType == TrackType.Audio)
                            audioTracks.Add(new TrackInformation(track.Id, track.Language));
                    }
                    subtitleTracks.Add(new TrackInformation(mediaPlayer.Spu, "Disable"));
                    audioTracks.Add(new TrackInformation(mediaPlayer.AudioTrack, "Disable"));

                    subtitlesListBox.ItemsSource = subtitleTracks;
                    audioListBox.ItemsSource = audioTracks;
                });
                isSubtitlesLoaded = true;
            }

        }
        private void subtitlesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Récupérez l'index de l'élément sélectionné dans la ListBox
            int selectedIndex = subtitlesListBox.SelectedIndex;
            // Si un élément est sélectionné
            if (selectedIndex >= 0)
            {
                // Récupérez la piste de sous-titres correspondante
                TrackInformation t = subtitlesListBox.Items.GetItemAt(selectedIndex) as TrackInformation;

                // Définissez la piste de sous-titres active sur la piste sélectionnée
                mediaPlayer.SetSpu(t.id);
            }
            else
            {
                mediaPlayer.SetSpu(0);
            }
        }
        private void audioListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Récupérez l'index de l'élément sélectionné dans la ListBox
            int selectedIndex = subtitlesListBox.SelectedIndex;
            // Si un élément est sélectionné
            if (selectedIndex >= 0)
            {
                // Récupérez la piste de sous-titres correspondante
                TrackInformation t = audioListBox.Items.GetItemAt(selectedIndex) as TrackInformation;

                // Définissez la piste de sous-titres active sur la piste sélectionnée
                mediaPlayer.SetAudioTrack(t.id);
            }
            else
            {
                mediaPlayer.SetAudioTrack(0);
            }

        }
        private void timeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!mediaPlayer.IsPlaying)
                mediaPlayer.Time = (long)timeSlider.Value;
        }
        #endregion
        #region APICALLS
        private async Task UploadFileInChunks(string filePath)
        {
            const int chunkSize = 50 * 1024 * 1024; // Taille de chaque chunk (1 Mo dans cet exemple)
            byte[] buffer = new byte[chunkSize];
            int bytesRead;
            string fileId = AppSetting.RandomString();


            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                int chunkNumber = 0;
                List<Task> chunkSendingTasks = new List<Task>();
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Créez un objet FileChunk pour le chunk actuel
                    FileChunk fileChunk = new FileChunk
                    {
                        FileId = fileId,
                        ChunkNumber = chunkNumber,
                        Data = new byte[bytesRead]
                    };

                    // Copiez les données du chunk dans l'objet FileChunk
                    Array.Copy(buffer, fileChunk.Data, bytesRead);

                    // Envoyez le chunk au serveur
                    chunkSendingTasks.Add(SendChunkToApi(fileChunk));

                    chunkNumber++;
                }
                await Task.WhenAll(chunkSendingTasks);
                await UploadFileFinished(fileId, filePath.Substring(filePath.LastIndexOf('.') + 1));

            }

        }

        private async Task UploadFileFinished(string fileId, string fileExtension)
        {
            // Créez un objet HttpClient
            using (HttpClient httpClient = new HttpClient())
            {
                // Construisez l'URL complète avec les valeurs de fileId et extension
                string fullUrl = settings.ApiUrl+ settings.UploadEndpointSaveFile+"/"+ fileId + "/" + fileExtension;

                // Effectuez la requête POST
                HttpResponseMessage response = await httpClient.PostAsync(fullUrl, null);

                // Traitez la réponse
                if (response.IsSuccessStatusCode)
                {
                    // La requête a réussi
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Réponse du serveur : " + responseBody);
                }
                else
                {
                    // La requête a échoué
                    Console.WriteLine("Erreur de la requête : " + response.StatusCode);
                }
            }
        }
        
        private async Task SendChunkToApi(FileChunk fileChunk)
    
        {
            using (HttpClient httpClient = new HttpClient())
            {

                // Sérialisation de l'objet FileChunk en JSON
                string jsonContent = JsonConvert.SerializeObject(fileChunk);

                // Créer un objet StringContent pour le contenu JSON
                StringContent stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Effectuer la requête POST vers l'API
                HttpResponseMessage response = await httpClient.PostAsync(settings.ApiUrl+settings.UploadEndpointChunk, stringContent);

                // Traiter la réponse ici
                if (response.IsSuccessStatusCode)
                {
                    // Le chunk a été envoyé avec succès
                    Console.WriteLine($"Chunk {fileChunk.ChunkNumber} envoyé avec succès.");
                }
                else
                {
                    // Gérer les erreurs ici
                    Console.WriteLine($"Erreur lors de l'envoi du chunk {fileChunk.ChunkNumber}. Code de statut : {response.StatusCode}");
                }
            }
        }
        #endregion
        #region CONFIGS
        private void OpenSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Settings settingsDialog = new Settings(settings.WebSocketUrl, settings.ApiUrl);
            Window window = new Window();
            window.Content = settingsDialog;
            bool? result = window.ShowDialog();
            // Si l'utilisateur a enregistré les modifications, mettez à jour les paramètres de configuration
            if (result == true)
            {
                settings.WebSocketUrl = settingsDialog.websocketUrlSetting;
                settings.ApiUrl = settingsDialog.apiUrlSetting;

                // Sauvegardez les paramètres mis à jour dans le fichier appsettings.json
                SaveConfiguration();
            }
        }
        private void LoadConfiguration()
        {
            try
            {
                // Chemin vers le fichier appsettings.json
                string configFilePath = "appsettings.json";

                // Vérifiez si le fichier de configuration existe
                if (File.Exists(configFilePath))
                {
                    // Lisez le contenu du fichier JSON
                    string json = File.ReadAllText(configFilePath);
                    
                    settings = JsonConvert.DeserializeObject<AppSetting>(json);
                    // Désérialisez le contenu JSON en un objet AppSettings
                }
                else
                {
                    MessageBox.Show("Le fichier de configuration appsettings.json n'a pas été trouvé.");
                    FileStream fs = File.Create(configFilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement de la configuration : {ex.Message}");
            }
        }

        private void SaveConfiguration()
        {
            try
            {
                // Sérialisez les paramètres de configuration en JSON
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);

                // Écrivez le JSON dans un fichier
                File.WriteAllText("appsettings.json", json);
            }
            catch (Exception ex)
            {
                // Gérez les erreurs de sauvegarde ici (par exemple, en affichant un message d'erreur)
                MessageBox.Show("Erreur lors de la sauvegarde des parametres");
            }
        }
        #endregion
    }
    /*             
     *             For loading files
     *             
     *             media.Parse();
                while (!media.IsParsed)
                {
                    currentTime.Text = "Waiting info...";
                }*/
}
