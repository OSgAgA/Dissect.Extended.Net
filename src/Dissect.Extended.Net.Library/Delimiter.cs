using System;
using System.Collections.Generic;
using System.Text;

namespace Dissect.Extended.Net.Library
{
    /// <summary>
    /// A delimiter is a part of the string that is not matched to a key. 
    /// </summary>
    /// <param name="delimiterText">The delimiter as a string.</param>
    /// <param name="usePadding">A value indicating whether right padding should be applied.</param>
    internal class Delimiter(string delimiterText, bool usePadding) : DissectBase
    {
        /// <summary>
        /// Gets the delimiter text.
        /// </summary>
        public string DelimiterText { get; } = delimiterText;

        /// <summary>
        /// Gets a value indicating whether padding should be used.
        /// </summary>
        public bool UsePadding { get; set; } = usePadding;
    }
}
