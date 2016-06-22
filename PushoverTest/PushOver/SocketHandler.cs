using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PushoverTest.PushOver
{
    class SocketHandler
    {
        private MessageWebSocket messageWebSocket;
        private DataWriter messageWriter;
        private ApplicationDataContainer localSettings;
        private bool busy;
        private PushMessagesList parentPage;

        public SocketHandler(PushMessagesList parentPage)
        {
            localSettings = ApplicationData.Current.LocalSettings;
            this.parentPage = parentPage;
            //OnConnect();
        }

        private async void OnConnect()
        {
         //   SetBusy(true);
            await ConnectAsync();
           // SetBusy(false);
        }

        public async Task ConnectAsync()
        {
            //if (String.IsNullOrEmpty(InputField.Text))
            //{
            //    //rootPage.NotifyUser("Please specify text to send", NotifyType.ErrorMessage);
            //    return;
            //}

            // Validating the URI is required since it was received from an untrusted source (user input).
            // The URI is validated by calling TryGetUri() that will return 'false' for strings that are not
            // valid WebSocket URIs.
            // Note that when enabling the text box users may provide URIs to machines on the intrAnet
            // or intErnet. In these cases the app requires the "Home or Work Networking" or
            // "Internet (Client)" capability respectively.
            Uri server = TryGetUri("wss://client.pushover.net/push");
            if (server == null)
            {
                return;
            }

            messageWebSocket = new MessageWebSocket();
            messageWebSocket.Control.MessageType = SocketMessageType.Utf8;
            messageWebSocket.MessageReceived += MessageReceived;
            messageWebSocket.Closed += OnClosed;

            //AppendOutputLine($"Connecting to {server}...");
            try
            {
                await messageWebSocket.ConnectAsync(server);
            }
            catch (Exception ex) // For debugging
            {
                // Error happened during connect operation.
                messageWebSocket.Dispose();
                messageWebSocket = null;

                //AppendOutputLine(MainPage.BuildWebSocketError(ex));
                //AppendOutputLine(ex.Message);
                return;
            }

            // The default DataWriter encoding is Utf8.
            messageWriter = new DataWriter(messageWebSocket.OutputStream);
            //rootPage.NotifyUser("Connected", NotifyType.StatusMessage);
        }

        async void OnSend()
        {
            //SetBusy(true);
            await SendAsync();
            //SetBusy(false);
        }

        public async Task SendAsync()
        {
            //string message = InputField.Text;
            //if (String.IsNullOrEmpty(message))
            //{
            //    //rootPage.NotifyUser("Please specify text to send", NotifyType.ErrorMessage);
            //    return;
            //}

            //AppendOutputLine("Sending Message: " + message);

            // Buffer any data we want to send.
            messageWriter.WriteString("login:" + localSettings.Values["device_id"] + ":" + localSettings.Values["secret"] + "\n");

            try
            {
                // Send the data as one complete message.
                var response = await messageWriter.StoreAsync();
            }
            catch (Exception ex)
            {
                //AppendOutputLine(MainPage.BuildWebSocketError(ex));
                //AppendOutputLine(ex.Message);
                return;
            }

            //rootPage.NotifyUser("Send Complete", NotifyType.StatusMessage);
        }

        private async void MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
           
            //AppendOutputLine("Message Received; Type: " + args.MessageType);
            using (DataReader reader = args.GetDataReader())
            {
                reader.UnicodeEncoding = UnicodeEncoding.Utf8;

                try
                {
                    string read = reader.ReadString(reader.UnconsumedBufferLength);
                    switch (read)
                    {
                        case "#":
                            break;
                        case "!":
                            var ignore = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                            {
                                PushOver pushOver = new PushOver();
                                var messageList = await pushOver.RetrieveCurrentMessagesAsync();
                                var messageStore = new MessageStore();
                                await messageStore.StoreMessages(messageList.messages);
                                var messages = await messageStore.ReadStoredMessagesAsync();
                                foreach (var message in messages)
                                {
                                    parentPage.MessageList.Add(message);
                                }
                            });
                            break;
                        case "R":
                            CloseSocket();
                            await ConnectAsync();
                            await SendAsync();
                            break;
                        case "E":
                            CloseSocket();
                            break;

                    }
                    //AppendOutputLine(read);
                }
                catch (Exception ex)
                {
                    //AppendOutputLine(MainPage.BuildWebSocketError(ex));
                    //AppendOutputLine(ex.Message);
                }
            }
           
        }

        private void OnDisconnect()
        {
           // SetBusy(true);
            //rootPage.NotifyUser("Closing", NotifyType.StatusMessage);
            CloseSocket();
            //SetBusy(false);
        }

        // This may be triggered remotely by the server or locally by Close/Dispose()
        private void OnClosed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            // Dispatch the event to the UI thread so we do not need to synchronize access to messageWebSocket.
            //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //{
                //AppendOutputLine("Closed; Code: " + args.Code + ", Reason: " + args.Reason);

                if (messageWebSocket == sender)
                {
                    CloseSocket();
                    //UpdateVisualState();
                }
            //});
        }

        private void CloseSocket()
        {
            if (messageWebSocket != null)
            {
                try
                {
                    messageWebSocket.Close(1000, "Closed due to user request.");
                }
                catch (Exception ex)
                {
                    //AppendOutputLine(MainPage.BuildWebSocketError(ex));
                    //AppendOutputLine(ex.Message);
                }
                messageWebSocket = null;
            }
            if (messageWriter != null)
            {
                messageWriter.Dispose();
                messageWriter = null;
            }
        }


        //public async void startConnectionAsync()
        //{
        //    var localSettings = ApplicationData.Current.LocalSettings;

        //    localSettings.Values["websocketUrl"] = websocketUrl;

        //    Uri server =TryGetUri(websocketUrl);
        //    if (server == null)
        //    {
        //        return;
        //    }

        //    SocketActivityInformation socketInformation;
        //    if (!SocketActivityInformation.AllSockets.TryGetValue(socketId, out socketInformation))
        //    {
        //        socket = new MessageWebSocket();
        //            //socket.EnableTransferOwnership(task.TaskId, SocketActivityConnectedStandbyAction.Wake);
        //            var targetServer = new HostName(hostname);
        //            await socket.ConnectAsync(targetServer, port, SocketProtectionLevel.Tls12);
        //    DataWriter writer = new DataWriter(socket.OutputStream);
        //    writer.WriteString("login:" + (string)localSettings.Values["device_id"] + ":" + (string)localSettings.Values["secret"] + "\n");
        //            // To demonstrate usage of CancelIOAsync async, have a pending read on the socket and call 
        //            // cancel before transfering the socket. 
        //            DataReader reader = new DataReader(socket.InputStream);
        //            reader.InputStreamOptions = InputStreamOptions.Partial;
        //            var read = reader.LoadAsync(250);
        //            read.Completed += (info, status) =>
        //            {

        //            };
        //            await socket.CancelIOAsync();
        //            socket.TransferOwnership(socketId);
        //            socket = null;
        //        }

        //}

        //public void InitializeConnection()
        //{

        //        foreach (var current in BackgroundTaskRegistration.AllTasks)
        //        {
        //            if (current.Value.Name == "SocketActivityBackgroundTask")
        //            {
        //                task = current.Value;
        //                break;
        //            }
        //        }

        //        // If there is no task allready created, create a new one
        //        if (task == null)
        //        {
        //            var socketTaskBuilder = new BackgroundTaskBuilder();
        //            socketTaskBuilder.Name = "SocketActivityBackgroundTask";
        //            socketTaskBuilder.TaskEntryPoint = "SocketActivityBackgroundTask.SocketActivityTask";
        //            var trigger = new SocketActivityTrigger();
        //            socketTaskBuilder.SetTrigger(trigger);
        //            task = socketTaskBuilder.Register();
        //        }

        //        SocketActivityInformation socketInformation;
        //        if (SocketActivityInformation.AllSockets.TryGetValue(socketId, out socketInformation))
        //        {
        //            // Application can take ownership of the socket and make any socket operation
        //            // For sample it is just transfering it back.
        //            socket = socketInformation.StreamSocket;
        //            socket.TransferOwnership(socketId);
        //            socket = null;
        //        }


        //}

        private Uri TryGetUri(string uriString)
        {
            Uri webSocketUri;
            if (!Uri.TryCreate(uriString.Trim(), UriKind.Absolute, out webSocketUri))
            {
                //NotifyUser("Error: Invalid URI", NotifyType.ErrorMessage);
                return null;
            }

            // Fragments are not allowed in WebSocket URIs.
            if (!String.IsNullOrEmpty(webSocketUri.Fragment))
            {
                //NotifyUser("Error: URI fragments not supported in WebSocket URIs.", NotifyType.ErrorMessage);
                return null;
            }

            // Uri.SchemeName returns the canonicalized scheme name so we can use case-sensitive, ordinal string
            // comparison.
            if ((webSocketUri.Scheme != "ws") && (webSocketUri.Scheme != "wss"))
            {
                //NotifyUser("Error: WebSockets only support ws:// and wss:// schemes.", NotifyType.ErrorMessage);
                return null;
            }

            return webSocketUri;
        }



    }
}
