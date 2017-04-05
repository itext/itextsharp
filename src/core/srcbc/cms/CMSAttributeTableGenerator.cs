using System;
using System.Collections;

using Org.BouncyCastle.Asn1.Cms;

namespace Org.BouncyCastle.Cms
{
	/// <remarks>
	/// The 'Signature' parameter is only available when generating unsigned attributes.
	/// </remarks>
	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public enum CmsAttributeTableParameter
	{
//		const string ContentType = "contentType";
//		const string Digest = "digest";
//		const string Signature = "encryptedDigest";
//		const string DigestAlgorithmIdentifier = "digestAlgID";

		ContentType, Digest, Signature, DigestAlgorithmIdentifier
	}

	[Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public interface CmsAttributeTableGenerator
	{
		AttributeTable GetAttributes(IDictionary parameters);
	}
}
