using System;
using System.Collections.Generic;
using System.Text;

namespace Dissect.Extended.Net.Library
{
    /// <summary>
    /// Represents the states used by the state machine while parsing the pattern.
    /// </summary>
    internal enum States
    {
        /// <summary>
        /// The starting state.
        /// </summary>
        Start,

        /// <summary>
        /// The state while parsing a dilimiter.
        /// </summary>
        Delimiter,

        /// <summary>
        /// The state while starting to parse a key. Here all left side modifiers will be handled.
        /// </summary>
        StartKey,

        /// <summary>
        /// The state while parsing a key, wher all left side modifiers have allready been handled.
        /// </summary>
        Key,

        /// <summary>
        /// The state while parsing an order number for a partial key.
        /// </summary>
        Order,

        /// <summary>
        /// The state while parsing a data type.
        /// </summary>
        Type,

        /// <summary>
        /// The state while parsing a parameter of a data type.
        /// </summary>
        TypeParameter,

        /// <summary>
        /// The error state.
        /// </summary>
        Error
    }
}
