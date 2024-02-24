using System;
using System.CommandLine;
using System.Text.RegularExpressions;

namespace Finder
{
    class Program
    {
        static int Main(string[] args)
        {
            int errorValue = -1;
            if (args.Length == 1 && (args[0] == "--help" || args[0] == "-h"))
            {
                PrintHelp();
                return 0;
            }
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: citation_finder.exe <file>");
                return errorValue;
            }
            string fileName = args[0];
            if (!System.IO.File.Exists(fileName))
            {
                Console.WriteLine($"File '{fileName}' not found.");
                return errorValue;
            }
            Finding.Find(fileName);
            return 0;
        }
        static void PrintHelp()
        {
            Console.WriteLine("Usage: citation_finder.exe <file>");
            Console.WriteLine("Arguments:");
            Console.WriteLine("  <file>  Path to the file to be processed.");
            Console.WriteLine("Options:");
            Console.WriteLine("  --help, -h  Show this help message and exit.");
        }
    }

    // interface should be something with the db.
    // as in, another class calls the FindInFile method using a path for one file

    static class Finding
    {
        static int maxPathSearchDepth = 5;
        static int maxNumFiles = 100;

        public static void Find(string path)
        {
            // init the book tree


            string[] files = WalkPath(path, maxPathSearchDepth);
            foreach (string file in files)
            {
                string[] refs = FindInFile(file);
                if (refs.Length == 0)
                {
                    Console.WriteLine($"No references found in {file}");
                    continue;
                }
                foreach (string reference in refs)
                {
                    Console.WriteLine(reference);
                }
            }
        }

        static string[] WalkPath(string root, int depth)
        {
            List<string> filePaths = new List<string>();
            if (System.IO.File.Exists(root))
            {
                filePaths.Add(root);
            }
            else
            {
                string[] files = Directory.GetFiles(root);
                filePaths.AddRange(files);

                if (depth > 0)
                {
                    string[] subdirectories = Directory.GetDirectories(root);
                    foreach (string subdir in subdirectories)
                    {
                        string[] subdirectoryFiles = WalkPath(subdir, depth - 1);
                        filePaths.AddRange(subdirectoryFiles);
                        if (filePaths.Count > maxNumFiles)
                        {
                            Console.WriteLine($"Only searching the first {maxNumFiles} files.");
                            break;
                        }
                    }
                }
            }

            if (filePaths.Count > maxNumFiles)
            {
                filePaths.RemoveRange(maxNumFiles, filePaths.Count - maxNumFiles);
            }
            return filePaths.ToArray();
        }

        static string[] FindInFile(string file_path)
        {
            string file_contents = System.IO.File.ReadAllText(file_path);
            if (file_contents.Length == 0)
            {
                Console.WriteLine("File is empty.");
                return new string[0];
            }
            return FindReferences(file_contents);
        }
        static string[] FindReferences(string file_contents)
        {
            string[] refs = FindRefsDirectWithColon(file_contents);
            // next, tokenize the references and check the tokens for validity
            return refs;
        }
        static string[] FindRefsDirectWithColon(string file_contents)
        {
            string pattern = @"\b\w+\.*\s*\d+:\d+[\W0-9]*(?=[a-zA-Z\n\t])"; // eg. Mt.16:24 - 17:2
            MatchCollection matches = Regex.Matches(file_contents, pattern);
            for (int i = 0; i < matches.Count; i++)
            {
                Console.WriteLine(matches[i].Value);
            }
            return null;
        }
    }  
}
            