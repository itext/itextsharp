using System;
using System.Text;

namespace iTextSharp.text.log {
    /**
     * Implementation of the Counter interface that doesn't do anything.
     */
    public class DefaultCounter : ICounter {
        private int count = 0;
        private int level = 0;
		private readonly int[] repeat = {10000, 5000, 1000};
		private int repeat_level = 10000;

        /**
         * @param klass
         * @return this Counter implementation
         * @see com.itextpdf.text.log.Counter#getCounter(java.lang.Class)
         */

        public ICounter GetCounter(Type klass) {
            return this;
        }

        /**
         * @see com.itextpdf.text.log.Counter#read(long)
         */
        public void Read(long l) {
            PlusOne();
        }

        /**
         * @see com.itextpdf.text.log.Counter#written(long)
         */
        public void Written(long l) {
            PlusOne();
        }

        private void PlusOne() {
			if (count++ > repeat_level) {
				if (Version.IsAGPLVersion) {
					level++;
					if (level == 1) {
						repeat_level = repeat[1];
					} else {
						repeat_level = repeat[2];
					}
					Console.WriteLine (Encoding.UTF8.GetString (message));
				}
			}
        }

        private static byte[] message = Convert.FromBase64String(
            "DQoNCllvdSBhcmUgdXNpbmcgaVRleHQgdW5kZXIgdGhlIEFHUEwuDQoNCklmIHR"
            + "oaXMgaXMgeW91ciBpbnRlbnRpb24sIHlvdSBoYXZlIHB1Ymxpc2hlZCB5b3VyIG"
            + "93biBzb3VyY2UgY29kZSBhcyBBR1BMIHNvZnR3YXJlIHRvby4NClBsZWFzZSBsZ"
            + "XQgdXMga25vdyB3aGVyZSB0byBmaW5kIHlvdXIgc291cmNlIGNvZGUgYnkgc2Vu"
            + "ZGluZyBhIG1haWwgdG8gYWdwbEBpdGV4dHBkZi5jb20NCldlJ2QgYmUgaG9ub3J"
            + "lZCB0byBhZGQgaXQgdG8gb3VyIGxpc3Qgb2YgQUdQTCBwcm9qZWN0cyBidWlsdC"
            + "BvbiB0b3Agb2YgaVRleHQgb3IgaVRleHRTaGFycA0KYW5kIHdlJ2xsIGV4cGxha"
            + "W4gaG93IHRvIHJlbW92ZSB0aGlzIG1lc3NhZ2UgZnJvbSB5b3VyIGVycm9yIGxv"
            + "Z3MuDQoNCklmIHRoaXMgd2Fzbid0IHlvdXIgaW50ZW50aW9uLCB5b3UgYXJlIHB"
            + "yb2JhYmx5IHVzaW5nIGlUZXh0IGluIGEgbm9uLWZyZWUgZW52aXJvbm1lbnQuDQ"
            + "pJbiB0aGlzIGNhc2UsIHBsZWFzZSBjb250YWN0IHVzIGJ5IGZpbGxpbmcgb3V0I"
            + "HRoaXMgZm9ybTogaHR0cDovL2l0ZXh0cGRmLmNvbS9zYWxlcw0KSWYgeW91IGFy"
            + "ZSBhIGN1c3RvbWVyLCB3ZSdsbCBleHBsYWluIGhvdyB0byBpbnN0YWxsIHlvdXI"
            + "gbGljZW5zZSBrZXkgdG8gYXZvaWQgdGhpcyBtZXNzYWdlLg0KSWYgeW91J3JlIG"
            + "5vdCBhIGN1c3RvbWVyLCB3ZSdsbCBleHBsYWluIHRoZSBiZW5lZml0cyBvZiBiZ"
            + "WNvbWluZyBhIGN1c3RvbWVyLg0KDQo=");
    }
}
