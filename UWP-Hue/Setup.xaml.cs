using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using HueLibrary;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Windows.UI.Core;
using Windows.Foundation;

/// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
///Some code sampled from "HueLightController" on the MicrosoftUWP developer portal.


//TODO: Try and merge "oobe" with this class.

namespace UWP_Hue
{
    internal partial class Setup
    {
        //Init variables
        protected Frame rootFrame = new Frame();
        private Bridge _bridge;
        private IEnumerable<Light> _lights;

        public Setup()
        {
            this.InitializeComponent();
        }

        //When the proceed button is pressed, proceed with functions.
        private void PressedButton_Click(object sender, RoutedEventArgs e)
        {
            Payload();
        }

        public async void Payload()
        {
            await FindBridgeAsync();    //Find Phillips Hue Bridge.
            await FindLightsAsync();    //Find Lights after Bridge.
            SaveBridge();               //Save config files.

            await rootFrame.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                rootFrame.Navigate(typeof(MainPage),
                    new HuePayload { Bridge = _bridge, Lights = _lights });
                Window.Current.Content = rootFrame;
            });
        }

        /// <summary>
        /// Tries to find the bridge using multiple methods.
        /// http://www.developers.meethue.com/documentation/hue-bridge-discovery
        /// </summary>
        private async Task FindBridgeAsync()
        {
            try
            {
                // First attempt: local storage cache.
                var localStorage = ApplicationData.Current.LocalSettings.Values;
                if (localStorage.ContainsKey("bridgeIp") && localStorage.ContainsKey("userId"))
                {
                    _bridge = new Bridge(
                        localStorage["bridgeIp"].ToString(),
                        localStorage["userId"].ToString());

                    if (await PrepareBridgeAsync())
                    {
                        return;
                    }
                }

                // Second attempt: Hue N-UPnP service.
                _bridge = await Bridge.FindUPnP();

                if (await PrepareBridgeAsync())
                {
                    return;
                }

                // Third attempt: Re-try Hue N-UPnP service.
                await DispatchAwaitableUITask(async () =>
                    await RetryBridgeSearchPopup());
                _bridge = await Bridge.FindAsync();

                if (await PrepareBridgeAsync())
                {
                    return;
                }

                // Final attempt: manual entry.
                await DispatchAwaitableUITask(async () =>
                {
                    await BridgeEntryPopup.ShowAsync();
                    _bridge = new Bridge(BridgEntryIp.Text);
                });

                if (await PrepareBridgeAsync())
                {
                    return;
                }
            }

            //Uh-Oh...
            catch (Exception e)
            {
                await ReportErrorPopup(
                    "We encountered an unexpected problem trying to find your bridge: " + e + "/r/nThe Application will now exit...", true);
            }

            await ReportErrorPopup("We couldn't find your bridge. Make sure it's powered on, " +
                "has 3 blue lights illuminated, on the same network as this device, " +
                "and that you're connected to the Internet.", false);

            this.Frame.Navigate(typeof(oobe));
        }

        /// <summary>
        /// Saves bridge information to the app's local storage so the app can load faster next time. 
        /// </summary>
        private void SaveBridge()
        {
            var localStorage = ApplicationData.Current.LocalSettings.Values;
            localStorage["bridgeIp"] = _bridge.Ip;
            localStorage["userId"] = _bridge.UserId;

            //Show some bridge information.
            Debug.WriteLine(_bridge.Ip);
            Debug.WriteLine(_bridge.UserId);
        }

        private Task DispatchAwaitableUITask(Func<Task> task)
        {
            var completion = new TaskCompletionSource<bool>();
            var action = rootFrame.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    await task.Invoke();
                    completion.TrySetResult(true);
                }
                catch (Exception e)
                {
                    completion.TrySetException(e);
                }
            });
            return completion.Task;
        }


        /// <summary>
        /// Checks whether the bridge is reachable and the app is authorized to send commands. If the bridge
        /// is reachable but the app isn't authorized, it prompts the user to register it. 
        /// </summary>
        private async Task<bool> PrepareBridgeAsync(int attempts = 0)
        {
            if (null == _bridge || attempts > 2)
            {
                return false;
            }
            switch (await _bridge.PingAsync())
            {
                case BridgeConnectionStatus.Success:
                    return true;
                case BridgeConnectionStatus.Fail:
                    return false;
                case BridgeConnectionStatus.Unauthorized:
                    await _bridge.RegisterAsync();
                    return await PrepareBridgeAsync(++attempts);
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Tries to find lights on the network.
        /// </summary>
        private async Task FindLightsAsync()
        {
            try
            {
                _lights = new ObservableCollection<Light>(await _bridge.GetLightsAsync());
                if (!_lights.Any())
                {
                    await ReportErrorPopup("We couldn't find any lights. Make sure they're in " +
                        "range and connected to a power source.", false);
                }
            }
            catch (Exception e)
            {
                await ReportErrorPopup(
                    "We encountered an unexpected problem trying to find your lights: " + e, true);
            }
        }

        //TODO
        private async Task RetryBridgeSearchPopup()
        {
            ContentDialog retryBridge = new ContentDialog()
            {
                Title = "Find bridge",
                Content = "TODO",
                PrimaryButtonText = "Okay"
            };

            ContentDialogResult result = await retryBridge.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                return;
            }
        }

        //Report an error Function...
        private async Task ReportErrorPopup(string message, bool error)
        {
            //Constructor for Content Dialog
            ContentDialog errorReport = new ContentDialog()
            {
                Title = "Something went wrong!",
                Content = message,
                PrimaryButtonText = "Okay"
            };

            ContentDialogResult result = await errorReport.ShowAsync();

            if (result == ContentDialogResult.Primary && error == true)
            {
                Application.Current.Exit();
            }
        }

        internal class HuePayload
        {
            public Bridge Bridge { get; set; }
            public IEnumerable<Light> Lights { get; set; }
        }
    }
}
