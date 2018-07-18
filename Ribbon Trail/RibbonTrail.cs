using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ribbon_Trail
{
    static class RibbonTrail
    {
        public static string dir; // The directory containing the files to search through

        private static Language? language; // The programming language of files to search through
        private static string targetExtension; // The file extension of the programming language chosen

        private static List<string> files = new List<string>(); // All files to search through

        private static Dictionary<string, Method> methods = new Dictionary<string, Method>(); // All methods detected

        private static List<Char> ignoreCases = new List<char>(); // Characters to ignore when searching for a method name (spaces, line breaks, etc.)
        private static List<Char> invalidNameCharacters = new List<char>(); // Characters that can't be part of a method name 
        private static List<string> statements = new List<string>(); // Words that may precede an open bracket that aren't methods (if's, loops, etc.)

        static RibbonTrail()
        {
            ignoreCases.Add(' ');
            ignoreCases.Add('\n');
            ignoreCases.Add('\r');

            invalidNameCharacters.Add(' ');
            invalidNameCharacters.Add('{');
            invalidNameCharacters.Add('}');
            invalidNameCharacters.Add(')');
            invalidNameCharacters.Add('(');
            invalidNameCharacters.Add('[');
            invalidNameCharacters.Add(']');
            invalidNameCharacters.Add('=');
            invalidNameCharacters.Add('+');
            invalidNameCharacters.Add('-');
            invalidNameCharacters.Add('*');
            invalidNameCharacters.Add('/');

            statements.Add("if");
            statements.Add("for");
            statements.Add("foreach");
            statements.Add("while");
            statements.Add("&&");
            statements.Add("||");
            statements.Add("switch");
            statements.Add("case");
        }

        public static void Start()
        {
            if (dir == "" || language == null)
                return;

            getTargetFiles();

            foreach (string f in files)
                scanFile(f);

            printTrail();
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
        /// Scans a file and collects function data
        /// </summary>
        /// <param name="file">The file to scan</param>
        private static void scanFile(string file)
        {
            string contents = File.ReadAllText(file);
            int indent = 0;
            string parentMethod = null;

            for (int i = 0; i < contents.Length; i++)
            {
                if (contents[i] == '{' && parentMethod != null)
                    indent++;
                else if (contents[i] == '}')
                {
                    indent--;

                    if (indent == 0)
                        parentMethod = null;
                }
                else if (contents[i] == '(')
                {
                    // If not yet inside a function, will be a parent
                    if (parentMethod == null)
                    {
                        char charAfterArguments = findCharAfterArgumentList(contents, i);

                        if (charAfterArguments != ';') // If function found was not a constructor, add it to list
                        {
                            string methodName = findMethodName(contents, i);

                            if (methodName != null)
                            {
                                parentMethod = methodName;
                                addMethod(parentMethod);
                            }
                        }
                    }
                    else
                    {
                        string currentMethod = findMethodName(contents, i);

                        if (currentMethod != null)
                        {
                            addMethod(currentMethod);

                            methods[parentMethod].children.Add(methods[currentMethod]);
                            methods[currentMethod].parents.Add(methods[parentMethod]);
                        }
                    }
                }


            }
        }

        /// <summary>
        /// Finds the name of a function located in a file
        /// </summary>
        /// <param name="fileContents">The file this function was found</param>
        /// <param name="openingBracketLoc">The index location of the opening bracket of this function</param>
        /// <returns>The name of the function</returns>
        private static string findMethodName(string fileContents, int openingBracketLoc)
        {
            int j = openingBracketLoc - 1;

            // Keep stepping backwards if fileContents[j] is a character we should ignore (spaces or returns)
            while (ignoreCases.Contains(fileContents[j]))
                j--;

            // End early if preceded by an invalid character
            if (invalidNameCharacters.Contains(fileContents[j]))
                return null;

            // End early if comment rather than code
            if (checkIfComment(fileContents, j))
                return null;

            // Step backwards until first letter of name found or start of file hit
            while (j >= 0 && !invalidNameCharacters.Contains(fileContents[j - 1]))
                j--;


            string methodName = "";
            
            // Step forward to start building up methodName
            while (j < openingBracketLoc && !invalidNameCharacters.Contains(fileContents[j]))
            {
                methodName += fileContents[j];
                j++;
            }

            if (statements.Contains(methodName))
                return null;
            else
                return methodName;
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

        /// <summary>
        /// Adds a method to the method list if it is not there already
        /// </summary>
        /// <param name="name">The name of the method</param>
        private static void addMethod(string name)
        {
            if (!methods.ContainsKey(name))
                methods.Add(name, new Method(name));
        }

        /// <summary>
        /// Checks if part of a file is a code comment
        /// </summary>
        /// <param name="fileContents">The file to check</param>
        /// <param name="loc">The index location to check</param>
        /// <returns>true if comment, false if code</returns>
        private static bool checkIfComment(string fileContents, int loc)
        {
            int i = loc;

            while (i > 0)
            {
                // new line, not comment, return false
                if (fileContents[i] == '\r' || fileContents[i] == '\n')
                    return false;

                // Comment, return true
                if (fileContents[i] == '/' && (fileContents[i - 1] == '/' || fileContents[i-1] == '*'))
                    return true;
                
                i--;
            }

            return false;
        }

        /// <summary>
        /// Prints all found methods into RibbonTrail.xml file in dir 
        /// </summary>
        private static void printTrail()
        {
            string file = dir + "\\RibbonTrail.xml";

            File.WriteAllText(file, ""); // Create new or overwrite old file

            foreach (var element in methods)
            {
                Method method = element.Value;

                File.AppendAllText(file, "<METHOD>\n");

                File.AppendAllText(file, "\t<NAME> " + method.name + " </NAME>\n");

                foreach (Method p in method.parents)
                    File.AppendAllText(file, "\t\t<PARENT> " + p.name + " </PARENT>\n");

                foreach (Method c in method.children)
                    File.AppendAllText(file, "\t\t<CHILD> " + c.name + " </CHILD>\n");

                File.AppendAllText(file, "</METHOD>");

                File.AppendAllText(file, "\n\n");
            }

            
        }
    }
}
