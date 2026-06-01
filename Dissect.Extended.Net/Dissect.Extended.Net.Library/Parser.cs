using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Dissect.Extended.Net.Library
{
    /// <summary>
    /// Represents the extended .net dissect parser.
    /// </summary>
    public class Parser
    {
        /// <summary>
        /// The pattern that was parsed from the initially provided string.
        /// </summary>
        private Pattern pattern;

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="pattern">The pattern for parsing messages.</param>
        /// <param name="separator">An optional separator for appending partial keys.</param>
        public Parser(string pattern, string? separator = null)
        {
            this.pattern = Parser.ParsePattern(pattern);
            if (separator != null) this.pattern.Separator = separator;
        }

        /// <summary>
        /// Gets a value indicating whether the pattern is a valid extended dissect pattern.
        /// </summary>
        public bool IsValid { get => this.pattern.IsValid; }

        /// <summary>
        /// Gets an error message in case <see cref="IsValid"/> is true, or an empty string otherwise.
        /// </summary>
        public string ErrorMessage { get => this.pattern.ErrorMessage; }

        /// <summary>
        /// Parses the provided input by applying the pattern.
        /// </summary>
        /// <param name="input">The input that should be parsed.</param>
        /// <returns>The parse result.</returns>
        public DissectResult Parse(string input)
        {
            string remainingBuffer = input;
            var result = new DissectResult();

            if (!this.pattern.IsValid)
            {
                result.Success = false;
                return result;
            }

            Key? key = null;

            foreach (var element in this.pattern.Elements)
            {
                if (element is Key keyElement)
                {
                    key = keyElement;
                }
                else if (element is Delimiter delimiter)
                {
                    var split = remainingBuffer.Split(delimiter.DelimiterText, 2);
                    if (split.Length != 2)
                    {
                        result.Success = false;
                        return result;
                    }

                    if (key != null)
                    {
                        result.AddValue(key, split[0]);
                    }

                    remainingBuffer = split[1];

                    if (delimiter.UsePadding)
                    {
                        bool startsWithSequence = remainingBuffer.StartsWith(delimiter.DelimiterText);

                        while (startsWithSequence)
                        {
                            remainingBuffer = remainingBuffer[delimiter.DelimiterText.Length..];
                            startsWithSequence = remainingBuffer.StartsWith(delimiter.DelimiterText);
                        }
                    }
                }
            }

            // If last element of the pattern is a key it will contain the remaining buffer.
            if (this.pattern.Elements.Count > 0 && this.pattern.Elements.Last() is Key remainingBufferKey)
            {
                result.AddValue(remainingBufferKey, remainingBuffer);
            }

            result.BuildPartialValues(this.pattern.Separator);
            return result;
        }

        /// <summary>
        /// Holds buffered values during parsing.
        /// </summary>
        private struct ParseBuffer
        {
            /// <summary>
            /// The currently read string buffer, that is no interpreted, yet.
            /// </summary>
            public string buffer = string.Empty;

            /// <summary>
            /// The buffer containing the name of the active key.
            /// </summary>
            public string keyBuffer = string.Empty;

            /// <summary>
            /// The buffer containing the type of the active key.
            /// </summary>
            public string typeBuffer = string.Empty;

            /// <summary>
            /// The buffer containting the type parameter of the active key.
            /// </summary>
            public string typeParameterBuffer = string.Empty;

            /// <summary>
            /// The buffer containting the separator information of an append modifier.
            /// </summary>
            public string? separatorBuffer = null;

            /// <summary>
            /// A value indicating, whether a key is partial.
            /// </summary>
            public bool isPartial;

            /// <summary>
            /// A value indicating whether a key is a referential key.
            /// </summary>
            public bool isReferentialKey;

            /// <summary>
            /// A value indicating whehter a key is a referential key value.
            /// </summary>
            public bool isReferentialValue;

            /// <summary>
            /// A value indicating whether right padding is active.
            /// </summary>
            public bool isRightPaddingActive;

            /// <summary>
            /// A value containt the order number of the currently active partial key.
            /// </summary>
            public int order;

            /// <summary>
            /// Initializes a new instance of the <see cref="ParseBuffer"/> struct.
            /// </summary>
            public ParseBuffer()
            {
                this.Reset();
            }

            /// <summary>
            /// Resets all buffer values to their defaults.
            /// </summary>
            public void Reset()
            {
                this.buffer = string.Empty;
                this.keyBuffer = string.Empty;
                this.typeBuffer = string.Empty;
                this.typeParameterBuffer = string.Empty;
                this.isPartial = false;
                this.isReferentialKey = false;
                this.isReferentialValue = false;
                this.isRightPaddingActive = false;
                this.separatorBuffer = null;

                this.order = 0;
            }
        }

        /// <summary>
        /// Parses the extended dissect pattern.
        /// </summary>
        /// <param name="pattern">The pattern to be parsed.</param>
        /// <returns>The parsed pattern.</returns>
        private static Pattern ParsePattern(string pattern)
        {
            var dissectPattern = new Pattern();

            var parseBuffer = new ParseBuffer();
            var state = States.Start;

            for (int i = 0; i < pattern.Length; i++)
            {
                char c = pattern[i];

                char? nextChar = null;
                if (pattern.Length > i + 1) nextChar = pattern[i + 1];

                char? nextnextChar = null;
                if (pattern.Length > i + 2) nextnextChar = pattern[i + 2];

                switch (state)
                {
                    // Will detect either a new delimiter or the beginning of a key.
                    case States.Start:
                        if (c == '%' && nextChar == '{')
                        {
                            i++;
                            state = States.StartKey;
                        }
                        else
                        {
                            parseBuffer.buffer += c;
                            state = States.Delimiter;
                        }
                        break;
                    // Parses a delimiter.
                    case States.Delimiter:
                        if (c == '%' && nextChar == '{')
                        {
                            i++;
                            dissectPattern.Elements.Add(new Delimiter(parseBuffer.buffer, parseBuffer.isRightPaddingActive));
                            parseBuffer.Reset();
                            state = States.StartKey;
                        }
                        else
                        {
                            parseBuffer.buffer += c;
                        }
                        break;
                    // Starts parsing a key. Detects whether a left side modifier should be applied. 
                    case States.StartKey:
                        if (c == '}')
                        {
                            state = States.Start;
                        }
                        else if (c == '+')
                        {
                            parseBuffer.isPartial = true;
                            if (nextChar == '[')
                            {
                                state = States.Separator;
                                i = i + 1;
                            }
                            else
                            {
                                state = States.Key;
                            }
                        }
                        else if (c == '*')
                        {
                            parseBuffer.isReferentialKey = true;
                            state = States.Key;
                        }
                        else if (c == '&')
                        {
                            parseBuffer.isReferentialValue = true;
                            state = States.Key;
                        }
                        else
                        {
                            parseBuffer.buffer += c;
                            state = States.Key;
                        }
                        break;
                    // Parses a separator for an append modifier.
                    case States.Separator:
                        if (c == ']')
                        {
                            if (parseBuffer.separatorBuffer == null) parseBuffer.separatorBuffer = string.Empty;
                            state = States.Key;
                        }
                        else
                        {
                            if (parseBuffer.separatorBuffer == null)
                            {
                                parseBuffer.separatorBuffer = c.ToString();
                            }
                            else
                            {
                                parseBuffer.separatorBuffer += c;
                            }
                        }
                        break;
                    // Parses a key after all left side modifiers are handled.
                    case States.Key:
                        if (c == '}')
                        {
                            if (!string.IsNullOrWhiteSpace(parseBuffer.buffer))
                            {
                                dissectPattern.Elements.Add(new Key(parseBuffer.buffer, parseBuffer.isPartial, parseBuffer.isReferentialKey, parseBuffer.isReferentialValue, separator: parseBuffer.separatorBuffer));
                            }

                            parseBuffer.Reset();
                            state = States.Start;
                        }
                        else if (c == '/')
                        {
                            parseBuffer.keyBuffer = parseBuffer.buffer;
                            parseBuffer.buffer = string.Empty;
                            state = States.Order;
                        }
                        else if (c == ':')
                        {
                            parseBuffer.keyBuffer = parseBuffer.buffer;
                            parseBuffer.buffer = string.Empty;
                            state = States.Type;
                        }
                        else if (c == '-' && nextChar == '>' && nextnextChar == '}')
                        {
                            if (!string.IsNullOrWhiteSpace(parseBuffer.buffer) && parseBuffer.buffer != "?")
                            {
                                dissectPattern.Elements.Add(new Key(parseBuffer.buffer, parseBuffer.isPartial, parseBuffer.isReferentialKey, parseBuffer.isReferentialValue, separator: parseBuffer.separatorBuffer));
                            }

                            parseBuffer.Reset();
                            parseBuffer.isRightPaddingActive = true;
                            state = States.Start;
                            i = i + 2;
                        }
                        else if (c == '?')
                        {
                            dissectPattern.IsValid = false;
                            dissectPattern.ErrorMessage += $"Key must not contain char '?'.\n";
                        }
                        else
                        {
                            parseBuffer.buffer += c;
                        }
                        break;
                    // Parses the order of a partial key.
                    case States.Order:
                        if (c == '}')
                        {
                            int orderResult = 0;
                            if (int.TryParse(parseBuffer.buffer, out orderResult))
                            {
                                parseBuffer.order = orderResult;
                            }
                            else
                            {
                                dissectPattern.ErrorMessage += $"Order value '{parseBuffer.buffer}' must be a valid integer.\n";
                                dissectPattern.IsValid = false;
                            }

                            dissectPattern.Elements.Add(new Key(parseBuffer.keyBuffer, parseBuffer.isPartial, parseBuffer.isReferentialKey, parseBuffer.isReferentialValue, parseBuffer.order, type: parseBuffer.typeBuffer, separator: parseBuffer.separatorBuffer));
                            parseBuffer.Reset();
                            state = States.Start;
                        }
                        else if (c == ':')
                        {
                            int orderResult = 0;
                            if (int.TryParse(parseBuffer.buffer, out orderResult))
                            {
                                parseBuffer.order = orderResult;
                            }
                            else
                            {
                                dissectPattern.ErrorMessage += $"Order value '{parseBuffer.buffer}' must be a valid integer.\n";
                                dissectPattern.IsValid = false;
                            }
                            parseBuffer.buffer = string.Empty;
                            state = States.Type;
                        }
                        else if (c == '-' && nextChar == '>' && nextnextChar == '}')
                        {
                            if (!string.IsNullOrWhiteSpace(parseBuffer.buffer) && parseBuffer.buffer != "?")
                            {
                                int orderResult = 0;
                                if (int.TryParse(parseBuffer.buffer, out orderResult))
                                {
                                    parseBuffer.order = orderResult;
                                }
                                else
                                {
                                    dissectPattern.ErrorMessage += $"Order value '{parseBuffer.buffer}' must be a valid integer.\n";
                                    dissectPattern.IsValid = false;
                                }
                                dissectPattern.Elements.Add(new Key(parseBuffer.keyBuffer, parseBuffer.isPartial, parseBuffer.isReferentialKey, parseBuffer.isReferentialValue, parseBuffer.order, type: parseBuffer.typeBuffer, separator: parseBuffer.separatorBuffer));
                            }

                            parseBuffer.Reset();
                            parseBuffer.isRightPaddingActive = true;
                            state = States.Start;
                            i = i + 2;
                        }
                        else
                        {
                            parseBuffer.buffer += c;
                        }
                        break;
                    // Parses the data type of a key.
                    case States.Type:
                        if (c == '}')
                        {
                            if (string.IsNullOrEmpty(parseBuffer.typeBuffer)) parseBuffer.typeBuffer = parseBuffer.buffer;

                            dissectPattern.Elements.Add(new Key(parseBuffer.keyBuffer, parseBuffer.isPartial, parseBuffer.isReferentialKey, parseBuffer.isReferentialValue, parseBuffer.order, type: parseBuffer.typeBuffer, typeParameter: parseBuffer.typeParameterBuffer, separator: parseBuffer.separatorBuffer));
                            parseBuffer.Reset();
                            state = States.Start;
                        }
                        else if (c == '-' && nextChar == '>' && nextnextChar == '}')
                        {
                            if (!string.IsNullOrWhiteSpace(parseBuffer.buffer) && parseBuffer.buffer != "?")
                            {
                                dissectPattern.Elements.Add(new Key(parseBuffer.keyBuffer, parseBuffer.isPartial, parseBuffer.isReferentialKey, parseBuffer.isReferentialValue, parseBuffer.order, type: parseBuffer.buffer, separator: parseBuffer.separatorBuffer));
                            }

                            parseBuffer.Reset();
                            state = States.Start;
                            i = i + 2;
                        }
                        else if (c == '[')
                        {
                            parseBuffer.typeBuffer = parseBuffer.buffer;
                            parseBuffer.buffer = string.Empty;
                            state = States.TypeParameter;
                        }
                        else if (c == '/')
                        {
                            parseBuffer.typeBuffer = parseBuffer.buffer;
                            parseBuffer.buffer = string.Empty;
                            state = States.Order;
                        }
                        else
                        {
                            parseBuffer.buffer += c;
                        }
                        break;
                    case States.TypeParameter:
                        if (c == ']')
                        {
                            parseBuffer.typeParameterBuffer = parseBuffer.buffer;
                            state = States.Type;
                        }
                        else
                        {
                            parseBuffer.buffer += c;
                        }
                        break;
                    // Handles errors.
                    case States.Error:
                        dissectPattern.IsValid = false;
                        break;
                }
            }



            if (parseBuffer.buffer.Length > 0 && state == States.Delimiter)
            {
                dissectPattern.Elements.Add(new Delimiter(parseBuffer.buffer, usePadding: false));
            }

            if (parseBuffer.buffer.Length > 0 && state == States.TypeParameter)
            {
                dissectPattern.ErrorMessage += $"Could not read type parameter. Missing ']' at end of {parseBuffer.buffer}.\n";
                dissectPattern.IsValid = false;
            }
            else if (parseBuffer.buffer.Length > 0 && state == States.Separator)
            {
                dissectPattern.ErrorMessage += $"Could not read type separator pattern. Missing ']' at end of {parseBuffer.buffer}.\n";
                dissectPattern.IsValid = false;
            }
            else if (parseBuffer.buffer.Length > 0 && state != States.Delimiter && state != States.Error)
            {
                dissectPattern.ErrorMessage += $"Could not parse key '{parseBuffer.buffer}'. Are you missing a }}?\n";
                dissectPattern.IsValid = false;
            }
            else if (!dissectPattern.Elements.Where(element => element is Key).Any())
            {
                dissectPattern.ErrorMessage += "\nDissect pattern must contain at least one key.\n";
                dissectPattern.IsValid = false;
            }


            dissectPattern.ValidateElements();
            return dissectPattern;
        }
    }
}
