using LibVLCSharp.Shared;
using LibVLCSharp.WPF;
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
        private bool isFullscreen = false;
        private int originalColumnSpan;
        private int originalRowSpan;
        public MainWindow()
        {
            InitializeComponent();
            Core.Initialize();
            libVLC = new LibVLC("--no-xlib");
            mediaPlayer = new MediaPlayer(libVLC);
            videoView.MediaPlayer = mediaPlayer;
            volumeSlider.Value = mediaPlayer.Volume;
        }

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
            Media media = new Media(libVLC, new Uri("C:\\Users\\Admin Stagiaire\\source\\repos\\Bamc-dev\\StreamsFiles\\sample_1280x720_surfing_with_audio.mkv"));
            media.Parse();
            mediaPlayer.Play(media);
            timeSlider.Value = 0;
            timeSlider.Minimum = 0;
            timeSlider.Maximum = mediaPlayer.Length * 1000;
            currentTime.Text = ""+media.Duration;

        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            // Code pour mettre en pause la vidéo
            mediaPlayer.Pause();
        }
        private void SeekForwardButton_Click(object sender, RoutedEventArgs e)
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
            
            mediaPlayer.Time = (long)timeSlider.Value*1000;
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

    }
}
