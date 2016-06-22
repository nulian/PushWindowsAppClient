using PushoverTest.PushOver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PushoverTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private PushOver.PushOver pushOver;

        public MainPage()
        {
           
            this.InitializeComponent();
            pushOver = new PushOver.PushOver();
        }

        private async void submitLogin_Click(object sender, RoutedEventArgs e)
        {
            if (usernameBox.Text.Length != 0 && passwordBox.Password.Length != 0)
            {
                if (await pushOver.RequestUserInfoAsync(usernameBox.Text, passwordBox.Password))
                {
                    if (await pushOver.RegisterDeviceAsync())
                    {
                        var messageList = await pushOver.RetrieveCurrentMessagesAsync();
                        var messageStore = new MessageStore();
                        await messageStore.StoreMessages(messageList.messages);
                        this.Frame.Navigate(typeof(PushMessagesList), null);
                    }
                }
            }
        }
    }
}
