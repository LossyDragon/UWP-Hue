using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

//TODO: Background image?

namespace UWP_Hue
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class oobe : Page
    {

        public bool Bypass { get; private set; }

        public oobe()
        {
            this.InitializeComponent();
            //Function is called to see if HUE has been set-up or not.
            CheckSetup();
        }

        private async Task CheckSetup()
        {
            try
            {
                //Does not work for saved files.
                var folders = await ApplicationData.Current.LocalFolder.GetFoldersAsync();

                if (folders.Count > 0)
                {
                    this.Frame.Navigate(typeof(Main));
                }
                
            }
            catch(Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        //User Select oobe setup.
        private void SetMeUp(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(oobeSetup));
        }

    }
}
