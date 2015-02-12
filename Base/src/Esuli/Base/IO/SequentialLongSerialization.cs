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
    using System.IO;

    /// <summary>
    /// Sequential serialization of long integers. Uses delta coding and variable byte coding to save space.
    /// </summary>
    public class SequentialLongSerialization
        : ISequentialObjectSerialization<long>
    {
        public SequentialLongSerialization()
        {
        }

        public void WriteFirst(long obj, Stream stream)
        {
            if (obj < 0)
            {
                VariableByteCoding.Write(0, stream);
                VariableByteCoding.Write(-obj, stream);
            }
            else
            {
                VariableByteCoding.Write(1, stream);
                VariableByteCoding.Write(obj, stream);
            }
        }

        public void Write(long obj, long previousObj, Stream stream)
        {
            long delta = obj - previousObj;
            VariableByteCoding.Write(delta, stream);
        }

        public long ReadFirst(Stream stream)
        {
            long sign = VariableByteCoding.Read(stream);
            if (sign == 0)
            {
                sign = -1;
            }

            return VariableByteCoding.Read(stream) * sign;
        }

        public long Read(long previousObj, Stream stream)
        {
            return VariableByteCoding.Read(stream) + previousObj;
        }

        public long ReadFirst(byte[] buffer, ref long position)
        {
            long sign = VariableByteCoding.Read(buffer, ref position);
            if (sign == 0)
            {
                sign = -1;
            }

            return VariableByteCoding.Read(buffer, ref position) * sign;
        }
        
        public long Read(long previous, byte[] buffer, ref long position)
        {
            return previous + VariableByteCoding.Read(buffer, ref position);
        }

    }
}
