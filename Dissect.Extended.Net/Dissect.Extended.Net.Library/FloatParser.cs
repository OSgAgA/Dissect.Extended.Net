using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Dissect.Extended.Net.Library
{
    /// <summary>
    /// Represents a parser for parsing floating point numbers.
    /// </summary>
    internal class FloatParser : TypeParser
    {
        /// <summary>
        /// The culture to be used for parsing the numbers.
        /// </summary>
        private CultureInfo culture = CultureInfo.InvariantCulture;

        /// <summary>
        /// The data type that should be parsed.
        /// </summary>
        private DataTypes type;

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatParser"/> class.
        /// 
        /// Supports all floating point data types. 
        /// 
        /// If parameter cannot be parsed, the parser will be set to invalid.
        /// </summary>
        /// <param name="dataType">The type that should be parsed by the parser.</param>
        /// <param name="parameter">The optional parameter containing the name of the culture to be used, or an empty string.</param>
        /// <exception cref="Exception">Thrown if an unsupported data type is used.</exception>
        internal FloatParser(DataTypes dataType, string parameter)
        {
            var allowedValues = new List<DataTypes>()
            {
                DataTypes.Double,
                DataTypes.Float,
                DataTypes.Decimal
            };

            if (!allowedValues.Contains(dataType))
            {
                throw new Exception($"{nameof(FloatParser)} only supports {string.Join(",", allowedValues)} as data types. Received {type}.");
            }

            this.type = dataType;

            if (string.IsNullOrWhiteSpace(parameter))
            {
                this.culture = CultureInfo.InvariantCulture;
                return;
            }

            try
            {
                this.culture = CultureInfo.GetCultureInfo(parameter);
            }
            catch
            {
                this.ErrorMessage = $"CultureInfo '{parameter}' not found.";
                this.IsValid = false;
            }

        }

        /// <summary>
        /// Parses the given input to the supported type. Should only be called on valid parsers, which is secured by the base class.
        /// </summary>
        /// <param name="input">The input that will be parsed.</param>
        /// <returns></returns>
        protected override ParseResult<object> ParseInternal(string input)
        {
            bool success = false;

            switch (this.type)
            {
                case DataTypes.Double:
                    double doubleResult;
                    success = double.TryParse(input, this.culture, out doubleResult);
                    if (success) return ParseResult<object>.CreateSuccessResult(doubleResult);
                    break;
                case DataTypes.Float:
                    float floatResult;
                    success = float.TryParse(input, this.culture, out floatResult);
                    if (success) return ParseResult<object>.CreateSuccessResult(floatResult);
                    break;
                case DataTypes.Decimal:
                    decimal decimalResult;
                    success = decimal.TryParse(input, this.culture, out decimalResult);
                    if (success) return ParseResult<object>.CreateSuccessResult(decimalResult);
                    break;
            }

            return ParseResult<object>.CreateErrorResult($"Could not parse input '{input}' as type '{this.type}' with culture '{this.culture}'");
        }
    }
}
