using System;
using System.Runtime.Serialization;

namespace iTextSharp.text.exceptions
{
    public class ExceptionConverter : ApplicationException
    {
        public ExceptionConverter(string message) : base(message)
        {
        }

        public ExceptionConverter()
        {
        }

        public ExceptionConverter(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ExceptionConverter(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}