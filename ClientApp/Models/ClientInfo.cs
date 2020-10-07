using System;
using System.Collections.Generic;
using System.Text;
using ClientApp.Utils;

namespace ClientApp.Models
{
    class ClientInfo : ObservableObject
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
