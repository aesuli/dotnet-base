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
    /// Enumerates read streams from an array of FileInfo
    /// </summary>
    public class FileInfoStreamEnumerator : IEnumerator<Stream>
    {
        private FileInfo [] fileInfoList;
        private bool deleteFileOnMoveNext;
        private int position;
        private Stream currentStream;
        private int bufferSize;
        private FileOptions fileOptions;

        public FileInfoStreamEnumerator(FileInfo [] fileInfoList, bool deleteFileOnMoveNext, int bufferSize, FileOptions fileOptions)
        {
            this.fileInfoList = fileInfoList;
            position = -1;
            this.deleteFileOnMoveNext = deleteFileOnMoveNext;
            currentStream = null;
            this.bufferSize = bufferSize;
            this.fileOptions = fileOptions;
        }

        public Stream Current
        {
            get
            {
                return currentStream;
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
            }
        }

        object System.Collections.IEnumerator.Current
        {
            get
            {
                return currentStream;
            }
        }

        public bool MoveNext()
        {
            if (currentStream != null)
            {
                currentStream.Close();
                currentStream = null;
                if (deleteFileOnMoveNext)
                {
                    fileInfoList[position].Delete();
                }
            }
            ++position;
            if (position >= fileInfoList.Length)
            {
                position = fileInfoList.Length;
                return false;
            }

            currentStream = new FileStream(fileInfoList[position].FullName, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize, fileOptions);
            return true;
        }

        public void Reset()
        {
            if (deleteFileOnMoveNext)
            {
                throw new NotSupportedException("Cannot reset a " + this.GetType().Name + " when the deleteFileOnMoveNext flag is true");
            }
            if (currentStream != null)
            {
                currentStream.Close();
            }
            position = -1;
            currentStream = null;
        }
    }
}
