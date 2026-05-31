using System;
using System.Collections.Generic;
using System.Text;

namespace Dissect.Extended.Net.Library
{
    /// <summary>
    /// Represents the result of parsing a string to the value of a given type.
    /// </summary>
    internal class ParseResult<T> 
    {
        /// <summary>
        /// Creates an error result.
        /// </summary>
        /// <param name="errorMessage">The message describing the error.</param>
        /// <returns>The created <see cref="ParseResult"/>.</returns>
        public static ParseResult<T> CreateErrorResult(string errorMessage)
        {
            return new ParseResult<T>()
            {
                Result = default,
                Success = false,
                Error = errorMessage
            };
        }

        /// <summary>
        /// Creates a successful parse result.
        /// </summary>
        /// <param name="result">The parsed value.</param>
        /// <returns>The created <see cref="ParseResult"/>.</returns>
        public static ParseResult<T> CreateSuccessResult(T result)
        {
            return new ParseResult<T>()
            {
                Result = result,
                Success = true,
                Error = string.Empty
            };
        }

        /// <summary>
        /// Prevents the constructor from being called externally. Please use the static methods to create an instance.
        /// </summary>
        private ParseResult() { }

        /// <summary>
        /// The result of the parse operation. Value is undefined if parsing is not successful.
        /// </summary>
        public T? Result { get; set; } = default(T);

        /// <summary>
        /// A value indicating, whether the input could be successfully parsed.
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// An error message. Will only be set if <see cref="Success"/> is false.
        /// </summary>
        public string Error { get; set; } = string.Empty;
    }
}
