using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Dissect.Extended.Net.Library
{
    /// <summary>
    /// Provides a DateTimeParser.
    /// </summary>
    internal class DateAndTimeParser : TypeParser
    {
        /// <summary>
        /// The timezone of the time, that is read as the string input while parsing.
        /// </summary>
        private TimeZoneInfo? timeZoneInfo = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateAndTimeParser"/> class.
        /// 
        /// If an unsupported time zone is provided the parser will be set to not valid.
        /// </summary>
        /// <param name="type">Provideds the specific type. Supported: <see cref="DataTypes.DateTime"/> and <see cref="DataTypes.Time"/></param>
        /// <param name="parameter">An optional parameter, that contains the timezone as a IANA timecode or the word local. If an empty string is provided utc is assumed.</param>
        /// <exception cref="Exception">Thrown if a not supported type is provided,.</exception>
        public DateAndTimeParser(DataTypes type, string parameter)
        {
            if (type != DataTypes.DateTime && type != DataTypes.Time)
            {
                throw new Exception($"{nameof(DateAndTimeParser)} only supports {nameof(DataTypes.DateTime)} and {nameof(DataTypes.Time)} as data types. Received {type}.");
            }

            this.Type = type;

            if (parameter.ToLower() == "local")
            {
                this.timeZoneInfo = TimeZoneInfo.Local;
            }
            else if (!string.IsNullOrWhiteSpace(parameter))
            {
                TimeZoneInfo? timezone;
                var success = TimeZoneInfo.TryFindSystemTimeZoneById(parameter, out timezone);
                if (success && timezone != null)
                {
                    this.timeZoneInfo = timezone;
                }
                else
                {
                    this.ErrorMessage = $"Timezone '{parameter}' not found.";
                    this.IsValid = false;
                }
            }
        }

        /// <summary>
        /// Parses the given input as type <see cref="Type"/>.
        /// </summary>
        /// <param name="input">The string to be parsed.</param>
        /// <returns>A <see cref="ParseResult"/> containing the parsed object, or an error message.</returns>
        protected override ParseResult<object> ParseInternal(string input)
        {
            bool success = false;
            DateTime genericDate = DateTime.MinValue;

            switch (this.Type)
            {
                case DataTypes.DateTime:

                    success = DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out genericDate);
                    break;
                case DataTypes.Time:
                    TimeOnly time;
                    success = TimeOnly.TryParse(input, out time);
                    if (success)
                    {
                        genericDate = DateTime.Today.AddHours(time.Hour).AddMinutes(time.Minute).AddSeconds(time.Second);
                    }
                    break;
            }

            if (!success || genericDate == DateTime.MinValue)
            {
                return ParseResult<object>.CreateErrorResult($"Could not parse input '{input}' as DateTime.");
            }

            DateTime result = DateTime.MinValue;

            if (this.timeZoneInfo == null || genericDate.Kind != DateTimeKind.Unspecified)
            {
                result = TimeZoneInfo.ConvertTime(genericDate, TimeZoneInfo.Utc);
            }
            else 
            {
                result = TimeZoneInfo.ConvertTime(genericDate, this.timeZoneInfo, TimeZoneInfo.Utc);
            }

            return ParseResult<object>.CreateSuccessResult(result);
        }
    }
}
