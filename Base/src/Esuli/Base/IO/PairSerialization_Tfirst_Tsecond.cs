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
    using System.Collections.Generic;
    using System.IO;
    
    /// <summary>
    /// Interface for a class that implements the methods to read and write pairs of Tfirst, Tsecond objects from a stream.
    /// </summary>
    /// <typeparam name="Tfirst"></typeparam>
    /// <typeparam name="Tsecond"></typeparam>
    public class PairSerialization<Tfirst, Tsecond> : IObjectSerialization<KeyValuePair<Tfirst, Tsecond>>
    {
        private IObjectSerialization<Tfirst> firstSerialization;
        private IObjectSerialization<Tsecond> secondSerialization;

        public PairSerialization(IObjectSerialization<Tfirst> firstSerialization, IObjectSerialization<Tsecond> secondSerialization)
        {
            this.firstSerialization = firstSerialization;
            this.secondSerialization = secondSerialization;
        }

        public void Write(KeyValuePair<Tfirst, Tsecond> obj, Stream stream)
        {
            firstSerialization.Write(obj.Key, stream);
            secondSerialization.Write(obj.Value, stream);
        }

        public KeyValuePair<Tfirst, Tsecond> Read(Stream stream)
        {
            Tfirst first = firstSerialization.Read(stream);
            Tsecond second = secondSerialization.Read(stream);
            return new KeyValuePair<Tfirst, Tsecond>(first, second);
        }

        public KeyValuePair<Tfirst, Tsecond> Read(byte[] buffer, ref long position)
        {
            Tfirst first = firstSerialization.Read(buffer, ref position);
            Tsecond second = secondSerialization.Read(buffer, ref position);
            return new KeyValuePair<Tfirst, Tsecond>(first, second);
        }
    }
}
