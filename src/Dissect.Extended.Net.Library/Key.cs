
namespace Dissect.Extended.Net.Library
{
    /// <summary>
    /// Represents the key element of a dissect pattern.
    /// </summary>
    internal class Key : DissectBase
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Key"/> class.
        /// </summary>
        /// <param name="name">The name of the key.</param>
        /// <param name="isPartial">A value indicating whether the key is part of a constructed key, consisting of multiple partial keys.</param>
        /// <param name="isReferenceKey">A value indicating, whether the key is the key part of a reference modifier.</param>
        /// <param name="isReferenceValue">A value indicating, whether the key is the value part of a reference modifier.</param>
        /// <param name="order">The order number of the key to provide the order in which partial keys will be applied.</param>
        /// <param name="type">The data type of the key as a string representation.</param>
        /// <param name="typeParameter">The parameter string containing the optional parameters for the provided data type.</param>
        /// <param name="separator">The separator for appending multiple keys with an append modifier. Overrides the default separator if set to a value that is not null.</param>

        public Key(string name, bool isPartial = false, bool isReferenceKey = false, bool isReferenceValue = false, int order = 0, string type = "", string typeParameter = "", string? separator = null)
        {
            this.Name = name;
            this.IsPartial = isPartial;
            this.IsReferenceValue = isReferenceValue;
            this.IsReferenceKey = isReferenceKey;
            this.Order = order;
            this.Separator = separator;

            var typeResult = DataTypeParserFactory.ParseTypeInformation(type, typeParameter);

            if (!typeResult.Success || typeResult.Result == null)
            {
                this.IsValid = false;
                this.ErrorMessage = typeResult.Error;
                this.Type = new TypeParser();
            }
            else
            {
                this.Type = typeResult.Result;
            }
        }

        /// <summary>
        /// Gets or sets the separator used for appending multiple keys with an append modifier. Overrides the default separator if set to a value that is not null.
        /// </summary>
        public string? Separator { get; set;  } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the key is in a valid state.
        /// </summary>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// Gets or sets a message describing the error, if <see cref="IsValid"/> is false.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets the name of the key.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a value indicating whether the key is part of a constructed key, consisting of multiple partial keys.
        /// </summary>
        public bool IsPartial { get; }

        /// <summary>
        /// Gets a value indicating, whether the key is the key part of a reference modifier.
        /// </summary>
        public bool IsReferenceKey { get; }

        /// <summary>
        /// Gets a value indicating, whether the key is the value part of a reference modifier.
        /// </summary>
        public bool IsReferenceValue { get; }

        /// <summary>
        /// Gets the order number of the key to provide the order in which partial keys will be applied.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Gets the data type of the key.
        /// </summary>
        public TypeParser Type { get; }
    }

}
