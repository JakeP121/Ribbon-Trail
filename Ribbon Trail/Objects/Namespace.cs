using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ribbon_Trail.Objects
{
    class Namespace
    {
        public string name { get; private set; }
        public List<Class> classes = new List<Class>();

        public Namespace(string name)
        {
            this.name = name;
        }

        public void merge(Namespace mergingNamespace)
        {
            if (mergingNamespace.name != name)
                return;

            foreach (Class c in mergingNamespace.classes)
            {
                if (!classes.Contains(c))
                {
                    c.nameSpace = this;
                    classes.Add(c);
                }
            }
        }
    }
}
