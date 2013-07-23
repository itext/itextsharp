using System.IO;
using System.Text;

namespace iTextSharp.xmp.impl {
    /// <summary>
    /// Abstract class for reading filtered character streams.
    /// The abstract class <code>FilterReader</code> itself
    /// provides default methods that pass all requests to
    /// the contained stream. Subclasses of <code>FilterReader</code>
    /// should override some of these methods and may also provide
    /// additional methods and fields.
    /// 
    /// @author      Mark Reinhold
    /// @since       JDK1.1
    /// </summary>
    public abstract class FilterReader : StreamReader {
        protected object Locker;

        protected FilterReader(Stream inp, Encoding enc)
            : base(inp, enc) {
            Locker = this;
        }

        protected FilterReader(Stream inp, string enc)
            : this(inp, Encoding.GetEncoding(enc)) {
        }

        /// <summary>
        /// Creates a new filtered reader.
        /// </summary>
        /// <param name="in">  a Reader object providing the underlying stream. </param>
        protected FilterReader(Stream inp)
            : this(inp, Encoding.Default) {
        }

        /// <summary>
        /// Skips characters.
        /// </summary>
        /// <exception cref="IOException">  If an I/O error occurs </exception>
        public virtual long Skip(long n) {
            char[] buf = new char[n];
            return Read(buf, 0, (int) n);
        }

        /// <summary>
        /// Tells whether this stream is ready to be read.
        /// </summary>
        /// <exception cref="IOException">  If an I/O error occurs </exception>
        public virtual bool Ready() {
            return BaseStream != null;
        }
    }
}