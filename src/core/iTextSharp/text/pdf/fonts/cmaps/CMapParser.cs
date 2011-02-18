using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;
using System.util;
using iTextSharp.text.error_messages;
/**
 * Copyright (c) 2005-2006, www.fontbox.org
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice,
 *    this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 3. Neither the name of fontbox; nor the names of its
 *    contributors may be used to endorse or promote products derived from this
 *    software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED.  IN NO EVENT SHALL THE REGENTS OR CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
 * ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 * http://www.fontbox.org
 *
 */
namespace iTextSharp.text.pdf.fonts.cmaps {

    /**
     * This will parser a CMap stream.
     *
     * @author <a href="mailto:ben@benlitchfield.com">Ben Litchfield</a>
     * @version $Revision: 4242 $
     * @since   2.1.4
     */
    public class CMapParser
    {
        private const String BEGIN_CODESPACE_RANGE = "begincodespacerange";
        private const String BEGIN_BASE_FONT_CHAR = "beginbfchar";
        private const String BEGIN_BASE_FONT_RANGE = "beginbfrange";

        private const String MARK_END_OF_DICTIONARY = ">>";
        private const String MARK_END_OF_ARRAY = "]";

        private byte[] tokenParserByteBuffer = new byte[512];

        /**
         * Creates a new instance of CMapParser.
         */
        public CMapParser()
        {
        }

        /**
         * This will parse the stream and create a cmap object.
         *
         * @param input The CMAP stream to parse.
         * @return The parsed stream as a java object.
         *
         * @throws IOException If there is an error parsing the stream.
         */
        public CMap Parse( Stream input )
        {
            PushbackStream cmapStream = new PushbackStream( input );
            CMap result = new CMap();
            Object token = null;
            while ( (token = ParseNextToken( cmapStream )) != null )
            {
                if ( token is Operator )
                {
                    Operator op = (Operator)token;
                    if ( op.op.Equals( BEGIN_CODESPACE_RANGE ) )
                    {
                        while (true)
                        {
                            Object nx = ParseNextToken( cmapStream );
                            if (nx is Operator && ((Operator)nx).op.Equals("endcodespacerange"))
                                break;
                            byte[] startRange = (byte[])nx;
                            byte[] endRange = (byte[])ParseNextToken( cmapStream );
                            CodespaceRange range = new CodespaceRange();
                            range.SetStart( startRange );
                            range.SetEnd( endRange );
                            result.AddCodespaceRange( range );
                        }
                    }
                    else if ( op.op.Equals( BEGIN_BASE_FONT_CHAR ) )
                    {
                        while (true)
                        {
                            Object nx = ParseNextToken( cmapStream );
                            if (nx is Operator && ((Operator)nx).op.Equals("endbfchar"))
                                break;
                            byte[] inputCode = (byte[])nx;
                            Object nextToken = ParseNextToken( cmapStream );
                            if ( nextToken is byte[] )
                            {
                                byte[] bytes = (byte[])nextToken;
                                String value = CreateStringFromBytes( bytes );
                                result.AddMapping( inputCode, value );
                            }
                            else if ( nextToken is LiteralName )
                            {
                                result.AddMapping( inputCode, ((LiteralName)nextToken).name );
                            }
                            else
                            {
                                throw new IOException(MessageLocalization.GetComposedMessage("error.parsing.cmap.beginbfchar.expected.cosstring.or.cosname.and.not.1", nextToken));
                            }
                        }
                    }
                   else if ( op.op.Equals( BEGIN_BASE_FONT_RANGE ) )
                   {
                        while (true)
                        {
                            Object nx = ParseNextToken( cmapStream );
                            if (nx is Operator && ((Operator)nx).op.Equals("endbfrange"))
                                break;
                            byte[] startCode = (byte[])nx;
                            byte[] endCode = (byte[])ParseNextToken( cmapStream );
                            Object nextToken = ParseNextToken( cmapStream );
                            IList<byte[]> array = null;
                            byte[] tokenBytes = null;
                            if ( nextToken is IList<byte[]> )
                            {
                                array = (IList<byte[]>)nextToken;
                                tokenBytes = array[0];
                            }
                            else
                            {
                                tokenBytes = (byte[])nextToken;
                            }

                            String value = null;

                            int arrayIndex = 0;
                            bool done = false;
                            while ( !done )
                            {
                                if ( Compare( startCode, endCode ) >= 0 )
                                {
                                    done = true;
                                }
                                value = CreateStringFromBytes( tokenBytes );
                                result.AddMapping( startCode, value );
                                Increment( startCode );

                                if ( array == null )
                                {
                                    Increment( tokenBytes );
                                }
                                else
                                {
                                    arrayIndex++;
                                    if ( arrayIndex < array.Count )
                                    {
                                        tokenBytes = array[arrayIndex];
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        private Object ParseNextToken( PushbackStream pis ) 
        {
            Object retval = null;
            int nextByte = pis.ReadByte();
            //skip whitespace
            while ( nextByte == 0x09 || nextByte == 0x20 || nextByte == 0x0D || nextByte == 0x0A )
            {
                nextByte = pis.ReadByte();
            }
            switch ( nextByte )
            {
                case '%':
                {
                    //header operations, for now return the entire line
                    //may need to smarter in the future
                    StringBuilder buffer = new StringBuilder();
                    buffer.Append( (char)nextByte );
                    ReadUntilEndOfLine( pis, buffer );
                    retval = buffer.ToString();
                    break;
                }
                case '(':
                {
                    StringBuilder buffer = new StringBuilder();
                    int stringByte = pis.ReadByte();

                    while ( stringByte != -1 && stringByte != ')' )
                    {
                        buffer.Append( (char)stringByte );
                        stringByte = pis.ReadByte();
                    }
                    retval = buffer.ToString();
                    break;
                }
                case '>':
                {
                    int secondCloseBrace = pis.ReadByte();
                    if ( secondCloseBrace == '>' )
                    {
                        retval = MARK_END_OF_DICTIONARY;
                    }
                    else
                    {
                        throw new IOException(MessageLocalization.GetComposedMessage("error.expected.the.end.of.a.dictionary"));
                    }
                    break;
                }
                case ']':
                {
                    retval = MARK_END_OF_ARRAY;
                    break;
                }
                case '[':
                {
                    IList<byte[]> list = new List<byte[]>();

                    Object nextToken = ParseNextToken( pis );
                    while (!MARK_END_OF_ARRAY.Equals(nextToken) )
                    {
                        list.Add( (byte[])nextToken );
                        nextToken = ParseNextToken( pis );
                    }
                    retval = list;
                    break;
                }
                case '<':
                {
                    int theNextByte = pis.ReadByte();
                    if ( theNextByte == '<' )
                    {
                        IDictionary<String, Object> result = new Dictionary<String, Object>();
                        //we are reading a dictionary
                        Object key = ParseNextToken( pis );
                        while ( key is LiteralName && !MARK_END_OF_DICTIONARY.Equals(key) )
                        {
                            Object value = ParseNextToken( pis );
                            result[((LiteralName)key).name] = value;
                            key = ParseNextToken( pis );
                        }
                        retval = result;
                    }
                    else
                    {
                        //won't read more than 512 bytes

                        int multiplyer = 16;
                        int bufferIndex = -1;
                        while ( theNextByte != -1 && theNextByte != '>' )
                        {
                            int intValue = 0;
                            if ( theNextByte >= '0' && theNextByte <= '9' )
                            {
                                intValue = theNextByte - '0';
                            }
                            else if ( theNextByte >= 'A' && theNextByte <= 'F' )
                            {
                                intValue = 10 + theNextByte - 'A';
                            }
                            else if ( theNextByte >= 'a' && theNextByte <= 'f' )
                            {
                                intValue = 10 + theNextByte - 'a';
                            }
                            else if( theNextByte == 0x20 || theNextByte == 0x09 )
                            {
                                // skipping whitespaces - from pdf's generated by Mac osx
                                theNextByte = pis.ReadByte();
                                continue;
                            }
                            else
                            {
                                throw new IOException(MessageLocalization.GetComposedMessage("error.expected.hex.character.and.not.char.thenextbyte.1", theNextByte));
                            }
                            intValue *= multiplyer;
                            if ( multiplyer == 16 )
                            {
                                bufferIndex++;
                                tokenParserByteBuffer[bufferIndex] = 0;
                                multiplyer = 1;
                            }
                            else
                            {
                                multiplyer = 16;
                            }
                            tokenParserByteBuffer[bufferIndex]+= (byte)intValue;
                            theNextByte = pis.ReadByte();
                        }
                        byte[] finalResult = new byte[bufferIndex+1];
                        System.Array.Copy(tokenParserByteBuffer,0,finalResult, 0, bufferIndex+1);
                        retval = finalResult;
                    }
                    break;
                }
                case '/':
                {
                    StringBuilder buffer = new StringBuilder();
                    int stringByte = pis.ReadByte();

                    while ( !IsWhitespaceOrEOF( stringByte ) )
                    {
                        buffer.Append( (char)stringByte );
                        stringByte = pis.ReadByte();
                    }
                    retval = new LiteralName( buffer.ToString() );
                    break;
                }
                case -1:
                {
                    //EOF return null;
                    break;
                }
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                {
                    StringBuilder buffer = new StringBuilder();
                    buffer.Append( (char)nextByte );
                    nextByte = pis.ReadByte();

                    while ( !IsWhitespaceOrEOF( nextByte ) &&
                            (Char.IsDigit( (char)nextByte )||
                             nextByte == '.' ) )
                    {
                        buffer.Append( (char)nextByte );
                        nextByte = pis.ReadByte();
                    }
                    pis.Unread( nextByte );
                    String value = buffer.ToString();
                    if ( value.IndexOf( '.' ) >=0 )
                    {
                        retval = double.Parse(value, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        retval = int.Parse(value, CultureInfo.InvariantCulture);
                    }
                    break;
                }
                default:
                {
                    StringBuilder buffer = new StringBuilder();
                    buffer.Append( (char)nextByte );
                    nextByte = pis.ReadByte();

                    while ( !IsWhitespaceOrEOF( nextByte ) )
                    {
                        buffer.Append( (char)nextByte );
                        nextByte = pis.ReadByte();
                    }
                    retval = new Operator( buffer.ToString() );

                    break;
                }
            }
            return retval;
        }

        private void ReadUntilEndOfLine( Stream pis, StringBuilder buf ) 
        {
            int nextByte = pis.ReadByte();
            while ( nextByte != -1 && nextByte != 0x0D && nextByte != 0x0A )
            {
                buf.Append( (char)nextByte );
                nextByte = pis.ReadByte();
            }
        }

        private bool IsWhitespaceOrEOF( int aByte )
        {
            return aByte == -1 || aByte == 0x20 || aByte == 0x0D || aByte == 0x0A;
        }


        private void Increment( byte[] data )
        {
            Increment( data, data.Length-1 );
        }

        private void Increment( byte[] data, int position )
        {
            if ( position > 0 && (data[position]+256)%256 == 255 )
            {
                data[position]=0;
                Increment( data, position-1);
            }
            else
            {
                data[position] = (byte)(data[position]+1);
            }
        }

        private String CreateStringFromBytes( byte[] bytes )
        {
            String retval = null;
            if ( bytes.Length == 1 )
            {
                retval = Convert.ToString((char)bytes[0]);
            }
            else
            {
                retval = Encoding.BigEndianUnicode.GetString(bytes);
            }
            return retval;
        }

        private int Compare( byte[] first, byte[] second )
        {
            int retval = 1;
            bool done = false;
            for ( int i=0; i<first.Length && !done; i++ )
            {
                if ( first[i] == second[i] )
                {
                    //move to next position
                }
                else if ( (first[i]+256)%256 < (second[i]+256)%256 )
                {
                    done = true;
                    retval = -1;
                }
                else
                {
                    done = true;
                    retval = 1;
                }
            }
            return retval;
        }

        /**
         * Internal class.
         */
        private class LiteralName
        {
            public String name;
            public LiteralName( String theName )
            {
                name = theName;
            }
        }

        /**
         * Internal class.
         */
        private class Operator
        {
            public String op;
            public Operator( String theOp )
            {
                op = theOp;
            }
        }

        /**
         * A simple class to test parsing of cmap files.
         *
         * @param args Some command line arguments.
         *
         * @throws Exception If there is an error parsing the file.
         */
        //public static void Main( String[] args ) throws Exception
        //{
        //    if ( args.length != 1 )
        //    {
        //        System.err.Println( "usage: java org.pdfbox.cmapparser.CMapParser <CMAP File>" );
        //        System.Exit( -1 );
        //    }
        //    CMapParser parser = new CMapParser(  );
        //    CMap result = parser.Parse( new FileInputStream( args[0] ) );
        //    System.out.Println( "Result:" + result );
        //}
    }
}