using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushoverTest.PushOver
{
    class Message
    {
        public int id { get; set; }
        public string message { get; set; }
        public string app { get; set; }
        public int aid { get; set; }
        public string icon { get; set; }
        public int date { get; set; }
        public int priority { get; set; }
        public int acked { get; set; }
        public int umid { get; set; }
        public string title { get; set; }
    }
}
