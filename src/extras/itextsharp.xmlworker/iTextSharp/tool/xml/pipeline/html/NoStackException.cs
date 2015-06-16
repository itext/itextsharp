using System;

/**
 *
 */
namespace iTextSharp.tool.xml.pipeline.html {

    /**
     * Thrown when a StackKeeper was expected but could not be retrieved.
     * @author itextpdf.com
     *
     */
    [Serializable]
    public class NoStackException : Exception {
	    public NoStackException() {
	    }

	    /**
	     * @param message a message
	     */
	    public NoStackException(String message) : base(message) {
	    }

	    /**
	     * @param cause a cause
	     */
	    public NoStackException(Exception cause) : base("", cause) {
	    }

	    /**
	     * @param message a message
	     * @param cause a cause
	     */
	    public NoStackException(String message, Exception cause) : base(message, cause) {
	    }

        protected NoStackException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
