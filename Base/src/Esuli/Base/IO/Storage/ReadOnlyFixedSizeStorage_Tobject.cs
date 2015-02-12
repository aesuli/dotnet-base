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

namespace Esuli.Base.IO.Storage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Esuli.Base.Enumerators;
    using Esuli.Base.IO;

    public class ReadOnlyFixedSizeStorage<Tobject>
        : IReadOnlyList<Tobject>, IDisposable
    {
        private Stream stream;
        private IFixedSizeObjectSerialization<Tobject> objectSerialization;
        private int count;
        private long offset;

        public ReadOnlyFixedSizeStorage(string name, string location)
        {
            stream = new FileStream(location + Path.DirectorySeparatorChar + name + Constants.StorageFileExtension, FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryReader binaryReader = new BinaryReader(stream);
            count = binaryReader.ReadInt32();
            objectSerialization = TypeSerialization.CreateInstance<IFixedSizeObjectSerialization<Tobject>>(stream);
            offset = stream.Position;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                stream.Dispose();
            }
        }

        public Tobject this[int id]
        {
            get
            {
                long newPosition = (((long)id) * objectSerialization.ObjectSize) + offset;
                if (stream.Position != newPosition)
                {
                    stream.Position = newPosition;
                }
                return objectSerialization.Read(stream);
            }
        }

        public int Count
        {
            get
            {
                return count;
            }
        }

        public IEnumerator<Tobject> GetEnumerator()
        {
            return new CustomSelectorEnumerator<ReadOnlyFixedSizeStorage<Tobject>, Tobject, int>(this, (collection, index) => collection[index],new IntRangeEnumerator(0, count));
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new CustomSelectorEnumerator<ReadOnlyFixedSizeStorage<Tobject>, Tobject, int>(this, (collection, index) => collection[index], new IntRangeEnumerator(0, count));
        }
    }
}
