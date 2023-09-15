using LibVLCSharp.Shared;
using Microsoft.Win32;
using Newtonsoft.Json;
using StreamsFiles.Entity;
using StreamsFiles.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using File = System.IO.File;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;

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
        private WebSocketClient wsClient;
        private Thread monitorThread;



        private bool isSubtitlesLoaded = false;
        private bool isFullscreen = false;
        private int originalColumnSpan;
        private int originalRowSpan;
        private bool rightMouseButtonReleased = true;
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
            timeSlider.PreviewMouseRightButtonDown += TimeSlider_PreviewMouseRightButtonDown;
            timeSlider.PreviewMouseRightButtonUp+= TimeSlider_PreviewMouseRightButtonUp;
            mediaPlayer.Paused += MediaPlayer_Paused;
            if (!string.IsNullOrWhiteSpace(settings.WebSocketUrl))
            {
                this.wsClient = new WebSocketClient(settings.WebSocketUrl);
                this.wsClient.Connect();
                this.wsClient.MessagePlay += WsClient_MessagePlayReceived;
                this.wsClient.MessagePause += WsClient_MessagePauseReceived;
                this.wsClient.MessageUrl += WsClient_MessageUrl;
                this.wsClient.MessageTime += WsClient_MessageTime;
                this.wsClient.MessageStop += WsClient_MessageStop;


            }
            monitorThread = new Thread(MonitorWillPlay);
        }

        private void MediaPlayer_Paused(object? sender, EventArgs e)
        {
            _timer.Start();
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

                if (mediaPlayer.IsPlaying)
                {
                    wsClient.SendMessage("pause");
                   
                }
                else
                {
                    wsClient.SendMessage("play");
                }
        }
        private void SyncTime_Click(object sender, RoutedEventArgs e)
        {
            if (!mediaPlayer.IsPlaying)
            {
                wsClient.SendMessage("time:" + Convert.ToInt64(timeSlider.Value));
            }

        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            // Code pour arrêter la vidéo
            wsClient.SendMessage("stop");
        }
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Réglez le volume du lecteur vidéo en fonction de la valeur du Slider
            
            mediaPlayer.Volume = (int)volumeSlider.Value;
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
                    BackgroundWorkerUtils bg = new BackgroundWorkerUtils(fichier, settings);
                    bg.Bg.RunWorkerAsync();
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
        private void TimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Réglez la position de la vidéo en fonction de la valeur du Slider
            if(!mediaPlayer.IsPlaying)
            {
                if(rightMouseButtonReleased)
                {
                    this.wsClient.SendMessage("time:" + (long)timeSlider.Value);
                }
            }
        }
        private void TimeSlider_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            rightMouseButtonReleased = false;
        }

        private void TimeSlider_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            rightMouseButtonReleased = true;
        }
        private void timeSlider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            wsClient.SendMessage("time:" + (long)timeSlider.Value * 1000);
        }
        private void btn_OpenFile_Click(object sender, RoutedEventArgs e)
        {

            SelectFiles selectFiles = new SelectFiles(settings.ApiUrl);
            Window window = new Window();
            window.Width = 500;
            window.Height = 400;
            window.Content = selectFiles;
            bool? result = window.ShowDialog();
            // Si l'utilisateur a enregistré les modifications, mettez à jour les paramètres de configuration
            if (result == true)
            {
                wsClient.SendMessage("url:" + settings.ApiUrl + selectFiles.SelectedFile.downloadUri);
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
                if (!string.IsNullOrWhiteSpace(settings.WebSocketUrl))
                {
                    this.wsClient = new WebSocketClient(settings.WebSocketUrl);
                }
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
        #region WEBSOCKET
        private void WsClient_MessagePlayReceived(object sender, string obj)
        {
            if(mediaPlayer.Media != null)
            {

                mediaPlayer.Play();
                _timer.Start();
            }
            else
            {
                mediaPlayer.Play(media);
                _timer.Start();

            }
        }
        private void WsClient_MessagePauseReceived(object sender, string obj)
        {
            if(mediaPlayer.Media != null)
            {
                mediaPlayer.Pause();
            }
        }
        private void WsClient_MessageTime(object sender, string obj)
        {
            if (mediaPlayer.Media != null)
            {
                if(!mediaPlayer.IsPlaying) {
                    mediaPlayer.Time = long.Parse(obj.Split(':')[1]);
                }

            }
        }
        private void WsClient_MessageUrl(object sender, string obj)
        {
            media = new Media(libVLC, obj.Substring(4), FromType.FromLocation);
            monitorThread.Start();
        }
        private void WsClient_MessageStop(object sender, string obj)
        {
            mediaPlayer.Stop();
            mediaPlayer.Media = null;
        }

        private void MonitorWillPlay()
        {
            while (mediaPlayer.WillPlay)
            {
                // Mettez à jour l'interface utilisateur depuis le thread de l'interface utilisateur
                Dispatcher.Invoke(() =>
                {
                    if (mediaPlayer.WillPlay)
                    {
                        txt_MediaStatus.Text = "Can Play";
                        txt_MediaStatus.Foreground = Brushes.Green;
                    }
                    else
                    {
                        txt_MediaStatus.Text = "Waiting";
                        txt_MediaStatus.Foreground = Brushes.Red;

                    }
                });
            }
        }
        #endregion
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.wsClient.Disconnect().Wait();
        }
    }
}
