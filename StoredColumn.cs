using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NolekClient.Model
{
    public class StoredColumn
    {
        public string Name { get; set; }
        public List<string> ColumnNames { get; set; }
        public List<string> ColumnTypes { get; set; }

        public StoredColumn()
        {
            ColumnNames = new List<string>();
            ColumnTypes = new List<string>();
        }
    }
}
