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

namespace Esuli.Base.Enumerators
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Enumerator that allows to fast forward until the Current object is equal or greater than the reference object.
    /// </summary>
    /// <typeparam name="Titem"></typeparam>
    public class EnumeratorWithJump<Titem> : IEnumeratorWithJump<Titem>
    {
        private IEnumerator<Titem> enumerator;
        private bool moved;

        public EnumeratorWithJump(IEnumerator<Titem> enumerator)
        {
            this.enumerator = enumerator;
            moved = false;
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
                enumerator.Dispose();
            }
        }
        
        public bool Jump(Titem item, IComparer<Titem> comparer)
        {
            bool noonecares;
            return Jump(item, comparer, out noonecares);
        }

        public bool Jump(Titem item, IComparer<Titem> comparer, out bool exactMatch)
        {
            exactMatch = false;
            if (moved)
            {
                int comp = comparer.Compare(enumerator.Current, item);
                if (comp == 0)
                {
                    exactMatch = true;
                    return true;
                }
                if (comp > 0)
                {
                    return true;
                }
            }

            while (enumerator.MoveNext())
            {
                moved = true;
                int comp = comparer.Compare(enumerator.Current, item);
                if (comp == 0)
                {
                    exactMatch = true;
                    return true;
                }
                if (comp > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public Titem Current
        {
            get
            {
                return enumerator.Current;
            }
        }

        object System.Collections.IEnumerator.Current
        {
            get
            {
                return enumerator.Current;
            }
        }

        public bool MoveNext()
        {
            moved = true;
            return enumerator.MoveNext();
        }

        public void Reset()
        {
            enumerator.Reset();
            moved = false;
        }
    }
}