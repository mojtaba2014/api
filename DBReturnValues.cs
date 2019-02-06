using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NolekClient.Model
{
    public class DBReturnValues
    {
        public List<List<string>> Rows;
        public List<string> ColumnNames;
        public List<string> Types;

        public string Result { get; set; }
        public DBReturnValues()
        {
            Rows = new List<List<string>>();
            ColumnNames = new List<string>();
            Types = new List<string>();
        }
    }
}
