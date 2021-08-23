using System;
using System.Collections.Generic;
using System.Linq;

namespace SecretHitler.Application.Common.Exceptions
{
    /// <summary>
    /// Simple subclass of <see cref="Exception"/> that indicates an invalid request was issued
    /// to the state machine.
    /// </summary>
    public class GameStateException : InvalidOperationException
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public GameStateException() { }

        /// <summary>
        /// Constructor with an exception message.
        /// </summary>
        /// <param name="message">Message</param>
        public GameStateException(string message) : base(message) { }
    }
}
