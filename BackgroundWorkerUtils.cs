using Newtonsoft.Json;
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
using static System.Net.WebRequestMethods;

namespace StreamsFiles
{
    public class BackgroundWorkerUtils
    {
        public BackgroundWorker Bg { get; set; }
        public string FilePath { get; set; }
        public AppSetting settings { get; set; }
        private string fileId;
        public BackgroundWorkerUtils(string filePath, AppSetting appSetting)
        {
            Bg = new BackgroundWorker();
            Bg.WorkerReportsProgress = true;
            Bg.DoWork += DoWork;
            Bg.ProgressChanged += ProgressChanged;
            Bg.RunWorkerCompleted += Completed;
            this.FilePath = filePath;
            this.settings = appSetting;
            fileId = AppSetting.RandomString() + "-" + Path.GetFileName(filePath).Split('.')[0];

        }

        private async void DoWork(object sender, DoWorkEventArgs e)
        {
            const int chunkSize = 50 * 1024 * 1024; // Taille de chaque chunk (1 Mo dans cet exemple)
            byte[] buffer = new byte[chunkSize];
            int bytesRead;
            


            using (FileStream fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
            {
                int chunkNumber = 0;
                List<Task> chunkSendingTasks = new List<Task>();
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Créez un objet FileChunk pour le chunk actuel
                    FileChunk fileChunk = new FileChunk
                    {
                        FileId = this.fileId,
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

            }
        }
        protected void ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {

        }
        protected void Completed(object? sender, EventArgs e)
        {
            Thread.Sleep(5000);
            UploadFileFinished(this.fileId, FilePath.Substring(FilePath.LastIndexOf('.') + 1));

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
        private async Task UploadFileFinished(string fileId, string fileExtension)
        {
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
    }
}
