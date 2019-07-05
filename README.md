### **PLEASE NOTE: iTextSharp is EOL, and has been replaced by [iText 7][itext7]. Only security fixes will be added**
 
We HIGHLY recommend customers use iText 7 for new projects, and to consider moving existing projects from iTextSharp to iText 7 to benefit from the many improvements such as:
 
- HTML to PDF (PDF/UA) conversion
- PDF Redaction
- SVG support
- Better language support: Indic, Thai, Khmer, Arabic, Hebrew. (Close-source addon)
- PDF Debugging for your IDE
- Data Extraction
- Better continued support and bugfixes
- More modular, extensible handling of your document workflow
- Extra practical add-ons
- Encryption, hashing & digital signatures


### [iTextSharp][itext] consists of several dlls.

The main release contains:
- ```itextsharp.dll```: the core library
- ```itextsharp.xtra.dll```: extra functionality (PDF 2!)
- ```itextsharp.pdfa.dll```: PDF/A-related functionality
- ```itextsharp.xmlworker.dll```: XML (and HTML) functionality

This project is hosted on https://github.com/itext/itextsharp

You can find the latest release here:
- https://github.com/itext/itextsharp/releases/latest

You can also [build iTextSharp from source][building].

We also have RUPS — a Java tool that can help you debug PDFs. It's hosted on http://github.com/itext/rups

If you have an idea on how to improve iTextSharp and you want to submit code,
please read our [Contribution Guidelines][contributing].

iTextSharp is licensed as [AGPL][agpl] software.

AGPL is a free / open source software license.

This doesn't mean the software is [gratis][gratis]!

Buying a license is mandatory as soon as you develop commercial activities
distributing the iText software inside your product or deploying it on a network
without disclosing the source code of your own applications under the AGPL license.
These activities include:
- offering paid services to customers as an ASP
- serving PDFs on the fly in the cloud or in a web application
- shipping iText with a closed source product

Contact sales for more info: http://itextpdf.com/sales

[agpl]: LICENSE.md
[building]: BUILDING.md
[contributing]: CONTRIBUTING.md
[gratis]: https://en.wikipedia.org/wiki/Gratis_versus_libre
[itext]: http://itextpdf.com/
[itext7]: https://github.com/itext/itext7-dotnet