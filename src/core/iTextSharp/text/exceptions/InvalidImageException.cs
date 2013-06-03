using System;
using System.Runtime.Serialization;

namespace iTextSharp.text.exceptions
{
    /**
     * RuntimeException to indicate that the provided Image is invalid/corrupted.
     * Should only be thrown/not caught when ignoring invalid images.
     * @since 5.4.2
     */
    public class InvalidImageException : Exception
    {
        public InvalidImageException() { }

        public InvalidImageException(string message) : base(message) { }

        public InvalidImageException(string message, Exception innerException)
            : base(message, innerException) { }

        protected InvalidImageException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}