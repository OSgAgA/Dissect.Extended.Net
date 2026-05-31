using System;
using System.Collections.Generic;
using System.Text;

namespace Dissect.Extended.Net.Library
{
    /// <summary>
    /// Declares a delegate type for parsing a simple value.
    /// </summary>
    /// <param name="input">The input that should be parsed.</param>
    /// <param name="value">The parsed value.</param>
    /// <returns>A value indicating whether parsing has been successful.</returns>
    public delegate bool ParserDelegate(string input, out object value);

    /// <summary>
    /// Represents a parser for simple types, that do not have parameters and use default .net parsing
    /// functions.
    /// </summary>
    internal class SimpleTypeParser : TypeParser
    {
        /// <summary>
        /// The parser used to parse the type.
        /// </summary>
        private ParserDelegate parser;

        /// <summary>
        /// The type, that will be parsed.
        /// </summary>
        private DataTypes type;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleTypeParser"/> class.
        /// </summary>
        /// <param name="type">The type that will be parsed.</param>
        /// <param name="parser">The parser for parsing the input.</param>
        internal SimpleTypeParser(DataTypes type, ParserDelegate parser)
        {
            this.parser = parser;
            this.type = type;
        }

        /// <summary>
        /// Parses the provided input.
        /// </summary>
        /// <param name="input">The string that should be parsed.</param>
        /// <returns></returns>
        protected override ParseResult<object> ParseInternal(string input)
        {
            object result;
            bool success = this.parser(input, out result);

            if (!success)
            {
                return ParseResult<object>.CreateErrorResult($"Could not parse input '{input}' as {this.type}.");
            }

            return ParseResult<object>.CreateSuccessResult(result);
        }
    }
}
