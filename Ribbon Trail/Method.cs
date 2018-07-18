using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ribbon_Trail
{
    class Method
    {
        public string name;
        public List<Method> parents = new List<Method>();
        public List<Method> children = new List<Method>();

        public Method(string name)
        {
            this.name = name;
        }
    }
}
