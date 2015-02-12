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
    using System.Collections.Generic;

    public interface IEnumeratorWithJump<Titem> : IEnumerator<Titem>
    {
        /// <summary>
        /// Moves forward the enumerator until it returns an item that is equal or greater 
        /// that the input item (with respect to the comparer function).
        /// </summary>
        /// <param name="item">The reference item.</param>
        /// <param name="comparer">The comparer of items.</param>
        /// <returns><c>false</c> if the enumerator hits the end without findings a equal or greater item</returns>
        bool Jump(Titem item, IComparer<Titem> comparer);

        /// <summary>
        /// Moves forward the enumerator until it returns an item that is equal or greater 
        /// that the input item (with respect to the comparer function).
        /// </summary>
        /// <param name="item">The reference item.</param>
        /// <param name="comparer">The comparer of items.</param>
        /// <param name="exactMatch">secondary output: <c>true</c> if the Current item is equal to the input item.</param>
        /// <returns><c>false</c> if the enumerator hits the end without findings a equal or greater item</returns>
        bool Jump(Titem item, IComparer<Titem> comparer, out bool exactMatch);
    }
}
