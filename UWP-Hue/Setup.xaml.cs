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

/// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
///Some code sampled from "HueLightController" on the MicrosoftUWP developer portal.

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
            await FindModelBridge();
            SaveBridge();               //Save config files.

            await rootFrame.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                rootFrame.Navigate(typeof(MainPage),
                    new HuePayload { Bridge = _bridge, Lights = _lights });
                Window.Current.Content = rootFrame;
            });
        }

        /// <summary>
        /// Function to obtain the Brigde's model before launching MainPage.xaml
        /// </summary>
        private async Task FindModelBridge()
        {
            await _bridge.FindBridgeModelName();
        }

        /// <summary>
        /// Tries to find the bridge using multiple methods.
        /// http://www.developers.meethue.com/documentation/hue-bridge-discovery
        /// </summary>
        private async Task FindBridgeAsync()
        {
            try
            {
                //Check local config file first before Bridge Discovery.
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

                //First attempt: Try Hue N-UPnP service.
                _bridge = await Bridge.FindAsync();

                if (await PrepareBridgeAsync())
                {
                    return;
                }

                //Second attempt: IP scan.
                await PopUp(
                    "Attention",
                    "UWP-Hue could not a bridge using N-UPnP search. Pressing \"Okay\" will start an IP scan within your network to find a bridge.",
                    "Okay",
                    false);
                _bridge = await Bridge.FindUPnP();
                
                if (await PrepareBridgeAsync())
                {
                    return;
                }

                //Final attempt: Manual entry.
                await DispatchAwaitableUITask(async () =>
                {
                    await BridgeEntryPopup.ShowAsync();
                    _bridge = new Bridge(BridgEntryIp.Text);
                });

                Debug.Write(BridgEntryIp.Text);

                if (await PrepareBridgeAsync() && BridgEntryIp.Text != null)
                {
                    return;
                }
            }
            catch (Exception e)
            {
                await PopUp(
                    "Error",
                    "We encountered an unexpected problem trying to find your bridge: \r\n" + e,
                    "Exit", 
                    true); //There is an issue, terminate the program after user has confirmed popup.
            }

            await PopUp(
                "Check Connection!",
                "We couldn't find your bridge. Make sure it's powered on, " +
                "has 3 blue lights illuminated, on the same network as this device, " +
                "and that you're connected to the Internet.",
                "Exit",
                true);
        }

        /// <summary>
        /// Utility method for awaiting calls sent to the UI thread via the dispatcher. Used to display popups
        /// on the UI thread from a background thread and wait for user input before proceeding. 
        /// </summary>
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
    /// Saves bridge information to the app's local storage so the app can load faster next time. 
    /// </summary>
    private void SaveBridge()
        {
            var localStorage = ApplicationData.Current.LocalSettings.Values;
            localStorage["bridgeIp"] = _bridge.Ip;
            localStorage["userId"] = _bridge.UserId;

            //Show some bridge information.
            //Debug.WriteLine(_bridge.Ip);
            //Debug.WriteLine(_bridge.UserId);
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
                    await PopUp(
                        "Check Lights",
                        "The Brige couldn't find any lights. Check to see they are within range and connected to a power source that is plugged in.",
                        "Okay",
                        false);
                }
            }
            catch (Exception e)
            {
                await PopUp(
                    "Error",
                    "We encountered an unexpected problem trying to find your lights: " + e, 
                    "Exit",
                    true); //There is an issue, terminate the program after user has confirmed popup.
            }
        }

        /// <summary>
        /// Universal function for a popup, with one button.
        /// </summary>
        /// <param name="title">Title of popup</param>
        /// <param name="message">Message of error</param>
        /// <param name="button">Text message for primary button</param>
        /// <param name="shouldQuit">Bool to indicate if the application should quit</param>
        /// <returns></returns>
        private async Task PopUp(string title, string message, string button, bool shouldQuit)
        {
            ContentDialog popup = new ContentDialog()
            {
                Title = title,
                Content = message,
                PrimaryButtonText = button
            };

            ContentDialogResult result = await popup.ShowAsync();

            if (result == ContentDialogResult.Primary && shouldQuit == true)
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
