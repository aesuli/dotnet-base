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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Esuli.Base.Enumerators;

    /// <summary>
    /// Sorts the elements from the enumerations using external sorting.
    /// </summary>
    public class MWayMergeSort
    {
        public static IEnumerator<Titem> MergeSort<Titem>(IEnumerator<Titem> items, int ways, IObjectSerialization<Titem> objectSerialization, IComparer<Titem> comparer, int blockSize, int bufferSize, string tempFilePrefix)
        {
            List<FileInfo> blocks = SortIntoFileBlocks<Titem>(items, objectSerialization, comparer, blockSize, bufferSize, tempFilePrefix);
            return MergeFileBlocks<Titem>(blocks, objectSerialization, comparer, ways, bufferSize, tempFilePrefix, true);
        }

        public static List<FileInfo> SortIntoFileBlocks<Titem>(IEnumerator<Titem> items, IObjectSerialization<Titem> objectSerialization, IComparer<Titem> comparer, int blockSize, int bufferSize, string tmpPrefix)
        {
            List<FileInfo> blocks = new List<FileInfo>();
            List<Titem> block = new List<Titem>(blockSize);
            while (items.MoveNext())
            {
                block.Add(items.Current);
                if (block.Count == blockSize)
                {
                    block.Sort(comparer);
                    FileInfo fileInfo = new FileInfo(tmpPrefix + "_" + blocks.Count);
                    blocks.Add(fileInfo);
                    using (var blockStream = new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.SequentialScan))
                    {
                        for (int i = 0; i < blockSize; ++i)
                        {
                            objectSerialization.Write(block[i], blockStream);
                        }
                    }
                    block.Clear();
                }
            }
            if (block.Count > 0)
            {
                blockSize = block.Count;
                block.Sort(comparer);
                FileInfo fileInfo = new FileInfo(tmpPrefix + "_" + blocks.Count);
                blocks.Add(fileInfo);
                using (var blockStream = new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.SequentialScan))
                {
                    for (int i = 0; i < blockSize; ++i)
                    {
                        objectSerialization.Write(block[i], blockStream);
                    }
                }
                block.Clear();
            }
            return blocks;
        }

        public static IEnumerator<Titem> MergeFileBlocks<Titem>(List<FileInfo> blocks, IObjectSerialization<Titem> objectSerialization, IComparer<Titem> comparer, int ways, int bufferSize, string tempFilePrefix, bool deleteOriginalBlocks)
        {
            if (blocks.Count == 1)
            {
                using (StreamEnumerator<Titem> streamEnumerator = new StreamEnumerator<Titem>(new FileStream(blocks[0].FullName, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize, FileOptions.SequentialScan), objectSerialization))
                {
                    while (streamEnumerator.MoveNext())
                    {
                        yield return streamEnumerator.Current;
                    }
                }
                if (deleteOriginalBlocks)
                {
                    blocks[0].Delete();
                }
            }
            else
            {
                while (true)
                {
                    int iterations = (int)Math.Ceiling(((double)blocks.Count) / ways);
                    if (iterations <= 1)
                    {
                        var itemEnumerators = new List<IEnumerator<Titem>>();
                        foreach (var fileInfo in blocks)
                        {
                            itemEnumerators.Add(new StreamEnumerator<Titem>(new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize, FileOptions.SequentialScan), objectSerialization));
                        }
                        using (var itemEnumerator = new MergedEnumerators<Titem>(itemEnumerators.ToArray(), comparer, true))
                        {
                            while (itemEnumerator.MoveNext())
                            {
                                yield return itemEnumerator.Current;
                            }
                        }
                        if (deleteOriginalBlocks)
                        {
                            foreach (FileInfo fileInfo in blocks)
                            {
                                fileInfo.Delete();
                            }
                        }
                        break;
                    }
                    else
                    {
                        List<FileInfo> iterBlocks = new List<FileInfo>(iterations);
                        tempFilePrefix += "_r";
                        for (int iter = 0; iter < iterations; ++iter)
                        {
                            List<FileInfo> tempBlocks = new List<FileInfo>(ways);
                            int baseIdx = iter * ways;
                            int endIdx = (iter + 1) * ways;
                            if (endIdx > blocks.Count)
                            {
                                endIdx = blocks.Count;
                            }
                            for (int i = baseIdx; i < endIdx; ++i)
                            {
                                tempBlocks.Add(blocks[i]);
                            }
                            FileInfo iterBlock = new FileInfo(tempFilePrefix + "_" + iter);
                            iterBlocks.Add(iterBlock);

                            var itemEnumerators = new List<IEnumerator<Titem>>();
                            foreach (var fileInfo in tempBlocks)
                            {
                                itemEnumerators.Add(new StreamEnumerator<Titem>(new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize, FileOptions.SequentialScan), objectSerialization));
                            }
                            using (IEnumerator<Titem> itemEnumerator = new MergedEnumerators<Titem>(itemEnumerators.ToArray(), comparer, true))
                            using (Stream stream = new FileStream(iterBlock.FullName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.SequentialScan))
                            {
                                while (itemEnumerator.MoveNext())
                                {
                                    objectSerialization.Write(itemEnumerator.Current, stream);
                                }
                            }

                            if (deleteOriginalBlocks)
                            {
                                foreach (FileInfo fileInfo in tempBlocks)
                                {
                                    fileInfo.Delete();
                                }
                            }
                        }

                        blocks = iterBlocks;
                        deleteOriginalBlocks = true;
                    }
                }
            }
        }
    }
}
