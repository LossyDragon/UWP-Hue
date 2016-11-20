using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.Storage;
using HueLibrary;
using System;
using System.Diagnostics;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace UWP_Hue
{
    internal sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        
        private Bridge _bridge;
        private ObservableCollection<Light> _lights;
        private ObservableCollection<Light> Lights
        {
            get { return _lights; }
            set
            {
                if (_lights != value)
                {
                    _lights = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Lights)));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// ¯\_(ツ)_/¯ 
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

#if (DEBUG)
            GetLightInfo.Visibility = Visibility.Visible;
#endif

        }

        /// <summary>
        /// Fires when the page is navigated to, which occurs after the extended
        /// splash screen has finished loading all Hue resources. 
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Setup.HuePayload args = e.Parameter as Setup.HuePayload;
            if (null != args)
            {
                _bridge = args.Bridge;
                Lights = new ObservableCollection<Light>(args.Lights);
            }

        }

        /// <summary>
        /// Refreshes the UI to match the actual state of the lights.
        /// </summary>
        private async void LightRefresh_Click(object sender, RoutedEventArgs e)
        {
            Lights = new ObservableCollection<Light>(await _bridge.GetLightsAsync());
        }

        /// <summary>
        /// Deletes the config files of an already set up bridge.
        /// </summary>
        private async void BridgeDelete(object sender, RoutedEventArgs e)
        {
            //Constructor for Content Dialog
            ContentDialog deleteFileDialog = new ContentDialog()
            {
                Title = "Confirm...",
                Content = "Are you sure you want to delete your API configuration?\r\nNext time you launch, you will need to pair the unit again.",
                PrimaryButtonText = "I am Sure",
                SecondaryButtonText = "Cancel"
            };
            
            //Wait for result
            ContentDialogResult result = await deleteFileDialog.ShowAsync();

            //Continue with deletion if user presses Primary Button.
            if (result == ContentDialogResult.Primary)
            {
                var localStorage = ApplicationData.Current.LocalSettings.Values;
                localStorage.Remove("bridgeIp");
                localStorage.Remove("userId");
            }
        }

        //Below are CommandBar Button actions.

        /// <summary>
        /// Turns all lights configured to bridge ON or OFF, depending on state.
        /// </summary>
        private async void Lights_OnOff(object sender, RoutedEventArgs e)
        {
            foreach (var x in Lights)
            {
                x.State.On = !x.State.On;   //Bool Switch.
                await x.ChangeStateAsync();
                Debug.WriteLine("Light " + x.Id + " state is now: " + x.State.On);
            }
        }

        /// <summary>
        /// Used to display some information within the HueLibrary (Bridge and Light info).
        /// </summary>
        private void GetLights(object sender, RoutedEventArgs e)
        {
            foreach (var x in Lights)
            {
                Debug.WriteLine("Light ID: " + x.Id);           //Shows the ID of a bulb from bridge.
                Debug.WriteLine("Model Num: " + x.ModelId);     //Shows the Model Number of a bulb.
                Debug.WriteLine("State: " + x.State.On);        //Shows the On/Off state of a bulb.
                Debug.WriteLine("----------");

            }
        }
    }
}
