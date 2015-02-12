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
    using System.Diagnostics;
    using System.IO;
    using Esuli.Base.IO;

    public class SequentialWriteOnlyFixedSizeStorage<Tobject, TobjectSerialization>
         : ISequentiallyWriteableStorage<Tobject>
         where TobjectSerialization : IFixedSizeObjectSerialization<Tobject>, new()
    {
        private Stream stream;
        private IFixedSizeObjectSerialization<Tobject> objectSerialization;
        private int count;
        private bool modified;
        private long offset;

        public SequentialWriteOnlyFixedSizeStorage(string name, string location)
            : this(name, location, false)
        {
        }

        public SequentialWriteOnlyFixedSizeStorage(string name, string location, bool overwrite)
        {
            stream = new FileStream(location + Path.DirectorySeparatorChar + name + Constants.StorageFileExtension, overwrite ? FileMode.Create : FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            if (stream.Length > 0)
            {
                BinaryReader binaryReader = new BinaryReader(stream);
                count = binaryReader.ReadInt32();
                objectSerialization = TypeSerialization.CreateInstance<IFixedSizeObjectSerialization<Tobject>>(stream);
                offset = stream.Position;
                modified = false;
            }
            else
            {
                count = 0;
                BinaryWriter binaryWrite = new BinaryWriter(stream);
                binaryWrite.Write(count);
                objectSerialization = new TobjectSerialization();
                TypeSerialization.Write(objectSerialization.GetType(), stream);
                offset = stream.Position;
                modified = false;
            }
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
                Close();
                stream.Dispose();
            }
        }

        private void Close()
        {
            if (modified)
            {
                stream.Position = 0;
                BinaryWriter binaryWrite = new BinaryWriter(stream);
                binaryWrite.Write(count);
                modified = false;
            }
            stream.Close();
            count = 0;
            offset = 0;
            objectSerialization = null;
        }

        public int Write(Tobject obj)
        {
            int id = count;
            Debug.Assert(stream.Position == (((long)id) * objectSerialization.ObjectSize) + offset);
            objectSerialization.Write(obj, stream);
            ++count;
            modified = true;
            return id;
        }

        public int Count
        {
            get
            {
                return count;
            }
        }
    }
}
