using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using Newtonsoft.Json.Linq;
using Plugin.Connectivity;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using XFComputerVision;

namespace XFComputerVision
{
    public partial class CelebrityRecognitionPage : ContentPage
    {
        public CelebrityRecognitionPage()
        {
            InitializeComponent();
        }

        private async void TakePictureButton_Clicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", "No camera available.", "OK");
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                SaveToAlbum = true,
                Name = "test.jpg"
            });

            if (file == null)
                return;

            this.Indicator1.IsVisible = true;
            this.Indicator1.IsRunning = true;

            Image1.Source = ImageSource.FromStream(() => file.GetStream());

            var analysisResult = await AnalyzePictureAsync(file.GetStream());
            this.CelebrityName.Text = ParseCelebrityName(analysisResult.Result);

            this.Indicator1.IsRunning = false;
            this.Indicator1.IsVisible = false;
        }

        private async void UploadPictureButton_Clicked(object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("No upload", "Picking a photo is not supported.", "OK");
                return;
            }

            var file = await CrossMedia.Current.PickPhotoAsync();
            if (file == null)
                return;

            this.Indicator1.IsVisible = true;
            this.Indicator1.IsRunning = true;
            Image1.Source = ImageSource.FromStream(() => file.GetStream());

            AnalysisInDomainResult analysisResult = await AnalyzePictureAsync(file.GetStream());
            this.CelebrityName.Text = ParseCelebrityName(analysisResult.Result);

            this.Indicator1.IsRunning = false;
            this.Indicator1.IsVisible = false;
        }

        private async Task<Model> GetDomainModel()
        {
            ModelResult modelResult = await App.visionClient.ListModelsAsync();
            // At this writing, only celebrity recognition
            // is available. It is the first model in the list
            return modelResult.Models.First();
        }

        private string ParseCelebrityName(object analysisResult)
        {
            JObject parsedJSONresult = JObject.Parse(analysisResult.ToString());

            var celebrities = from celebrity in parsedJSONresult["celebrities"]
                              select (string)celebrity["name"];

            return celebrities.FirstOrDefault();
        }

        private async Task<AnalysisInDomainResult> AnalyzePictureAsync(Stream inputFile)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await DisplayAlert("Network error", "Please check your network connection and retry.", "OK");
                return null;
            }

            AnalysisInDomainResult analysisResult = 
                await App.visionClient.AnalyzeImageInDomainAsync(inputFile, await GetDomainModel());

           return analysisResult;
        }
    }
}
