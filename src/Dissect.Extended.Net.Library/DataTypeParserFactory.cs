using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Dissect.Extended.Net.Library
{
    /// <summary>
    /// Parses data type information provided from the dissect pattern and creates the corresponding parser.
    /// </summary>
    internal class DataTypeParserFactory
    {
        /// <summary>
        /// Parses the type information provided from the dissect pattern and creates the corresponding parser.
        /// </summary>
        /// <param name="dataTypeRepresentation">The text representation of the data type.</param>
        /// <param name="parameter">An optional type parameter.</param>
        /// <returns>The created type parser.</returns>
        internal static ParseResult<TypeParser> ParseTypeInformation(string dataTypeRepresentation, string parameter)
        {
            TypeParser? result = null;

            switch (dataTypeRepresentation.ToLower())
            {
                case "datetime":
                    result = new DateAndTimeParser(DataTypes.DateTime, parameter);
                    break;
                case "time":
                    result = new DateAndTimeParser(DataTypes.Time, parameter);
                    break;
                case "string":
                    result = new SimpleTypeParser(DataTypes.String, (string input, out object result) =>
                    {
                        result = input;
                        return true;
                    });
                    break;
                case "int":
                    result = new SimpleTypeParser(DataTypes.Int, (string input, out object result) =>
                    {
                        int intResult;
                        bool success = int.TryParse(input, out intResult);
                        result = intResult;
                        return success;
                    });
                    break;
                case "double":
                    result = new FloatParser(DataTypes.Double, parameter);
                    break;
                case "float":
                    result = new FloatParser(DataTypes.Float, parameter);
                    break;
                case "decimal":
                    result = new FloatParser(DataTypes.Decimal, parameter);
                    break;
            }

            if (result == null && string.IsNullOrWhiteSpace(dataTypeRepresentation))
            {
                result = new SimpleTypeParser(DataTypes.String, (string input, out object result) =>
                {
                    result = input;
                    return true;
                });
            }

            if (result == null)
            {
                return ParseResult<TypeParser>.CreateErrorResult($"'{dataTypeRepresentation}' is not a supported data type.");
            }

            if (!result.IsValid)
            {
                return ParseResult<TypeParser>.CreateErrorResult(result.ErrorMessage);
            }

            return ParseResult<TypeParser>.CreateSuccessResult(result);
        }
    }
}
