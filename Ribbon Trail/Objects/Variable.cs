using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ribbon_Trail.Objects
{
    class Variable
    {
        public string name { get; private set; }
        public string type { get; private set; }

        public Variable(string name, string type)
        {
            this.name = name;
            this.type = type;
        }
    }
}
