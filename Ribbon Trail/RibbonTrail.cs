using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ribbon_Trail.Objects;

namespace Ribbon_Trail
{
    public class Human
    { }

    public class Jake:Human
    { }


    static class RibbonTrail
    {
        public static string dir; // The directory containing the files to search through

        private static Language? language; // The programming language of files to search through
        private static string targetExtension; // The file extension of the programming language chosen

        private static List<string> files = new List<string>(); // All files to search through

        private static Dictionary<string, Namespace> namespaces = new Dictionary<string, Namespace>();
        private static List<string> namespaceKeys = new List<string>();

        public static void Start()
        {
            if (dir == "" || language == null)
                return;

            getTargetFiles();

            foreach (string f in files)
            {
                List<Namespace> foundNamespaces = FileScan.findClasses(f);

                foreach(Namespace n in foundNamespaces)
                {
                    if (namespaces.ContainsKey(n.name))
                        namespaces[n.name].merge(n);
                    else
                    {
                        namespaceKeys.Add(n.name);
                        namespaces.Add(n.name, n);
                    }
                }
            }

            foreach (string n in namespaceKeys)
            {
                foreach (Class c in namespaces[n].classes)
                    c.methods = FileScan.findMethods(c);
            }

            print();
            return;
        }

        /// <summary>
        /// Populates files array with all files of chosen language within dir and all sub directories 
        /// </summary>
        private static void getTargetFiles()
        {
            foreach (string f in Directory.GetFiles(dir, "*" + targetExtension, SearchOption.AllDirectories)) 
                files.Add(f);

            return;
        }

        /// <summary>
        /// Sets the filter of files to search through to one programming language
        /// </summary>
        /// <param name="lang">The programming language of the files to search</param>
        public static void setLanguage(Language lang)
        {
            language = lang;

            switch (language)
            {
                case (Language.C):
                    targetExtension = ".c";
                    break;
                case (Language.CPP):
                    targetExtension = ".cpp";
                    break;
                case (Language.CSHARP):
                    targetExtension = ".cs";
                    break;
            }
        }

        /// <summary>
        /// Finds the first valid character after a functions argument list
        /// </summary>
        /// <param name="fileContents">The file this function was found</param>
        /// <param name="openingBracketLoc">The index location of the opening bracket of this function</param>
        /// <returns>The first char after this method's closing bracket</returns>
        private static char findCharAfterArgumentList(string fileContents, int openingBracketLoc)
        {
            int j = openingBracketLoc + 1;
            bool closingBracketFound = false;

            // Step forward until hit end of file or closing bracket found
            while (j <= fileContents.Length && !closingBracketFound)
            {
                if (fileContents[j] == ')')
                    closingBracketFound = true;

                j++;
            }

            return fileContents[j];
        }

        private static void print()
        {
            string file = dir + "\\RibbonTrail.xml";
            File.WriteAllText(file, "");

            foreach (string n in namespaceKeys)
            {
                foreach (Class c in namespaces[n].classes)
                {
                    File.AppendAllText(file, "<CLASS>\n");
                    File.AppendAllText(file, "\t<NAMESPACE>" + c.nameSpace.name + "</NAMESPACE>\n");
                    File.AppendAllText(file, "\t<NAME>" + c.name + "</NAME>\n");

                    foreach (Method m in c.methods)
                    {
                        File.AppendAllText(file, "\t\t<METHOD>" + m.name + "\n");
                        File.AppendAllText(file, "\t\t\t<RETURNS>" + m.returnType + "</RETURNS>\n");
                        File.AppendAllText(file, "\t\t</METHOD>\n");
                    }

                    File.AppendAllText(file, "</CLASS>\n\n");
                }
            }
        }
    }
}
