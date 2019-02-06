using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NolekClient.Model
{
    public class DynamicClass
    {
        public string Type { get; set; }

        public string Value { get; set; }

        public string Name { get; set; }

        public DynamicClass(string Value, string Type, string Names)
        {
            this.Value = Value;
            this.Type = Type;
            this.Name = Names;
        }

        public DynamicClass()
        {

        }
    }
}
