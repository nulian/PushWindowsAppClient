using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PushoverTest.PushOver
{
    class MessageStore
    {
        public async Task StoreMessages(List<Message> messages)
        {
            StorageFile messageStorageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("MyMessages.db", CreationCollisionOption.ReplaceExisting);
            IRandomAccessStream raStream = await messageStorageFile.OpenAsync(FileAccessMode.ReadWrite);

            using (IOutputStream outStream = raStream.GetOutputStreamAt(0))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(List<Message>));
                serializer.WriteObject(outStream.AsStreamForWrite(), messages);
                await outStream.FlushAsync();
                outStream.Dispose();
                raStream.Dispose();
            }
        }

        public async Task<List<Message>> ReadStoredMessagesAsync()
        {
            List<Message> messages = new List<Message>();

            var serializer = new DataContractSerializer(typeof(List<Message>));
            using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync("MyMessages.db"))
            {
                messages = (List<Message>)serializer.ReadObject(stream);
            }
            return messages;
        }
    }
}
