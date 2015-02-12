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
    /// Enumerator of Titems from an enumerator of Streams.
    /// </summary>
    /// <typeparam name="Titem"></typeparam>
    public class StreamEnumeratorEnumerator<Titem> : IEnumerator<Titem>
    {
        private IObjectSerialization<Titem> objectSerialization;
        private IEnumerator<Stream> streamEnumerator;
        private Stream currentStream;
        private long currentStreamLength;
        private Titem current;

        public StreamEnumeratorEnumerator(IEnumerator<Stream> streamEnumerator, IObjectSerialization<Titem> objectSerialization)
        {
            this.streamEnumerator = streamEnumerator;
            this.objectSerialization = objectSerialization;
            currentStream = null;
            current = default(Titem);
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
                if (currentStream != null)
                {
                    currentStream.Dispose();
                }
                streamEnumerator.Dispose();
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
            while (true)
            {
                if (currentStream == null || currentStream.Position >= currentStreamLength)
                {
                    if (currentStream != null)
                    {
                        currentStream.Close();
                        currentStream = null;
                    }
                    if (!streamEnumerator.MoveNext())
                    {
                        return false;
                    }
                    currentStream = streamEnumerator.Current;
                    currentStreamLength = currentStream.Length;
                }
                else
                {
                    break;
                }
            }
            current = objectSerialization.Read(currentStream);
            return true;
        }

        public void Reset()
        {
            streamEnumerator.Reset();
            currentStream = null;
            current = default(Titem);
        }
    }
}
