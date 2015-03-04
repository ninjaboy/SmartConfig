using System;
using System.Runtime.Serialization;

namespace SmartConfig
{
    [Serializable]
    public class SmartConfigException : Exception
    {
        /// <summary>
        /// Constructs empty instance of BS2000Exception
        /// </summary>
        public SmartConfigException()
        {
        }

        /// <summary>
        /// Requires for serialization purposes.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Serialization context.</param>
        protected SmartConfigException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Constructs an instance of BS2000Exception and initializes with error message.
        /// </summary>
        /// <param name="message">Error message for exception details.</param>
        public SmartConfigException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructs an instance of BS2000Exception and initializes with error message and inner exception.
        /// </summary>
        /// <param name="message">Error message for exception details.</param>
        /// <param name="inner">Inner exception instance</param>
        public SmartConfigException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartConfigException"/> class with message formatting abilities.
        /// </summary>
        /// <param name="format">The format template.</param>
        /// <param name="args">The arguments for formatting the message.</param>
        public SmartConfigException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }
    }
}