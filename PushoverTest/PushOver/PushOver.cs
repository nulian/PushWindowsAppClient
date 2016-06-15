using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.ExchangeActiveSyncProvisioning;

namespace PushoverTest.PushOver
{
    class PushOver
    {
        private const string PUSH_OVER_URL = "https://api.pushover.net/1/";
        private Windows.Storage.ApplicationDataContainer localSettings;

        public PushOver()
        {
            localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        }

        public async Task<bool> RequestUserInfoAsync(string username, string password)
        {
            if (((string)localSettings.Values["id"]) == null)
            {
                using (var client = new HttpClient())
                {
                    var values = new Dictionary<string, string>
                    {
                        { "email", username },
                        { "password", password }
                    };

                    var content = new FormUrlEncodedContent(values);

                    var response = await client.PostAsync(PUSH_OVER_URL + "users/login.json", content);

                    var responseString = await response.Content.ReadAsStringAsync();

                    Dictionary<string, string> userData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

                    StoreUserData(userData);

                    return Int32.Parse(userData["status"]) == 1;
                }
            }
            return true;
        }

        public async Task<bool> RegisterDeviceAsync()
        {
            if (((string)localSettings.Values["device_id"]) == null)
            {
                using (var client = new HttpClient())
                {
                    var deviceInformation = new EasClientDeviceInformation();
                    var deviceName = deviceInformation.FriendlyName;

                    var formContent = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("secret", ((string)localSettings.Values["secret"])),
                        new KeyValuePair<string, string>("name", deviceName),
                        new KeyValuePair<string, string>("os", "O")
                    });

                    var response = await client.PostAsync(PUSH_OVER_URL + "devices.json", formContent);

                    var responseString = await response.Content.ReadAsStringAsync();

                    Dictionary<string, string> registrationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

                    localSettings.Values["device_id"] = registrationData["id"];

                    return Int32.Parse(registrationData["status"]) == 1;
                }
            }
            return true;
        }

        public async Task<Messages> RetrieveCurrentMessagesAsync()
        {
            using (var client = new HttpClient())
            {

                var response = await client.GetAsync(PUSH_OVER_URL + "messages.json?secret=" + localSettings.Values["secret"] + "&device_id=" + localSettings.Values["device_id"]);

                var responseString = await response.Content.ReadAsStringAsync();

                var userData = JsonConvert.DeserializeObject<Messages>(responseString);
                return userData;
            }
        }

        private void StoreUserData(Dictionary<string, string> userData)
        {
            localSettings.Values["id"] = userData["id"];
            localSettings.Values["secret"] = userData["secret"];
        }

    }
}
