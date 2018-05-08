using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentScheduler.Lib.ViewModel
{
    public class ServerViewModel
    {
        public int ServerId { get; set; }
        public string ServerName { get; set; }
        public string Configuration { get; set; }
        public string Status { get; set; }
    }
}
