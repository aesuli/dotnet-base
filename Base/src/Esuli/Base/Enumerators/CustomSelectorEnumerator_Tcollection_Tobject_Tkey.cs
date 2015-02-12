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

namespace Esuli.Base.Enumerators
{
    using System;
    using System.Collections.Generic;

    public class CustomSelectorEnumerator<Tcollection, Tobject, Tkey>: IEnumerator<Tobject>
    {
        private Tcollection collection;
        private Func<Tcollection, Tkey, Tobject> selector;
        private IEnumerator<Tkey> indexEnumerator;

        public CustomSelectorEnumerator(Tcollection collection, Func<Tcollection, Tkey, Tobject> selector, IEnumerator<Tkey> indexEnumerator)
        {
            this.collection = collection;
            this.selector = selector;
            this.indexEnumerator = indexEnumerator;
        }

        public Tobject Current
        {
            get
            {
                return selector(collection, indexEnumerator.Current);
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
                indexEnumerator.Dispose();
            }
        }

        object System.Collections.IEnumerator.Current
        {
            get
            {
                return selector(collection, indexEnumerator.Current);
            }
        }

        public bool MoveNext()
        {
            return indexEnumerator.MoveNext();
        }

        public void Reset()
        {
            indexEnumerator.Reset();
        }
    }
}
