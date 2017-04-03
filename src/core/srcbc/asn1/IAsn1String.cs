using System;

namespace Org.BouncyCastle.Asn1
{
    /**
     * basic interface for Der string objects.
     */
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public interface IAsn1String
    {
        string GetString();
    }
}
