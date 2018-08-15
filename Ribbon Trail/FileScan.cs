using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ribbon_Trail.Objects;

namespace Ribbon_Trail
{
    static class FileScan
    {
        private static List<Char> ignoreCases = new List<char>(); // Characters to ignore when searching for a method name (spaces, line breaks, etc.)
        private static List<Char> invalidNameCharacters = new List<char>(); // Characters that can't be part of a method name 
        private static List<string> statements = new List<string>(); // Words that may precede an open bracket that aren't methods (if's, loops, etc.)

        private static Namespace currentNamespace = null; // The namespace that the index is currently in.
        private static Class currentClass = null; // The class that the index is currently in.
        private static Method currentMethod = null; // The method that the index is currently in.

        private static int namespaceIndent = 0; // How many indents the index is into the current class (resets currentNamespace when returned to 0)
        private static int classIndent = 0; // How many indents the index is into the current class (resets currentClass when returned to 0)
        private static int methodIndent = 0; // How many indents the index is into the current method (resets currentMethod when returned to 0)

        static FileScan()
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
            invalidNameCharacters.Add(':');

            statements.Add("if");
            statements.Add("for");
            statements.Add("foreach");
            statements.Add("while");
            statements.Add("&&");
            statements.Add("||");
            statements.Add("switch");
            statements.Add("case");
        }

        /// <summary>
        /// Resets all necessary variables for next run
        /// </summary>
        private static void reset()
        {
            currentNamespace = null;
            currentClass = null;
            namespaceIndent = 0;
            classIndent = 0;
        }

        /// <summary>
        /// Scans a file for all classes and namespaces
        /// </summary>
        /// <param name="file">The file to scan</param>
        /// <returns>A list of namespaces, which in turn hold the classes</returns>
        public static List<Namespace> findClasses(string file)
        {
            reset();

            List<Namespace> namespaces = new List<Namespace>();
            List<Class> classes = new List<Class>();
            string contents = File.ReadAllText(file);
            int index = 0;

            foreach (char c in contents)
            {
                if (c == '{')
                {
                    if (!isComment(contents, index))
                    {
                        if (currentClass != null)
                        {
                            increaseIndent();
                        }
                        else
                        {
                            string[] keyWords = new string[] { "class", "namespace", "enum" };
                            List<string> words = new List<string>();

                            int nameStart = index - 1;
                            string currentWord = findPreviousWord(contents, nameStart, out nameStart);

                            while (words.Count == 0 || (!statements.Contains(currentWord) && !keyWords.Contains(currentWord)))
                            {
                                words.Add(currentWord);

                                currentWord = findPreviousWord(contents, nameStart, out nameStart);
                            }

                            string type = currentWord;

                            switch (type)
                            {
                                case ("namespace"):
                                    string name = "";
                                    for (int i = words.Count - 1; i >= 0; i--)
                                    {
                                        name += words[i];

                                        if (i != 0)
                                            name += '.';
                                    }

                                    currentNamespace = new Namespace(name);
                                    namespaces.Add(currentNamespace);
                                    break;

                                case ("class"):
                                    Class newClass = new Class(words.Last(), currentNamespace, file, nameStart + currentWord.Length + 1);

                                    if (currentNamespace != null)
                                        currentNamespace.classes.Add(newClass);

                                    classes.Add(newClass);
                                    currentClass = classes.Last();
                                    break;
                            }

                            increaseIndent();
                        }
                    }
                }
                else if (c == '}')
                {
                    if (!isComment(contents, index) && currentClass != null)
                    {
                        decreaseIndent();
                    }
                }



                index++;
            }

            return namespaces;
        }

        public static List<Method> findMethods(Class inputClass)
        {
            reset();

            string contents = File.ReadAllText(inputClass.fileDir);
            int index = inputClass.indexInFile;

            currentClass = inputClass;
            List<Method> foundMethods = new List<Method>();

            while (contents[index] != '{') // Step forward until start of function reached
                index++;

            increaseIndent();
            index++;

            for (; index < contents.Length; index++)
            {
                char c = contents[index];

                if (currentClass != inputClass) // Return early if end of class reached
                    return foundMethods;

                if (c == '(')
                {
                    if (currentMethod == null)
                    {
                        int nameStart = -1;
                        string name = findPreviousWord(contents, index, out nameStart);
                        string returnType = findPreviousWord(contents, nameStart, out int ignore);

                        if (returnType != "new") // If the return type is new then it is not a method
                        {

                            int openBracketLoc = index;
                            while (contents[openBracketLoc] != '{')
                                openBracketLoc++;

                            currentMethod = new Method(name, returnType, currentClass, openBracketLoc);
                            foundMethods.Add(currentMethod);

                            index = openBracketLoc;
                            increaseIndent();
                        }
                    }
                }
                else if (c == '{')
                {
                    increaseIndent();
                }
                else if (c == '}')
                {
                    decreaseIndent();
                }
            }

            return foundMethods;
        }

        /// <summary>
        /// Increases the level of indent 
        /// </summary>
        private static void increaseIndent()
        {
            if (currentNamespace != null)
                namespaceIndent++;

            if (currentClass != null)
                classIndent++;

            if (currentMethod != null)
                methodIndent++;
        }

        /// <summary>
        /// Decreases the level of indent
        /// </summary>
        private static void decreaseIndent()
        {
            namespaceIndent--;
            classIndent--;
            methodIndent--;

            if (namespaceIndent <= 0)
            {
                currentNamespace = null;
                namespaceIndent = 0;
            }

            if (classIndent <= 0)
            {
                currentClass = null;
                classIndent = 0;
            }

            if (methodIndent <= 0)
            {
                currentMethod = null;
                methodIndent = 0;
            }
        }

        /// <summary>
        /// Finds the name of a function, class or namespace located in a file
        /// </summary>
        /// <param name="fileContents">The file this function was found.</param>
        /// <param name="endIndex">The index location of the first character after this name.</param>
        /// <param name="wordStartIndex">The index location of the start of the name.</param>
        /// <returns>The name of the function</returns>
        private static string findPreviousWord(string fileContents, int endIndex, out int wordStartIndex)
        {
            int i = endIndex - 1;
            wordStartIndex = i;

            // Keep stepping backwards if fileContents[i] is a character we should ignore (spaces or returns)
            while (ignoreCases.Contains(fileContents[i]))
                i--;

            // End early if preceded by an invalid character
            if (invalidNameCharacters.Contains(fileContents[i]))
                return null;

            // End early if comment rather than code
            if (isComment(fileContents, i))
                return null;

            // Step backwards until first letter of name found or start of file hit
            while (i >= 0 && !invalidNameCharacters.Contains(fileContents[i - 1]) && !ignoreCases.Contains(fileContents[i - 1]))
                i--;

            wordStartIndex = i;
            string name = "";

            // Step forward to start building up methodName
            while (i < endIndex && !invalidNameCharacters.Contains(fileContents[i]) && !ignoreCases.Contains(fileContents[i]))
            {
                name += fileContents[i];
                i++;
            }

            if (statements.Contains(name))
                return null;
            else
                return name;
        }

        /// <summary>
        /// Checks if part of a file is a code comment
        /// </summary>
        /// <param name="fileContents">The file to check</param>
        /// <param name="loc">The index location to check</param>
        /// <returns>true if comment, false if code</returns>
        private static bool isComment(string fileContents, int loc)
        {
            int i = loc;

            while (i > 0)
            {
                // new line, not comment, return false
                if (fileContents[i] == '\r' || fileContents[i] == '\n')
                    return false;

                // Comment, return true
                if (fileContents[i] == '/' && (fileContents[i - 1] == '/' || fileContents[i + 1] == '*'))
                    return true;

                //if (fileContents[i] == '/' && (fileContents[i - 1] == '/' || fileContents[i-1] == '*'))
                //    return true;

                i--;
            }

            return false;
        }
    }
}
