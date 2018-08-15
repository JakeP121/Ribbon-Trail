using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ribbon_Trail.Objects
{
    class Class
    {
        public string name { get; private set; }
        public string fileDir { get; private set; }
        public int indexInFile { get; private set; }
        public Namespace nameSpace;

        public List<Variable> variables;
        public List<Method> methods;

        public Class(string name, Namespace nameSpace, string fileDir, int indexInFile)
        {
            this.name = name;
            this.fileDir = fileDir;
            this.nameSpace = nameSpace;
            this.indexInFile = indexInFile;
        }
    }
}
