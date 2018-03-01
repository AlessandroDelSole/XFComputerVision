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
using Xamarin.Forms.Xaml;
using XFComputerVision;

namespace XFComputerVision
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImageAnalysisPage : ContentPage
    {
        public ImageAnalysisPage()
        {
            InitializeComponent();
        }
        
        private async Task<AnalysisResult> AnalyzePictureAsync(Stream inputFile)
        {
            // Use the connectivity plugin to detect
            // if a network connection is available
            // Remember a using Plugin.Connectivity; directive
            if (!CrossConnectivity.Current.IsConnected)
            {
                await DisplayAlert("Network error",
                    "Please check your network connection and retry.", "OK");
                return null;
            }

            VisualFeature[] visualFeatures = new VisualFeature[] { VisualFeature.Adult,
                VisualFeature.Categories, VisualFeature.Color, VisualFeature.Description,
                VisualFeature.Faces, VisualFeature.ImageType, VisualFeature.Tags };

            AnalysisResult analysisResult =
                await App.visionClient.AnalyzeImageAsync(inputFile,
                visualFeatures);

            return analysisResult;
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
            this.BindingContext = await AnalyzePictureAsync(file.GetStream());

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

            try
            {
                this.BindingContext = await AnalyzePictureAsync(file.GetStream());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
                return;
            }
            finally
            {
                this.Indicator1.IsRunning = false;
                this.Indicator1.IsVisible = false;
            }
        }
    }
}