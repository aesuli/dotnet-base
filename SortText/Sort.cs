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

namespace Esuli.SortText
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using Esuli.Base;
    using Esuli.Base.Collections;
    using Esuli.Base.IO;
    using Esuli.Base.IO.Sort;

    /// <summary>
    /// This application uses external memory sort to implement line-by-line sort of text.
    /// It is very similar to the Unix sort utility.
    /// </summary>
    class Sort
    {
        public static void Usage()
        {
            Console.Error.WriteLine("Usage: Sort [-z] [-m <mergeWayCount>] [-t <tempFilesPath>] [<directory/file list>+]");
            Console.Error.WriteLine("Note:  All the arguments are optional. The order of arguments is fixed.");
            Console.Error.WriteLine("Note:  -z means that the input is gzipped.");
            Console.Error.WriteLine("Note:  If no directories/files are given, the program will use stdin as input.");
        }

        public static void Main(string[] args)
        {
            bool gzipped = false;
            int mergeWays = 16;
            string tempFilePath = Path.GetTempPath();
            int argShift = 0;
            if (args.Length > 0)
            {
                if (args[0] == "-h" || args[0] == "--help")
                {
                    Usage();
                    return;
                }
                if (args[0] == "-z")
                {
                    gzipped = true;
                    argShift = 1;
                }
                if (args[0 + argShift] == "-m")
                {
                    mergeWays = int.Parse(args[1 + argShift]);
                    argShift += 2;
                }
                if (args[0 + argShift] == "-t")
                {
                    tempFilePath = args[1 + argShift];
                    argShift += 2;
                }
            }

            IEnumerator<string> lineEnumerator;
            if (args.Length - argShift > 0)
            {
               lineEnumerator = ProcessArguments(args, argShift, gzipped);
            }
            else
            {
                if (gzipped)
                {
                    lineEnumerator = ProcessStreamReader(new StreamReader(new GZipStream(Console.OpenStandardInput(), CompressionMode.Decompress, false)));
                }
                else
                {
                    lineEnumerator = ProcessStreamReader(new StreamReader(Console.OpenStandardInput()));
                }
            }

            IObjectSerialization<string> objectSerialization = new StringSerialization();
            IComparer<string> comparer = new OrdinalStringComparer();
            int blockSize = Constants.OneMega;
            int bufferSize = 10*Constants.OneMebi;
            var tempFilePrefix = tempFilePath + Path.DirectorySeparatorChar + "sortBlock_" + DateTime.Now.Ticks;
            int sortProcesses = Environment.ProcessorCount;
            using (IEnumerator<string> sorted = ParallelMWayMergeSort.MergeSort<string>(lineEnumerator, mergeWays, sortProcesses, objectSerialization, comparer, blockSize, bufferSize, tempFilePrefix))
            {
                while (sorted.MoveNext())
                {
                    Console.WriteLine(sorted.Current);
                }
            }
        }

        private static IEnumerator<string> ProcessArguments(string[] args, int argShift, bool gzipped)
        {
            List<FileInfo> fileinfos = new List<FileInfo>();
            for (int i = argShift; i < args.Length; ++i)
            {
                if (File.Exists(args[i]))
                {
                    var enumerator = ProcessFile(new FileInfo(args[i]), gzipped);
                    while (enumerator.MoveNext())
                    {
                        yield return enumerator.Current;
                    }
                }
                else if (Directory.Exists(args[i]))
                {
                    var enumerator = ProcessDirectory(new DirectoryInfo(args[i]), gzipped);
                    while (enumerator.MoveNext())
                    {
                        yield return enumerator.Current;
                    }
                }
            }
        }

        static IEnumerator<string> ProcessDirectory(DirectoryInfo directoryInfo, bool gzipped)
        {
            foreach (FileInfo fileInfo in directoryInfo.GetFiles())
            {
                var enumerator = ProcessFile(fileInfo, gzipped);
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
            foreach (DirectoryInfo subDirectoryInfo in directoryInfo.GetDirectories())
            {
                var enumerator = ProcessDirectory(subDirectoryInfo, gzipped);
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
        }

        static IEnumerator<string> ProcessFile(FileInfo fileInfo, bool gzipped)
        {
            if (gzipped)
            {
                var stream = new StreamReader(new GZipStream(fileInfo.OpenRead(), CompressionMode.Decompress, false));
                return ProcessStreamReader(stream);
            }
            else
            {
                var stream = fileInfo.OpenText();
                return ProcessStreamReader(stream);
            }
        }

        static IEnumerator<string> ProcessStreamReader(StreamReader stream)
        {
            string line;
            while ((line = stream.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }
}