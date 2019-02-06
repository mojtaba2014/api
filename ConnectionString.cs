using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NolekClient.Model
{
    public class ConnectionString
    {

        public string Database { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }

        public ConnectionString()
        {

        }

        public ConnectionString(string Database, string UserName, string PassWord)
        {
            this.Database = Database;
            this.UserName = UserName;
            this.PassWord = PassWord;
        }
    }
}
