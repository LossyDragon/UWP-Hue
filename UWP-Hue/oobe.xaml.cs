using System;
using System.Diagnostics;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

//TODO: This can be better. But it works for now

namespace UWP_Hue
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class oobe : Page
    {

        public oobe()
        {
            //Function is called to see if HUE has been set-up or not.
            CheckSetup();
            this.InitializeComponent();
        }

        /// <summary>
        /// When the application launches, check to see if setup has been done, if so, go to main.
        /// </summary>
        private void CheckSetup()
        {
            try
            {
                var localStorage = ApplicationData.Current.LocalSettings.Values;
                Debug.WriteLine(localStorage);
                if (localStorage.ContainsKey("bridgeIp") && localStorage.ContainsKey("userId"))
                {
                    Debug.WriteLine("---YES----");
                    this.Frame.Navigate(typeof(Main));
                }
                Debug.WriteLine("---NO---");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        //User Select oobe setup.
        private async void SetMeUp(object sender, RoutedEventArgs e)
        {

            var localStorage = ApplicationData.Current.LocalSettings.Values;
            Debug.WriteLine(localStorage);
            if (localStorage.ContainsKey("bridgeIp") && localStorage.ContainsKey("userId"))
            {
                Debug.WriteLine("---YES----");

                Setup setup = new Setup(); //Setup Temporaty class to find HUE bridge/lights...
                setup.PressedButton_Click(null,null);

                this.Frame.Navigate(typeof(Main));
            }
            else
            {
                this.Frame.Navigate(typeof(Setup));
            }
        }

    }
}
