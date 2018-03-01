using Microsoft.ProjectOxford.Common.Contract;
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
    public partial class EmotionPage : ContentPage
    {
        public EmotionPage()
        {
            InitializeComponent();
        }

        private async Task<IEnumerable<string>> RecognizeEmotionAsync(Stream imageFile)
        {
            // Get emotions from the specified stream
            Emotion[] emotionResult = await App.emotionClient.RecognizeAsync(imageFile);
            // Assuming the picture has one face, retrieve emotions for the
            // first item in the returned array
            var faceEmotion = emotionResult[0]?.Scores.ToRankedList();

            var emotions = new List<string>();

            foreach(var emotion in faceEmotion)
            {
                emotions.Add($"{emotion.Key}, {emotion.Value}");
            }

            return emotions.AsEnumerable();
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
                this.BindingContext = await RecognizeEmotionAsync(file.GetStream());
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
            this.BindingContext = await RecognizeEmotionAsync(file.GetStream());

            this.Indicator1.IsRunning = false;
            this.Indicator1.IsVisible = false;
        }
    }
}