using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ribbon_Trail.Objects
{
    class Method
    {
        public string name { get; private set; }
        public string returnType { get; private set; }
        public Class parentClass { get; private set; }
        public int indexInFile { get; private set; }

        public List<Method> parents = new List<Method>();
        public List<Method> children = new List<Method>();

        public Method(string name, string returnType, Class parentClass, int indexInFile)
        {
            this.name = name;
            this.returnType = returnType;
            this.parentClass = parentClass;
            this.indexInFile = indexInFile;
        }
    }
}
