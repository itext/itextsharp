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

namespace iTextSharp.xmp.options {
    /// <summary>
    /// Options for <code>XMPIterator</code> construction.
    /// 
    /// @since 24.01.2006
    /// </summary>
    public sealed class IteratorOptions : XmpOptions {
        /// <summary>
        /// Just do the immediate children of the root, default is subtree. </summary>
        public const uint JUST_CHILDREN = 0x0100;

        /// <summary>
        /// Just do the leaf nodes, default is all nodes in the subtree.
        ///  Bugfix #2658965: If this option is set the Iterator returns the namespace 
        ///  of the leaf instead of the namespace of the base property. 
        /// </summary>
        public const uint JUST_LEAFNODES = 0x0200;

        /// <summary>
        /// Return just the leaf part of the path, default is the full path. </summary>
        public const uint JUST_LEAFNAME = 0x0400;

        //	/** Include aliases, default is just actual properties. <em>Note:</em> Not supported. 
        //	 *  @deprecated it is commonly preferred to work with the base properties */
        //	public static final int INCLUDE_ALIASES = 0x0800;
        /// <summary>
        /// Omit all qualifiers. </summary>
        public const uint OMIT_QUALIFIERS = 0x1000;


        /// <returns> Returns whether the option is set. </returns>
        public bool JustChildren {
            get { return GetOption(JUST_CHILDREN); }
            set { SetOption(JUST_CHILDREN, value); }
        }


        /// <returns> Returns whether the option is set. </returns>
        public bool JustLeafname {
            get { return GetOption(JUST_LEAFNAME); }
            set { SetOption(JUST_LEAFNAME, value); }
        }


        /// <returns> Returns whether the option is set. </returns>
        public bool JustLeafnodes {
            get { return GetOption(JUST_LEAFNODES); }
            set { SetOption(JUST_LEAFNODES, value); }
        }


        /// <returns> Returns whether the option is set. </returns>
        public bool OmitQualifiers {
            get { return GetOption(OMIT_QUALIFIERS); }
            set { SetOption(OMIT_QUALIFIERS, value); }
        }

        /// <seealso cref= Options#getValidOptions() </seealso>
        protected internal override uint ValidOptions {
            get { return JUST_CHILDREN | JUST_LEAFNODES | JUST_LEAFNAME | OMIT_QUALIFIERS; }
        }


        /// <seealso cref= Options#defineOptionName(int) </seealso>
        protected internal override string DefineOptionName(uint option) {
            switch (option) {
                case JUST_CHILDREN:
                    return "JUST_CHILDREN";
                case JUST_LEAFNODES:
                    return "JUST_LEAFNODES";
                case JUST_LEAFNAME:
                    return "JUST_LEAFNAME";
                case OMIT_QUALIFIERS:
                    return "OMIT_QUALIFIERS";
                default:
                    return null;
            }
        }
    }
}
