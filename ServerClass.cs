using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NolekClient.Model
{
    public class ServerClass
    {
        public string ServerName { get; set; }

        public ConnectionString connectionString { get; set; }

        public ServerClass(string ServerName, ConnectionString connectionString)
        {
            this.ServerName = ServerName;
            this.connectionString = connectionString;
        }
    }
}
