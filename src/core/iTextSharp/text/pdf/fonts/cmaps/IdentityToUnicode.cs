using System;

namespace iTextSharp.text.pdf.fonts.cmaps
{
public class IdentityToUnicode {
    private static CMapToUnicode identityCNS;
    private static CMapToUnicode identityJapan;
    private static CMapToUnicode identityKorea;
    private static CMapToUnicode identityGB;
    
    public static CMapToUnicode GetMapFromOrdering(String ordering){
        if (ordering.Equals("CNS1")) {
            if (identityCNS == null) {
                CMapUniCid uni = CMapCache.GetCachedCMapUniCid("UniCNS-UTF16-H");
                if (uni == null)
                    return null;
                identityCNS = uni.ExportToUnicode();
            }
            return identityCNS;
        }
        else if (ordering.Equals("Japan1")) {
            if (identityJapan == null) {
                CMapUniCid uni = CMapCache.GetCachedCMapUniCid("UniJIS-UTF16-H");
                if (uni == null)
                    return null;
                identityJapan = uni.ExportToUnicode();
            }
            return identityJapan;
        }
        else if (ordering.Equals("Korea1")) {
            if (identityKorea == null) {
                CMapUniCid uni = CMapCache.GetCachedCMapUniCid("UniKS-UTF16-H");
                if (uni == null)
                    return null;
                identityKorea = uni.ExportToUnicode();
            }
            return identityKorea;
        }
        else if (ordering.Equals("GB1")) {
            if (identityGB == null) {
                CMapUniCid uni = CMapCache.GetCachedCMapUniCid("UniGB-UTF16-H");
                if (uni == null)
                    return null;
                identityGB = uni.ExportToUnicode();
            }
            return identityGB;
        }
        return null;
    }
}
}