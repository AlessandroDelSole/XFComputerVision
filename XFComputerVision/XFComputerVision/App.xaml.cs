using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace XFComputerVision
{
	public partial class App : Application
	{
        internal static VisionServiceClient visionClient;
        internal static EmotionServiceClient emotionClient;

        public App ()
		{
			InitializeComponent();

            visionClient = new VisionServiceClient("YOUR-KEY-GOES-HERE",
    "https://westeurope.api.cognitive.microsoft.com/vision/v1.0");

            emotionClient = new EmotionServiceClient("YOUR-KEY-GOES-HERE",
                            "https://westus.api.cognitive.microsoft.com/emotion/v1.0");

            MainPage = new XFComputerVision.MainPage();
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
