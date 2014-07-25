using System.Collections;
using System.Text;

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
    using XMPError = XmpError;
    using XmpException = XmpException;

    /// <summary>
    /// The base class for a collection of 32 flag bits. Individual flags are defined as enum value bit
    /// masks. Inheriting classes add convenience accessor methods.
    /// 
    /// @since 24.01.2006
    /// </summary>
    public abstract class XmpOptions {
        /// <summary>
        /// a map containing the bit names </summary>
        private IDictionary _optionNames;

        /// <summary>
        /// the internal int containing all options </summary>
        private uint _options;


        /// <summary>
        /// The default constructor.
        /// </summary>
        protected XmpOptions() {
            // EMTPY
        }


        /// <summary>
        /// Constructor with the options bit mask. 
        /// </summary>
        /// <param name="options"> the options bit mask </param>
        /// <exception cref="XmpException"> If the options are not correct </exception>
        public XmpOptions(uint options) {
            AssertOptionsValid(options);
            Options = options;
        }

        /// <summary>
        /// Is friendly to access it during the tests. </summary>
        /// <returns> Returns the options. </returns>
        public virtual uint Options {
            get { return _options; }
            set {
                AssertOptionsValid(value);
                _options = value;
            }
        }

        /// <summary>
        /// Creates a human readable string from the set options. <em>Note:</em> This method is quite
        /// expensive and should only be used within tests or as </summary>
        /// <returns> Returns a String listing all options that are set to <code>true</code> by their name,
        /// like &quot;option1 | option4&quot;. </returns>
        public virtual string OptionsString {
            get {
                if (_options != 0) {
                    StringBuilder sb = new StringBuilder();
                    uint theBits = _options;
                    while (theBits != 0) {
                        uint oneLessBit = theBits & (theBits - 1); // clear rightmost one bit
                        uint singleBit = theBits ^ oneLessBit;
                        string bitName = GetOptionName(singleBit);
                        sb.Append(bitName);
                        if (oneLessBit != 0) {
                            sb.Append(" | ");
                        }
                        theBits = oneLessBit;
                    }
                    return sb.ToString();
                }
                return "<none>";
            }
        }

        /// <summary>
        /// To be implemeted by inheritants. </summary>
        /// <returns> Returns a bit mask where all valid option bits are set. </returns>
        protected internal abstract uint ValidOptions { get; }


        /// <summary>
        /// Resets the options.
        /// </summary>
        public virtual void Clear() {
            _options = 0;
        }


        /// <param name="optionBits"> an option bitmask </param>
        /// <returns> Returns true, if this object is equal to the given options.  </returns>
        public virtual bool IsExactly(uint optionBits) {
            return Options == optionBits;
        }


        /// <param name="optionBits"> an option bitmask </param>
        /// <returns> Returns true, if this object contains all given options.  </returns>
        public virtual bool ContainsAllOptions(uint optionBits) {
            return (Options & optionBits) == optionBits;
        }


        /// <param name="optionBits"> an option bitmask </param>
        /// <returns> Returns true, if this object contain at least one of the given options.  </returns>
        public virtual bool ContainsOneOf(uint optionBits) {
            return ((Options) & optionBits) != 0;
        }


        /// <param name="optionBit"> the binary bit or bits that are requested </param>
        /// <returns> Returns if <emp>all</emp> of the requested bits are set or not. </returns>
        protected internal virtual bool GetOption(uint optionBit) {
            return (_options & optionBit) != 0;
        }


        /// <param name="optionBits"> the binary bit or bits that shall be set to the given value </param>
        /// <param name="value"> the boolean value to set </param>
        public virtual void SetOption(uint optionBits, bool value) {
            _options = value ? _options | optionBits : _options & ~optionBits;
        }


        /// <seealso cref= Object#equals(Object) </seealso>
        public override bool Equals(object obj) {
            return Options == ((XmpOptions) obj).Options;
        }


        /// <seealso cref= java.lang.Object#hashCode() </seealso>
        public override int GetHashCode() {
            return (int) Options;
        }


        /// <returns> Returns the options as hex bitmask.  </returns>
        public override string ToString() {
            return "0x" + _options.ToString("X");
        }


        /// <summary>
        /// To be implemeted by inheritants. </summary>
        /// <param name="option"> a single, valid option bit. </param>
        /// <returns> Returns a human readable name for an option bit. </returns>
        protected internal abstract string DefineOptionName(uint option);


        /// <summary>
        /// The inheriting option class can do additional checks on the options.
        /// <em>Note:</em> For performance reasons this method is only called 
        /// when setting bitmasks directly.
        /// When get- and set-methods are used, this method must be called manually,
        /// normally only when the Options-object has been created from a client
        /// (it has to be made public therefore).
        /// </summary>
        /// <param name="options"> the bitmask to check. </param>
        /// <exception cref="XmpException"> Thrown if the options are not consistent. </exception>
        protected internal virtual void AssertConsistency(uint options) {
            // empty, no checks
        }


        /// <summary>
        /// Checks options before they are set.
        /// First it is checked if only defined options are used,
        /// second the additional <seealso cref="AssertConsistency(uint)"/>-method is called.
        /// </summary>
        /// <param name="options"> the options to check </param>
        /// <exception cref="XmpException"> Thrown if the options are invalid. </exception>
        private void AssertOptionsValid(uint options) {
            uint invalidOptions = options & ~ValidOptions;
            if (invalidOptions == 0) {
                AssertConsistency(options);
            }
            else {
                throw new XmpException("The option bit(s) 0x" + invalidOptions.ToString("X") + " are invalid!",
                                       XmpError.BADOPTIONS);
            }
        }


        /// <summary>
        /// Looks up or asks the inherited class for the name of an option bit.
        /// Its save that there is only one valid option handed into the method. </summary>
        /// <param name="option"> a single option bit </param>
        /// <returns> Returns the option name or undefined. </returns>
        private string GetOptionName(uint option) {
            IDictionary optionsNames = ProcureOptionNames();

            uint? key = option;
            string result = (string) optionsNames[key];
            if (result == null) {
                result = DefineOptionName(option);
                if (result != null) {
                    optionsNames[key] = result;
                }
                else {
                    result = "<option name not defined>";
                }
            }

            return result;
        }


        /// <returns> Returns the optionNames map and creates it if required. </returns>
        private IDictionary ProcureOptionNames() {
            _optionNames = _optionNames ?? new Hashtable();
            return _optionNames;
        }
    }
}
