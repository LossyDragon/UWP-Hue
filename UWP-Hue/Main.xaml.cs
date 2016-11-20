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
            oobeSetup.HuePayload args = e.Parameter as oobeSetup.HuePayload;
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
    }
}
