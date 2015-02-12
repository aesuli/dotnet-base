// Copyright (C) 2013 Andrea Esuli
// http://www.esuli.it
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

namespace Esuli.Base.IO.Sort
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Sorts the elements from the enumerations using external sorting.
    /// </summary>
    public class ParallelMWayMergeSort
    {
        public static IEnumerator<Titem> MergeSort<Titem>(IEnumerator<Titem> items, int mergeWays, int sortProcesses, IObjectSerialization<Titem> objectSerialization, IComparer<Titem> comparer, int blockSize, int bufferSize, string tempFilePrefix)
        {
            List<FileInfo> blocks = SortIntoFileBlocks<Titem>(items, objectSerialization, sortProcesses, comparer, blockSize, bufferSize, tempFilePrefix);
            return MWayMergeSort.MergeFileBlocks<Titem>(blocks, objectSerialization, comparer, mergeWays, bufferSize, tempFilePrefix, true);
        }

        public static List<FileInfo> SortIntoFileBlocks<Titem>(IEnumerator<Titem> items, IObjectSerialization<Titem> objectSerialization, int sortProcesses, IComparer<Titem> comparer, int blockSize, int bufferSize, string tmpPrefix)
        {
            List<FileInfo> blocks = new List<FileInfo>();
            var sortingTasks = new Task<int>[sortProcesses];
            for (int i = 0; i < sortProcesses; ++i)
            {
                int j = i;
                sortingTasks[i] = Task<int>.Factory.StartNew(() =>
                {
                    Titem item;
                    List<Titem> block = new List<Titem>(blockSize);

                    bool end = false;
                    while (!end)
                    {
                        lock (items)
                        {
                            if (items.MoveNext())
                            {
                                item = items.Current;
                            }
                            else
                            {
                                end = true;
                                item = default(Titem);
                            }
                        }
                        if (!end)
                        {
                            block.Add(item);
                            if (block.Count == blockSize)
                            {
                                block.Sort(comparer);
                                FileInfo fileInfo;
                                lock (blocks)
                                {
                                    fileInfo = new FileInfo(tmpPrefix + "_" + blocks.Count);
                                    blocks.Add(fileInfo);
                                }
                                using (var blockStream = new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.SequentialScan))
                                {
                                    for (int k = 0; k < blockSize; ++k)
                                    {
                                        objectSerialization.Write(block[k], blockStream);
                                    }
                                }
                                block.Clear();
                            }
                        }
                    }
                    if (block.Count > 0)
                    {
                        int lastBlockSize = block.Count;
                        block.Sort(comparer);
                        FileInfo fileInfo;
                        lock (blocks)
                        {
                            fileInfo = new FileInfo(tmpPrefix + "_" + blocks.Count);
                            blocks.Add(fileInfo);
                        }
                        using (var blockStream = new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.SequentialScan))
                        {
                            for (int k = 0; k < lastBlockSize; ++k)
                            {
                                objectSerialization.Write(block[k], blockStream);
                            }
                        }
                        block.Clear();
                    }
                    return j;
                });
            }
            Task.WaitAll(sortingTasks);

            return blocks;
        }
    }
}
