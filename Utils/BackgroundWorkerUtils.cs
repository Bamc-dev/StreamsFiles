using Newtonsoft.Json;
using StreamsFiles.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static System.Net.WebRequestMethods;

namespace StreamsFiles.Utils
{
    public class BackgroundWorkerUtils
    {
        public BackgroundWorker Bg { get; set; }
        private static int instances = 0;
        private static int chunksNumberTotal = 0;
        private static int chunksNumberUploaded;

        public string FilePath { get; set; }
        public AppSetting settings { get; set; }
        private string fileId;
        private TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
        private MainWindow mainWindow;
        public BackgroundWorkerUtils(string filePath, AppSetting appSetting, MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            Bg = new BackgroundWorker();
            Bg.WorkerReportsProgress = true;
            Bg.DoWork += DoWork;
            Bg.ProgressChanged += ProgressChanged;
            Bg.RunWorkerCompleted += Completed;
            FilePath = filePath;
            settings = appSetting;
            fileId = AppSetting.RandomString() + "-" + Path.GetFileName(filePath).Split('.')[0];
            instances++;
        }
        ~BackgroundWorkerUtils()
        {
            instances--;
        }

        private async void DoWork(object sender, DoWorkEventArgs e)
        {
            const int chunkSize = 2*1024*1024;
            byte[] buffer = new byte[chunkSize];
            int bytesRead;



            using (FileStream fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
            {
                int chunkNumber = 0;
                int fileSizeByChunks = (int)(fileStream.Length / chunkSize);
                chunksNumberTotal += fileSizeByChunks;
                Debug.WriteLine(chunksNumberTotal);
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
                    if (chunkNumber % 10 == 0 || buffer.Length < bytesRead)
                    {
                        await Task.WhenAll(chunkSendingTasks);
                        chunkSendingTasks.Clear();
                    }
                    chunkNumber++;
                }

                completionSource.TrySetResult(true);
            }

        }
        protected void ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {

        }
        protected async void Completed(object? sender, EventArgs e)
        {
            await completionSource.Task;
            Thread.Sleep(2000);
            UploadFileFinished(fileId, FilePath.Substring(FilePath.LastIndexOf('.') + 1));

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
                HttpResponseMessage response = await httpClient.PostAsync(settings.ApiUrl + settings.UploadEndpointChunk, stringContent);
                // Traiter la réponse ici
                if (response.IsSuccessStatusCode)
                {
                    chunksNumberUploaded++;
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        
                        Debug.WriteLine(chunksNumberUploaded + " / " + chunksNumberTotal + " : " + CalculerPourcentage(chunksNumberUploaded, chunksNumberTotal));
                        mainWindow.UpdateProgressBar(CalculerPourcentage(chunksNumberUploaded, chunksNumberTotal));
                    });
                    Debug.WriteLine("Chunk : " + fileChunk.ChunkNumber + "Received by Server");
                }
            }
        }
        private async Task UploadFileFinished(string fileId, string fileExtension)
        {
            Debug.WriteLine("This method called");
            // Créez un objet HttpClient
            using (HttpClient httpClient = new HttpClient())
            {
                // Construisez l'URL complète avec les valeurs de fileId et extension
                string fullUrl = settings.ApiUrl + settings.UploadEndpointSaveFile + "/" + fileId + "/" + fileExtension;

                // Effectuez la requête POST
                HttpResponseMessage response = await httpClient.PostAsync(fullUrl, null);

                // Traitez la réponse
                if (response.IsSuccessStatusCode)
                {
                    // La requête a réussi
                    string responseBody = await response.Content.ReadAsStringAsync();

                }
                else
                {
                    Debug.WriteLine("Erreur de la requête : " + response.StatusCode + " | " + response);
                }
            }
        }
        private int CalculerPourcentage(int dividende, int diviseur)
        {
            if (diviseur == 0)
            {
                throw new ArgumentException("Le diviseur ne peut pas être zéro.");
            }

            double resultatDecimal = (double)dividende / diviseur * 100;
            int resultat = (int)resultatDecimal;

            return resultat;
        }
        public static int GetActiveInstances()
        {
            return instances;
        }
        public static int GetChunksNumberTotal()
        {
            return chunksNumberTotal;
        }
        public static void DeleteChunksNumber()
        {
            chunksNumberTotal = 0;
            chunksNumberUploaded = 0;
        }
    }
}