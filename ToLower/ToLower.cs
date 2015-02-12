// Copyright (C) 2013 Andrea Esuli (andrea@esuli.it)
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace Esuli.ToLower
{
    using System;
    using System.IO;
    
    /// <summary>
    /// This small app converts to lower case any text given as input via stdin or list of file and directories.
    /// Directories will be processed recursively and any file contained in them will be processed (i.e., without
    /// checking if it is a text file or not).
    /// </summary>
    class ToLower
    {
        public static void Usage()
        {
            Console.Error.WriteLine("Usage: ToLower [<directory/file list>+]");
            Console.Error.WriteLine("Note:  If no arguments are given, the program will use stdin as input.");
        }

        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "-h" || args[0] == "--help")
                {
                    Usage();
                    return;
                }
            }

            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; ++i)
                {
                    if (File.Exists(args[i]))
                    {
                        IndexFile(new FileInfo(args[i]));
                    }
                    else if (Directory.Exists(args[i]))
                    {
                        IndexDirectory(new DirectoryInfo(args[i]));
                    }
                }
            }
            else
            {
                IndexStream(Console.In);
            }
        }

        static void IndexDirectory(DirectoryInfo directoryInfo)
        {
            foreach (FileInfo fileInfo in directoryInfo.GetFiles())
            {
                IndexFile(fileInfo);
            }
            foreach (DirectoryInfo dis in directoryInfo.GetDirectories())
            {
                IndexDirectory(dis);
            }
        }

        static void IndexFile(FileInfo fileInfo)
        {
            StreamReader reader = File.OpenText(fileInfo.FullName);
            IndexStream(reader);
            reader.Close();
        }

        static void IndexStream(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine(line.ToLowerInvariant());
            }
        }
    }
}