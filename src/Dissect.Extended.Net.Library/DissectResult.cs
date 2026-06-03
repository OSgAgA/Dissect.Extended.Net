using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace Dissect.Extended.Net.Library
{
    /// <summary>
    /// Represents the result of a dissect message parsing.
    /// </summary>
    public class DissectResult
    {
        /// <summary>
        /// Represents a partial value, that can be combined to a key.
        /// </summary>
        private struct partialKey
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="partialKey"/> struct.
            /// </summary>
            /// <param name="value">The string representation of the value.</param>
            /// <param name="order">The order number of the key.</param>
            /// <param name="type">The data type of the value.</param>
            /// <param name="separator">The seperator used for appending partial keys.</param>
            public partialKey(string value, int order, TypeParser type, string? separator)
            {
                Value = value;
                Order = order;
                Type = type;
                Separator = separator;
            }

            /// <summary>
            /// Gets or sets the data type of the value.
            /// </summary>
            public TypeParser? Type;

            /// <summary>
            /// The string representation of the value.
            /// </summary>
            public string Value;

            /// <summary>
            /// The order of the partial key.
            /// </summary>
            public int Order;

            /// <summary>
            /// The seperator for appending partial keys.
            /// </summary>
            public string? Separator;
        }

        /// <summary>
        /// Represents a reference key.
        /// </summary>
        private class ReferenceKey
        {
            /// <summary>
            /// The value of the reference key.
            /// </summary>
            public string? Value = null;

            /// <summary>
            /// The key of the reference key.
            /// </summary>
            public string? Key = null;

            /// <summary>
            /// The data type of the value.
            /// </summary>
            public TypeParser Type = new ();
        }

        /// <summary>
        /// contains the result dictionary as objects.
        /// </summary>
        private Dictionary<string, object> resultDict = new();

        /// <summary>
        /// Holds all partial keys, by mapping the key name to all partial values.
        /// </summary>
        private Dictionary<string, List<partialKey>> partialValues = new Dictionary<string, List<partialKey>>();

        /// <summary>
        /// Holds all referenced keys, by mapping the key name to the reference keys.
        /// </summary>
        private Dictionary<string, ReferenceKey> referenceKeys = new();

        /// <summary>
        /// Tries to parse a value to the given data type and then add the result to the result dictionary.
        /// </summary>
        /// <param name="keyName">The key name.</param>
        /// <param name="type">The data type of the value.</param>
        /// <param name="value">The value as a string representation.</param>
        private void AddValue(string keyName, TypeParser type, string value)
        {
            if (keyName.StartsWith("?")) return;

            var result = type.Parse(value);

            if (result.Success && result.Result != null)
            {
                this.resultDict[keyName] = result.Result;
            }
            else
            {
                this.Success = false;
            }
        }

        /// <summary>
        /// Adds a partial value.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <param name="order">The order of the partial value.</param>
        /// <param name="value">The string representation of the value.</param>
        /// <param name="type">The data type of the value.</param>
        /// <param name="separator">The seperator used for appending partial values.</param>
        private void AddPartialValue(string key, int order, string value, TypeParser type, string? separator)
        {
            if (!this.partialValues.ContainsKey(key))
            {
                this.partialValues[key] = new List<partialKey>();
            }

            this.partialValues[key].Add(new partialKey(value, order, type, separator));
        }

        /// <summary>
        /// Adds a value to the result.
        /// </summary>
        /// <param name="key">The key for which the value should be added.</param>
        /// <param name="value">The string representation of the value.</param>
        internal void AddValue(Key key, string value)
        {
            if (key.IsReferenceKey)
            {
                if (!this.referenceKeys.ContainsKey(key.Name)) this.referenceKeys[key.Name] = new();

                this.referenceKeys[key.Name].Key = value;
                this.referenceKeys[key.Name].Type = key.Type;
            }
            else if (key.IsReferenceValue)
            {
                if (!this.referenceKeys.ContainsKey(key.Name)) this.referenceKeys[key.Name] = new();

                this.referenceKeys[key.Name].Value = value;
                this.referenceKeys[key.Name].Type = key.Type;
            }
            else
            {
                this.AddPartialValue(key.Name, key.Order, value, key.Type, key.Separator);
            }
        }

        /// <summary>
        /// Builds the real values out of the partial ones.
        /// </summary>
        /// <param name="separator">The separator used for appending the partial string values.</param>
        internal void BuildPartialValues(string separator)
        {
            if (this.Success)
            {
                foreach (var item in this.partialValues)
                {
                    TypeParser type = new TypeParser();
                    StringBuilder sb = new();

                    bool first = true;

                    foreach (var partialValue in item.Value.OrderBy(value => value.Order))
                    {
                        if (!first) sb.Append(separator);
                        first = false;

                        if (partialValue.Separator != null)
                        {
                            separator = partialValue.Separator;
                        }

                        sb.Append(partialValue.Value.ToString());
                        if (partialValue.Type != null)
                        {
                            type = partialValue.Type;
                        }
                    }

                    this.AddValue(item.Key, type, sb.ToString());
                }

                foreach(var item in this.referenceKeys)
                {
                    if (item.Value.Key == null || item.Value.Value == null)
                    {
                        this.Success = false;
                        return;
                    }

                    this.AddValue(item.Value.Key, item.Value.Type, item.Value.Value);
                }
            }
        }

        /// <summary>
        /// A value indicating whether the pattern was successfully applied to the input.
        /// </summary>
        public bool Success { get; internal set; } = true;

        /// <summary>
        /// Provides the result as a dictionary.
        /// </summary>
        /// <returns>The result as a dictionary.</returns>
        public Dictionary<string, object> ToDictionary()
        {
            return this.resultDict;
        }
    }
}
