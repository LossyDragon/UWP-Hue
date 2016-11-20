using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using HueLibrary;
using System;
using System.Diagnostics;
using System.Threading;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace UWP_Hue
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    internal sealed partial class Main : Page, INotifyPropertyChanged
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
        private IBackgroundTaskRegistration _taskRegistration;
        private const string _taskName = "HueBackgroundTask";

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Constructor for MainPage.
        /// </summary>
        public Main()
        {
            this.InitializeComponent();
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
            _taskRegistration = BackgroundTaskRegistration.AllTasks.Values
                .FirstOrDefault(x => x.Name == _taskName);

        }

        /// <summary>
        /// Refreshes the UI to match the actual state of the lights.
        /// </summary>
        private async void LightRefresh_Click(object sender, RoutedEventArgs e)
        {
            Lights = new ObservableCollection<Light>(await _bridge.GetLightsAsync());
        }

        private async void BridgeDelete(object sender, RoutedEventArgs e)
        {

            ContentDialog deleteFileDialog = new ContentDialog()
            {
                Title = "Confirm...",
                Content = "Are you sure you want to delete your API configuration?\r\nNext time you launch, you will need to pair the unit again.",
                PrimaryButtonText = "Yes!",
                SecondaryButtonText = "No!"
            };

            ContentDialogResult result = await deleteFileDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var localStorage = ApplicationData.Current.LocalSettings.Values;
                localStorage.Remove("bridgeIp");
                localStorage.Remove("userId");
            }
        }

        ///
        /// Below are CommandBar functions. 
        /// 

        //Turn all lights ON or OFF, depending on state.
        private async void Lights_OnOff(object sender, RoutedEventArgs e)
        {
            foreach (var x in Lights)
            {
                try
                {
                    x.State.On = !x.State.On;   //Bool Switch.

                    await x.ChangeStateAsync();
                    Debug.WriteLine("Light " + x.Id + " state is now: " + x.State.On);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        //Who Knows...
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
