using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushoverTest.PushOver
{
    class Messages
    {
        public int status { get; set; }
        public string request { get; set; }
        public List<Message> messages { get; set; }
    }
}
