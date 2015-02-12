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

namespace Esuli.Base.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Enumerator of Titems from a Stream.
    /// </summary>
    /// <typeparam name="Titem"></typeparam>
    public class StreamEnumerator<Titem> : IEnumerator<Titem>
    {
        private IObjectSerialization<Titem> objectSerialization;
        private Stream stream;
        private long initialPosition;
        private Titem current;
        private long lenght;

        public StreamEnumerator(Stream stream, IObjectSerialization<Titem> objectSerialization)
        {
            this.stream = stream;
            this.objectSerialization = objectSerialization;
            initialPosition = stream.Position;
            current = default(Titem);
            lenght = stream.Length;
        }

        public Titem Current
        {
            get
            {
                return current;
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
                stream.Dispose();
            }
        }

        object System.Collections.IEnumerator.Current
        {
            get
            {
                return current;
            }
        }

        public bool MoveNext()
        {
            if (stream.Position>=lenght)
            {
                return false;
            }
            current = objectSerialization.Read(stream);
            return true;
        }

        public void Reset()
        {
            stream.Position = initialPosition;
            current = default(Titem);
        }
    }
}
