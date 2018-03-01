using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
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
    public partial class OcrRecognitionPage : ContentPage
    {
        public OcrRecognitionPage()
        {
            InitializeComponent();
        }

        private async Task<OcrResults> AnalyzePictureAsync(Stream inputFile)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await DisplayAlert("Network error", "Please check your network connection and retry.", "OK");
                return null;
            }

            OcrResults ocrResult = await App.visionClient.RecognizeTextAsync(inputFile);
            return ocrResult;
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

            var ocrResult = await AnalyzePictureAsync(file.GetStream());
            this.BindingContext = null;           
            this.BindingContext = ocrResult;

            PopulateUIWithRegions(ocrResult);

            this.Indicator1.IsRunning = false;
            this.Indicator1.IsVisible = false;
        }

        private void PopulateUIWithRegions(OcrResults ocrResult)
        {
            // Iterate the regions
            foreach (var region in ocrResult.Regions)
            {
                // Iterate lines per region
                foreach (var line in region.Lines)
                {
                    // For each line, add a panel
                    // to present words horizontally
                    var lineStack = new StackLayout
                    { Orientation = StackOrientation.Horizontal };

                    // Iterate words per line and add the word
                    // to the StackLayout
                    foreach (var word in line.Words)
                    {
                        var textLabel = new Label { Text = word.Text };
                        lineStack.Children.Add(textLabel);
                    }
                    // Add the StackLayout to the UI
                    this.DetectedText.Children.Add(lineStack);
                }
            }
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

            var ocrResult = await AnalyzePictureAsync(file.GetStream());
            this.BindingContext = ocrResult;

            PopulateUIWithRegions(ocrResult);

            this.Indicator1.IsRunning = false;
            this.Indicator1.IsVisible = false;
        }
    }
}
