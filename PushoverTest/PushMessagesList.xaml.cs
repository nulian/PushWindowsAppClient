using PushoverTest.PushOver;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PushoverTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PushMessagesList : Page
    {
        private PushOver.PushOver pushOver;
        private MessageStore messageStore;
        private ObservableCollection<Message> messageList;
        public ObservableCollection<Message> MessageList { get { return this.messageList; } }
        private SocketHandler socketHandler;
        public PushMessagesList()
        {
            socketHandler = new SocketHandler(this);
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            messageStore = new MessageStore();
            this.LoadMessages();
            pushOver = new PushOver.PushOver();
            // socketHandler.InitializeConnection();
            // socketHandler.startConnectionAsync();
            //socketHandler.OnConnect();
        }

        private async void LoadMessages()
        {
            messageList = new ObservableCollection<Message>(await messageStore.ReadStoredMessagesAsync());
            await socketHandler.ConnectAsync();
            await socketHandler.SendAsync();


        }
    }
}
