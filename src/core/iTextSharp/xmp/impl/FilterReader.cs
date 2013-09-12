using System.IO;

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
    public abstract class FilterReader : TextReader {
        protected TextReader inp;

        protected FilterReader(TextReader inp) {
            this.inp = Synchronized(inp);
        }

        /**
         * Reads a single character.
         *
         * @exception  IOException  If an I/O error occurs
         */

        public override int Read() {
            return inp.Read();
        }

        /**
         * Reads characters into a portion of an array.
         *
         * @exception  IOException  If an I/O error occurs
         */
        override public int Read(char[] cbuf, int off, int len) {
            return inp.Read(cbuf, off, len);
        }

        ///**
        // * Skips characters.
        // *
        // * @exception  IOException  If an I/O error occurs
        // */

        //public long Skip(long n) {
        //    return inp.Skip(n);
        //}

        ///**
        // * Tells whether this stream is ready to be read.
        // *
        // * @exception  IOException  If an I/O error occurs
        // */

        //public bool Ready() {
        //    return inp.Ready();
        //}

        ///**
        // * Tells whether this stream supports the Mark() operation.
        // */

        //public bool MarkSupported() {
        //    return inp.MarkSupported();
        //}

        ///**
        // * Marks the present position inp the stream.
        // *
        // * @exception  IOException  If an I/O error occurs
        // */

        //public void Mark(int readAheadLimit) {
        //    inp.Mark(readAheadLimit);
        //}

        ///**
        // * Resets the stream.
        // *
        // * @exception  IOException  If an I/O error occurs
        // */

        //public void Reset() {
        //    inp.Reset();
        //}

        override public void Close() {
            inp.Close();
        }
    }
}