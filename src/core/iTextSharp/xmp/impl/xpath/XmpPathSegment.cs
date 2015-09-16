//Copyright (c) 2006, Adobe Systems Incorporated
//All rights reserved.
//
//        Redistribution and use in source and binary forms, with or without
//        modification, are permitted provided that the following conditions are met:
//        1. Redistributions of source code must retain the above copyright
//        notice, this list of conditions and the following disclaimer.
//        2. Redistributions in binary form must reproduce the above copyright
//        notice, this list of conditions and the following disclaimer in the
//        documentation and/or other materials provided with the distribution.
//        3. All advertising materials mentioning features or use of this software
//        must display the following acknowledgement:
//        This product includes software developed by the Adobe Systems Incorporated.
//        4. Neither the name of the Adobe Systems Incorporated nor the
//        names of its contributors may be used to endorse or promote products
//        derived from this software without specific prior written permission.
//
//        THIS SOFTWARE IS PROVIDED BY ADOBE SYSTEMS INCORPORATED ''AS IS'' AND ANY
//        EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//        WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//        DISCLAIMED. IN NO EVENT SHALL ADOBE SYSTEMS INCORPORATED BE LIABLE FOR ANY
//        DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//        (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//        LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//        ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//        (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//        SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//        http://www.adobe.com/devnet/xmp/library/eula-xmp-library-java.html

namespace iTextSharp.xmp.impl.xpath {
    /// <summary>
    /// A segment of a parsed <code>XmpPath</code>.
    ///  
    /// @since   23.06.2006
    /// </summary>
    public class XmpPathSegment {
        /// <summary>
        /// flag if segment is an alias </summary>
        private bool _alias;

        /// <summary>
        /// alias form if applicable </summary>
        private uint _aliasForm;

        /// <summary>
        /// kind of the path segment </summary>
        private uint _kind;

        /// <summary>
        /// name of the path segment </summary>
        private string _name;


        /// <summary>
        /// Constructor with initial values.
        /// </summary>
        /// <param name="name"> the name of the segment </param>
        public XmpPathSegment(string name) {
            _name = name;
        }


        /// <summary>
        /// Constructor with initial values.
        /// </summary>
        /// <param name="name"> the name of the segment </param>
        /// <param name="kind"> the kind of the segment </param>
        public XmpPathSegment(string name, uint kind) {
            _name = name;
            _kind = kind;
        }


        /// <returns> Returns the kind. </returns>
        public virtual uint Kind {
            get { return _kind; }
            set { _kind = value; }
        }


        /// <returns> Returns the name. </returns>
        public virtual string Name {
            get { return _name; }
            set { _name = value; }
        }


        /// <param name="alias"> the flag to set </param>
        public virtual bool Alias {
            set { _alias = value; }
            get { return _alias; }
        }


        /// <returns> Returns the aliasForm if this segment has been created by an alias. </returns>
        public virtual uint AliasForm {
            get { return _aliasForm; }
            set { _aliasForm = value; }
        }


        /// <seealso cref= Object#toString() </seealso>
        public override string ToString() {
            switch (_kind) {
                case XmpPath.STRUCT_FIELD_STEP:
                case XmpPath.ARRAY_INDEX_STEP:
                case XmpPath.QUALIFIER_STEP:
                case XmpPath.ARRAY_LAST_STEP:
                    return _name;
                case XmpPath.QUAL_SELECTOR_STEP:
                case XmpPath.FIELD_SELECTOR_STEP:
                    return _name;

                default:
                    // no defined step
                    return _name;
            }
        }
    }
}
