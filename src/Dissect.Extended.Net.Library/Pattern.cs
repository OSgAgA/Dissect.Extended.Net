using System;
using System.Collections.Generic;
using System.Text;

namespace Dissect.Extended.Net.Library
{
    /// <summary>
    /// Represents a dissect pattern.
    /// </summary>
    internal class Pattern
    {
        /// <summary>
        /// Gets the elements of the pattern in the correct order for parsing.
        /// </summary>
        public List<DissectBase> Elements { get; } = new();

        /// <summary>
        /// Gets the separator that should be used for appending partial strings.
        /// </summary>
        public string Separator { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the pattern is valid.
        /// </summary>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// Gets or sets an error message.
        /// </summary>
        public string ErrorMessage { get; internal set; } = string.Empty;

        /// <summary>
        /// Validates all elements and updates the <see cref="IsValid"/> and <see cref="ErrorMessage"/> properties.
        /// </summary>
        public void ValidateElements()
        {
            foreach (var element in Elements)
            {
                if (element is Key key && !key.IsValid)
                {
                    this.IsValid = false;
                    this.ErrorMessage += $"Error in key '{key.Name}': {key.ErrorMessage}\n";
                }
            }
        }
    }
}
