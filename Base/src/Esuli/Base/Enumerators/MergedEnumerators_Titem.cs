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
    using Esuli.Base.Collections;

    /// <summary>
    /// Merges an array of enumerators of sorted objects into a single enumeration.
    /// </summary>
    /// <typeparam name="Titem"></typeparam>
    public class MergedEnumerators<Titem> : IEnumerator<Titem>
    {
        private IEnumerator<Titem>[] enumerators;
        private IComparer<Titem> comparer;
        private Heap<KeyValuePair<Titem, int>> heap;
        private bool disposeEnumeratorsOnDispose;
        private Titem current;

        public MergedEnumerators(IEnumerator<Titem> [] enumerators, IComparer<Titem> comparer, bool disposeEnumeratorsOnDispose)
        {
            this.enumerators = enumerators;
            this.comparer = comparer;
            this.disposeEnumeratorsOnDispose = disposeEnumeratorsOnDispose;

            KeyValuePairKeyExternalComparer<Titem, int> keyComparer = new KeyValuePairKeyExternalComparer<Titem, int>(comparer);
            heap = new Heap<KeyValuePair<Titem, int>>(enumerators.Length, keyComparer);

            for (int i = 0; i < enumerators.Length; ++i)
            {
                if (enumerators[i].MoveNext())
                {
                    heap.Push(new KeyValuePair<Titem, int>(enumerators[i].Current, i));
                }
            }

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
                if (disposeEnumeratorsOnDispose)
                {
                    foreach (var enumerator in enumerators)
                    {
                        enumerator.Dispose();
                    }
                }
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
            if (heap.Count > 0)
            {
                KeyValuePair<Titem, int> pair = heap.Pop();
                current = pair.Key;
                int idx = pair.Value;
                if (enumerators[idx].MoveNext())
                {
                    heap.Push(new KeyValuePair<Titem, int>(enumerators[idx].Current, idx));
                }
                return true;
            }
            return false;
        }

        public void Reset()
        {
            KeyValuePairKeyExternalComparer<Titem, int> keyComparer = new KeyValuePairKeyExternalComparer<Titem, int>(comparer);
            heap = new Heap<KeyValuePair<Titem, int>>(enumerators.Length, keyComparer);

            for (int i = 0; i < enumerators.Length; ++i)
            {
                enumerators[i].Reset();
                if (enumerators[i].MoveNext())
                {
                    heap.Push(new KeyValuePair<Titem, int>(enumerators[i].Current, i));
                }
            }

            current = default(Titem);
        }
    }
}
