using System;
using System.Collections;
using System.IO;
using System.Text;

using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Asn1.Utilities
{
    public sealed class Asn1Dump
    {
		private static readonly string NewLine = Platform.NewLine;

		private Asn1Dump()
        {
        }

        private const string Tab = "    ";
        private const int SampleSize = 32;

        /**
         * dump a Der object as a formatted string with indentation
         *
         * @param obj the Asn1Object to be dumped out.
         */
        private static string AsString(
            string		indent,
            bool		verbose,
            Asn1Object	obj)
        {
            if (obj is Asn1Sequence)
            {
                StringBuilder buf = new StringBuilder(indent);

				string tab = indent + Tab;

                if (obj is BerSequence)
                {
                    buf.Append("BER Sequence");
                }
                else if (obj is DerSequence)
                {
                    buf.Append("DER Sequence");
                }
                else
                {
                    buf.Append("Sequence");
                }

                buf.Append(NewLine);

				foreach (Asn1Encodable o in ((Asn1Sequence)obj))
				{
                    if (o == null || o is Asn1Null)
                    {
                        buf.Append(tab);
                        buf.Append("NULL");
                        buf.Append(NewLine);
                    }
                    else
                    {
                        buf.Append(AsString(tab, verbose, o.ToAsn1Object()));
                    }
                }
                return buf.ToString();
            }
            else if (obj is DerTaggedObject)
            {
                StringBuilder buf = new StringBuilder();
                string tab = indent + Tab;

				buf.Append(indent);
                if (obj is BerTaggedObject)
                {
                    buf.Append("BER Tagged [");
                }
                else
                {
                    buf.Append("Tagged [");
                }

				DerTaggedObject o = (DerTaggedObject)obj;

				buf.Append(((int)o.TagNo).ToString());
                buf.Append(']');

				if (!o.IsExplicit())
                {
                    buf.Append(" IMPLICIT ");
                }

				buf.Append(NewLine);

				if (o.IsEmpty())
                {
                    buf.Append(tab);
                    buf.Append("EMPTY");
                    buf.Append(NewLine);
                }
                else
                {
                    buf.Append(AsString(tab, verbose, o.GetObject()));
                }

				return buf.ToString();
            }
            else if (obj is BerSet)
            {
                StringBuilder buf = new StringBuilder();
                string tab = indent + Tab;

				buf.Append(indent);
                buf.Append("BER Set");
                buf.Append(NewLine);

				foreach (Asn1Encodable o in ((Asn1Set)obj))
				{
                    if (o == null)
                    {
                        buf.Append(tab);
                        buf.Append("NULL");
                        buf.Append(NewLine);
                    }
                    else
                    {
                        buf.Append(AsString(tab, verbose, o.ToAsn1Object()));
                    }
                }

				return buf.ToString();
            }
            else if (obj is DerSet)
            {
                StringBuilder buf = new StringBuilder();
                string tab = indent + Tab;

				buf.Append(indent);
                buf.Append("DER Set");
                buf.Append(NewLine);

				foreach (Asn1Encodable o in ((Asn1Set)obj))
				{
                    if (o == null)
                    {
                        buf.Append(tab);
                        buf.Append("NULL");
                        buf.Append(NewLine);
                    }
                    else
                    {
                        buf.Append(AsString(tab, verbose, o.ToAsn1Object()));
                    }
                }

				return buf.ToString();
            }
            else if (obj is DerObjectIdentifier)
            {
                return indent + "ObjectIdentifier(" + ((DerObjectIdentifier)obj).Id + ")" + NewLine;
            }
            else if (obj is DerBoolean)
            {
                return indent + "Boolean(" + ((DerBoolean)obj).IsTrue + ")" + NewLine;
            }
            else if (obj is DerInteger)
            {
                return indent + "Integer(" + ((DerInteger)obj).Value + ")" + NewLine;
            }
			else if (obj is BerOctetString)
			{
				byte[] octets = ((Asn1OctetString)obj).GetOctets();
				string extra = verbose ? dumpBinaryDataAsString(indent, octets) : "";
				return indent + "BER Octet String" + "[" + octets.Length + "] " + extra + NewLine;
			}
            else if (obj is DerOctetString)
            {
				byte[] octets = ((Asn1OctetString)obj).GetOctets();
				string extra = verbose ? dumpBinaryDataAsString(indent, octets) : "";
				return indent + "DER Octet String" + "[" + octets.Length + "] " + extra + NewLine;
			}
			else if (obj is DerBitString)
			{
				DerBitString bt = (DerBitString)obj; 
				byte[] bytes = bt.GetBytes();
				string extra = verbose ? dumpBinaryDataAsString(indent, bytes) : "";
				return indent + "DER Bit String" + "[" + bytes.Length + ", " + bt.PadBits + "] " + extra + NewLine;
			}
            else if (obj is DerIA5String)
            {
                return indent + "IA5String(" + ((DerIA5String)obj).GetString() + ") " + NewLine;
            }
			else if (obj is DerUtf8String)
			{
				return indent + "UTF8String(" + ((DerUtf8String)obj).GetString() + ") " + NewLine;
			}
            else if (obj is DerPrintableString)
            {
                return indent + "PrintableString(" + ((DerPrintableString)obj).GetString() + ") " + NewLine;
            }
            else if (obj is DerVisibleString)
            {
                return indent + "VisibleString(" + ((DerVisibleString)obj).GetString() + ") " + NewLine;
            }
            else if (obj is DerBmpString)
            {
                return indent + "BMPString(" + ((DerBmpString)obj).GetString() + ") " + NewLine;
            }
            else if (obj is DerT61String)
            {
                return indent + "T61String(" + ((DerT61String)obj).GetString() + ") " + NewLine;
            }
            else if (obj is DerUtcTime)
            {
                return indent + "UTCTime(" + ((DerUtcTime)obj).TimeString + ") " + NewLine;
            }
			else if (obj is DerGeneralizedTime)
			{
				return indent + "GeneralizedTime(" + ((DerGeneralizedTime)obj).GetTime() + ") " + NewLine;
			}
            else if (obj is DerUnknownTag)
            {
				byte[] hex = Hex.Encode(((DerUnknownTag)obj).GetData());
                return indent + "Unknown " + ((int)((DerUnknownTag)obj).Tag).ToString("X") + " "
                    + Encoding.ASCII.GetString(hex, 0, hex.Length) + NewLine;
            }
            else if (obj is BerApplicationSpecific)
            {
                return outputApplicationSpecific("BER", indent, verbose, (BerApplicationSpecific)obj);
            }
            else if (obj is DerApplicationSpecific)
            {
                return outputApplicationSpecific("DER", indent, verbose, (DerApplicationSpecific)obj);
            }
            else
            {
                return indent + obj.ToString() + NewLine;
            }
        }

        private static string outputApplicationSpecific(
            string					type,
            string					indent,
            bool					verbose,
            DerApplicationSpecific	app)
        {
            StringBuilder buf = new StringBuilder();

            if (app.IsConstructed())
            {
                try
                {
                    Asn1Sequence s = Asn1Sequence.GetInstance(app.GetObject(Asn1Tags.Sequence));
                    buf.Append(indent + type + " ApplicationSpecific[" + app.ApplicationTag + "]" + NewLine);
                    foreach (Asn1Encodable ae in s)
                    {
                    	buf.Append(AsString(indent + Tab, verbose, ae.ToAsn1Object()));
                    }
                }
                catch (IOException e)
                {
                    buf.Append(e);
                }
                return buf.ToString();
            }

            return indent + type + " ApplicationSpecific[" + app.ApplicationTag + "] ("
                + Encoding.ASCII.GetString(Hex.Encode(app.GetContents())) + ")" + NewLine;
        }

		[Obsolete("Use version accepting Asn1Encodable")]
		public static string DumpAsString(
            object   obj)
        {
            if (obj is Asn1Encodable)
            {
                return AsString("", false, ((Asn1Encodable)obj).ToAsn1Object());
            }

            return "unknown object type " + obj.ToString();
        }

		/**
		 * dump out a DER object as a formatted string, in non-verbose mode
		 *
		 * @param obj the Asn1Encodable to be dumped out.
		 * @return  the resulting string.
		 */
		public static string DumpAsString(
			Asn1Encodable obj)
		{
			return DumpAsString(obj, false);
		}

		/**
		 * Dump out the object as a string
		 *
		 * @param obj the Asn1Encodable to be dumped out.
		 * @param verbose  if true, dump out the contents of octet and bit strings.
		 * @return  the resulting string.
		 */
		public static string DumpAsString(
			Asn1Encodable	obj,
			bool			verbose)
		{
			return AsString("", verbose, obj.ToAsn1Object());
		}

		private static string dumpBinaryDataAsString(string indent, byte[] bytes)
		{
			indent += Tab;

			StringBuilder buf = new StringBuilder(NewLine);

			for (int i = 0; i < bytes.Length; i += SampleSize)
			{
				if (bytes.Length - i > SampleSize)
				{
					buf.Append(indent);
					buf.Append(Encoding.ASCII.GetString(Hex.Encode(bytes, i, SampleSize)));
					buf.Append(Tab);
					buf.Append(calculateAscString(bytes, i, SampleSize));
					buf.Append(NewLine);
				}
				else
				{
					buf.Append(indent);
					buf.Append(Encoding.ASCII.GetString(Hex.Encode(bytes, i, bytes.Length - i)));
					for (int j = bytes.Length - i; j != SampleSize; j++)
					{
						buf.Append("  ");
					}
					buf.Append(Tab);
					buf.Append(calculateAscString(bytes, i, bytes.Length - i));
					buf.Append(NewLine);
				}
			}

			return buf.ToString();
		}

		private static string calculateAscString(
			byte[]	bytes,
			int		off,
			int		len)
		{
			StringBuilder buf = new StringBuilder();

			for (int i = off; i != off + len; i++)
			{
				char c = (char)bytes[i]; 
				if (c >= ' ' && c <= '~')
				{
					buf.Append(c);
				}
			}

			return buf.ToString();
		}
    }
}
