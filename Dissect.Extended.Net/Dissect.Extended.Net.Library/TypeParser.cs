using System;
using System.Collections.Generic;
using System.Text;

namespace Dissect.Extended.Net.Library
{
    /// <summary>
    /// Represents the base class for parsing different data types.
    /// </summary>
    internal class TypeParser
    {
        /// <summary>
        /// Parses a string as the supported type.
        /// classes.
        /// </summary>
        /// <param name="input">The input that should be parsed.</param>
        /// <returns>The parsed value.</returns>
        /// <exception cref="Exception">Thrown if parser is not valid.</exception>
        public ParseResult<object> Parse(string input)
        {
            if (!this.IsValid) throw new Exception($"Cannot call {nameof(Parse)} on a non valid instance. Parser has the following error: {this.ErrorMessage}.");

            return ParseInternal(input);
        }

        /// <summary>
        /// The default parsing method, that will just return the provided input. Should be overriden inside derived
        /// classes.
        /// </summary>
        /// <param name="input">The input that should be parsed.</param>
        /// <returns>The parsed value.</returns>

        protected virtual ParseResult<object> ParseInternal(string input)
        {
            return ParseResult<object>.CreateSuccessResult(input);
        }


        /// <summary>
        /// A value indicating whether the parser is in a valid state. 
        /// </summary>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// A message giving details about an invalid state. Will be empty if <see cref="IsValid"/> is true.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// The data type of the parsed input.
        /// </summary>
        public DataTypes Type { get; set; } = DataTypes.String;
    }
}
