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

namespace Esuli.Base.UI
{
    using System;

    /// <summary>
    /// Code for yes/no questions on console.
    /// </summary>
    public class Question
    {
        public static bool Ask(string question, bool defaultAnswer, char nondefaultAnswerChar, bool caseSensitive, bool hideChar)
        {
            Console.Write(question);
            char answer;
            if (caseSensitive)
            {
                answer = Console.ReadKey(hideChar).KeyChar;
            }
            else
            {
                nondefaultAnswerChar = Char.ToLower(nondefaultAnswerChar);
                answer = Char.ToLower(Console.ReadKey(hideChar).KeyChar);
            }
            Console.WriteLine();
            if (answer == nondefaultAnswerChar)
            {
                return !defaultAnswer;
            }
            else
            {
                return defaultAnswer;
            }
        }
    }
}
