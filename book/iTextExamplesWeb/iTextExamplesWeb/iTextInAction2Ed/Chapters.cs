/*
 * This class is __NOT__ part of the book "iText in Action - 2nd Edition".
 * it's a helper class to build the examples using VS2008 or higher
 * on your local machine, either in a web context or command line
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Compilation;
using System.Linq;
using System.Reflection;
using System.Web;

namespace kuujinbo.iTextInAction2Ed {
  public class Chapters {
// ===========================================================================
// example output types
    public enum OutputType {
      none,
      pdf,
      zip,
      other,
      not_implemented
    } 
// ---------------------------------------------------------------------------
// properties used for instantiating objects using reflection
    public string ChapterName {get; set;}
    public string ExampleName {get; set;}
// ---------------------------------------------------------------------------
// querystring lookup keys/parameters used to show source code and 
// run web examples
    public const string QS_CHAPTER = "ch";
    public const string QS_CLASS = "ex";
// ---------------------------------------------------------------------------
// sanity checks for web context
    public bool IsValidChapterExample {
      get {
        return 
          !string.IsNullOrEmpty(ChapterName)
          && !string.IsNullOrEmpty(ExampleName)
          && Chapters.Examples.ContainsKey(ChapterName)
          && Chapters.Examples[ChapterName].ContainsKey(ExampleName)
        ;
      }
    }
// determine the type of result file(s)  
    public bool IsPdfResult {
      get { return 
        Chapters.Examples[ChapterName][ExampleName].Equals(
          Chapters.OutputType.pdf
        );     
      }
    }
    public bool IsZipResult {
      get { return 
        Chapters.Examples[ChapterName][ExampleName].Equals(
          Chapters.OutputType.zip
        );     
      }
    }
    public bool IsOtherResult {
      get { return 
        Chapters.Examples[ChapterName][ExampleName].Equals(
          Chapters.OutputType.other
        );     
      }
    }    
    public bool HasResult {
      get {
        return IsPdfResult || IsZipResult || IsOtherResult;
      }
    }
// ---------------------------------------------------------------------------
// create / send output result; either zip or PDF.
    public void SendOutput() {
// create the example class objects using reflection    
      string typeString = string.Format("{0}.{1}.{2}",
        this.GetType().Namespace, ChapterName, ExampleName
      );
      IWriter iw = (IWriter) Activator.CreateInstance(
        Type.GetType(typeString)
      );
      
      HttpContext hc = HttpContext.Current;
// [1] running under web context
      if (hc != null) {
        if (IsZipResult) {
          Utility.SendZipHeaders(
            hc.Response, string.Format("{0}.{1}.zip", ChapterName, ExampleName)
          );
          iw.Write(hc.Response.OutputStream);
        }
        else if (IsPdfResult) {
          Utility.SendPdfHeaders(
            hc.Response, string.Format("{0}.{1}.pdf", ChapterName, ExampleName)
          );
          iw.Write(hc.Response.OutputStream);
        } else if (IsOtherResult) {
          iw.Write(hc.Response.OutputStream);
        }
      }
// [2] running from command line       
      else {
        string fileName = null;
        if (IsZipResult) {
          fileName = Path.Combine(
            Path.Combine(Utility.ResultDirectory, ChapterName), 
            ExampleName + ".zip"
          );
          using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate)) 
          {
            iw.Write(fs);
          }        
        }
        else if (IsPdfResult) {
          fileName = Path.Combine(
            Path.Combine(Utility.ResultDirectory, ChapterName),
            ExampleName + ".pdf"
          );
          using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
          { 
            iw.Write(fs);
          }
/*
 * chapter09 examples that __REQUIRE__ web context; dump text file with 
 * message saying that
 */
        } else if (IsOtherResult) {
          fileName = Path.Combine(
            Path.Combine(Utility.ResultDirectory, ChapterName),
            ExampleName + ".txt"
          );
          using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
          { 
            iw.Write(fs);
          }
        }        
      }
    }      
// ---------------------------------------------------------------------------
// book chapter / example names, with respective output type
    public static readonly Dictionary<string, Dictionary<string, OutputType>> 
    Examples = new Dictionary<string, Dictionary<string, OutputType>> ()
    {
      /*{"KuujinboWeb", new Dictionary<string, OutputType>() 
        {
          {"Chapters", OutputType.none},
          {"IWriter", OutputType.none},
          {"Utility", OutputType.none}
        }
      },      
      {"Intro_1_2", new Dictionary<string, OutputType>() 
        {
          {"Category", OutputType.none},
          {"Country", OutputType.none},
          {"Director", OutputType.none},
          {"Entry", OutputType.none},
          {"FilmFonts", OutputType.none},
          {"Movie", OutputType.none},
          {"PojoFactory", OutputType.none},
          {"PojoToElementFactory", OutputType.none},
          {"Screening", OutputType.none},
          {"AdoDB", OutputType.none}
        }
      },*/      
      {"Chapter01", new Dictionary<string, OutputType>() 
        {
          {"HelloWorld", OutputType.pdf},
          {"HelloWorldColumn", OutputType.pdf},
          {"HelloWorldDirect", OutputType.pdf},
          {"HelloWorldLandscape1", OutputType.pdf},
          {"HelloWorldLandscape2", OutputType.pdf},
          {"HelloWorldLetter", OutputType.pdf},
          {"HelloWorldMaximum", OutputType.pdf},
          {"HelloWorldMemory", OutputType.pdf},
          {"HelloWorldMirroredMargins", OutputType.pdf},
          {"HelloWorldMirroredMarginsTop", OutputType.pdf},
          {"HelloWorldNarrow", OutputType.pdf},
          {"HelloWorldVersion_1_7", OutputType.pdf},
          {"HelloZip", OutputType.zip}
        }
      },     
      {"Chapter02", new Dictionary<string, OutputType>() 
        {
          {"CountryChunks", OutputType.pdf},
          {"DatabaseTest", OutputType.pdf},
          {"DirectorOverview1", OutputType.pdf},
          {"DirectorOverview2", OutputType.pdf},
          {"DirectorOverview3", OutputType.pdf},
          {"DirectorPhrases1", OutputType.pdf},
          {"DirectorPhrases2", OutputType.pdf},
          {"MovieChain", OutputType.pdf},
          {"MovieHistory", OutputType.pdf},
          {"MovieLinks1", OutputType.pdf},
          {"MovieLinks2", OutputType.pdf},
          {"MovieLists1", OutputType.pdf},
          {"MovieLists2", OutputType.pdf},
          {"MovieLists3", OutputType.pdf},
          {"MovieLists4", OutputType.pdf},
          {"MovieParagraphs1", OutputType.pdf},
          {"MovieParagraphs2", OutputType.pdf},
          {"MoviePosters1", OutputType.pdf},
          {"MoviePosters2", OutputType.pdf},
          {"MoviePosters3", OutputType.pdf},
          {"MovieTitles", OutputType.pdf},
          {"PipeSplitCharacter", OutputType.none},
          {"PositionedArrow", OutputType.none},
          {"RiverPhoenix", OutputType.pdf},
          {"StarSeparator", OutputType.none}       
        }
      },
      {"Chapter03", new Dictionary<string, OutputType>() 
        {
          {"ColumnMovies1", OutputType.pdf},
          {"ColumnMovies2", OutputType.pdf},
          {"FestivalOpening", OutputType.pdf},
          {"FoobarFilmFestival", OutputType.pdf},
          {"GraphicsStateStack", OutputType.pdf},
          {"ImageDirect", OutputType.pdf},
          {"ImageInline", OutputType.pdf},
          {"ImageSkew", OutputType.pdf},
          {"MovieCalendar", OutputType.pdf},
          {"MovieColumns1", OutputType.pdf},
          {"MovieColumns2", OutputType.pdf},
          {"MovieColumns3", OutputType.pdf},
          {"MovieColumns4", OutputType.pdf},
          {"MoviePosters", OutputType.pdf},
          {"MovieTemplates", OutputType.pdf},
          {"MovieTextInfo", OutputType.pdf},
          {"MovieTimeBlocks", OutputType.pdf},
          {"MovieTimeTable", OutputType.pdf}       
        }
      }, 
      {"Chapter04", new Dictionary<string, OutputType>() 
        {
          {"CellHeights", OutputType.pdf},
          {"ColumnTable", OutputType.pdf},
          {"ColumnWidths", OutputType.pdf},
          {"HeaderFooter1", OutputType.pdf},
          {"HeaderFooter2", OutputType.pdf},
// change if implemented later          
          {"MemoryTests", OutputType.not_implemented},
          {"MovieCompositeMode", OutputType.pdf},
          {"MovieTextMode", OutputType.pdf},
          {"MyFirstTable", OutputType.pdf},
          {"NestedTable", OutputType.pdf},
          {"NestedTables", OutputType.pdf},
          {"PdfCalendar", OutputType.pdf},
          {"RotationAndColors", OutputType.pdf},
          {"Spacing", OutputType.pdf},
          {"TableAlignment", OutputType.pdf},
          {"TableHeight", OutputType.pdf},
          {"XMen", OutputType.pdf},
          {"Zhang", OutputType.pdf}       
        }
      },   
      {"Chapter05", new Dictionary<string, OutputType>() 
        {
          {"AlternatingBackground", OutputType.pdf},
          {"Hero1", OutputType.pdf},
          {"Hero2", OutputType.pdf},
          {"Hero3", OutputType.pdf},
          {"MovieCountries1", OutputType.pdf},
          {"MovieCountries2", OutputType.pdf},
          {"MovieHistory1", OutputType.pdf},
          {"MovieHistory2", OutputType.pdf},
          {"MovieSlideShow", OutputType.pdf},
          {"MovieYears", OutputType.pdf},
          {"NewPage", OutputType.pdf},
          {"PdfCalendar", OutputType.pdf},
          {"PressPreviews", OutputType.pdf},
          {"RunLengthEvent", OutputType.pdf}
        }
      },      
      {"Chapter06", new Dictionary<string, OutputType>() 
        {
          {"Burst", OutputType.zip},
          {"Concatenate", OutputType.zip},
          {"ConcatenateForms1", OutputType.zip},
          {"ConcatenateForms2", OutputType.zip},
          {"ConcatenateStamp", OutputType.zip},
          {"DataSheets1", OutputType.zip},
          {"DataSheets2", OutputType.zip},
          {"FillDataSheet", OutputType.zip},
          {"FormInformation", OutputType.zip},
          {"ImportingPages1", OutputType.zip},
          {"ImportingPages2", OutputType.zip},
          {"InsertPages", OutputType.zip},
          {"Layers", OutputType.zip},
// change if implemented later                    
          {"MemoryInfo", OutputType.not_implemented},
          {"NUpTool", OutputType.zip},
          {"PageInformation", OutputType.zip},
          {"SelectPages", OutputType.zip},
          {"StampStationery", OutputType.zip},
          {"StampText", OutputType.zip},
          {"Stationery", OutputType.zip},
          {"Superimposing", OutputType.zip},
          {"TilingHero", OutputType.zip},
          {"TwoPasses", OutputType.zip}       
        }
      },      
      {"Chapter07", new Dictionary<string, OutputType>() 
        {
          {"AddVersionChecker", OutputType.zip},
          {"Advertisement", OutputType.zip},
          {"BookmarkedTimeTable", OutputType.zip},
          {"ButtonsActions", OutputType.zip},
          {"Calculator", OutputType.zip},
          {"ConcatenateBookmarks", OutputType.zip},
          {"ConcatenateNamedDestinations", OutputType.zip},
          {"CreateOutlineTree", OutputType.zip},
          {"EventsAndActions", OutputType.zip},
          {"FindDirectors", OutputType.zip},
          {"GenericAnnotations", OutputType.pdf},
          {"LaunchAction", OutputType.pdf},
          {"LinkActions", OutputType.zip},
          {"MovieAnnotations1", OutputType.pdf},
          {"MovieAnnotations2", OutputType.pdf},
          {"MovieAnnotations3", OutputType.pdf},
          {"MoviePosters1", OutputType.pdf},
          {"MoviePosters2", OutputType.zip},
          {"NamedActions", OutputType.zip},
          {"PrintTimeTable", OutputType.zip},
          {"TimetableAnnotations1", OutputType.zip},
          {"TimetableAnnotations2", OutputType.zip},
          {"TimetableAnnotations3", OutputType.zip},
          {"TimetableDestinations", OutputType.zip}
        }
      },         
      {"Chapter08", new Dictionary<string, OutputType>() 
        {
          {"Buttons", OutputType.zip},
          {"ChildFieldEvent", OutputType.none},
          {"ChoiceFields", OutputType.zip},
          {"MovieAds", OutputType.zip},
          {"RadioButtons", OutputType.pdf},
          {"ReaderEnabledForm", OutputType.zip},
          {"ReplaceIcon", OutputType.zip},
          {"Subscribe", OutputType.zip},
          {"TextFieldActions", OutputType.pdf},
          {"TextFieldFonts", OutputType.zip},
          {"TextFields", OutputType.zip},
          {"XfaMovie", OutputType.zip},
          {"XfaMovies", OutputType.zip}
        }
      },
/*
 * we implement a semi-hack for chapter 9 examples to allow people who are
 * __NOT__ web developers to build all the other chapter output files. 
 * this (hopefully??) is the only chapter we use OutputType.other 
 */
      {"Chapter09", new Dictionary<string, OutputType>() 
        {
          {"CreateFDF", OutputType.zip},
          {"FDFServlet", OutputType.other},
          {"FormServlet", OutputType.other},
          {"Hello", OutputType.pdf},
          {"HtmlMovies1", OutputType.zip},
          {"HtmlMovies2", OutputType.zip},
          {"JSForm", OutputType.zip},
          {"MovieServlet", OutputType.zip},
          {"PdfServlet", OutputType.other},
          {"ShowData", OutputType.other},
          {"SubmitForm", OutputType.other},
          {"XFDFServlet", OutputType.other},
          {"XmlHandler", OutputType.none}
        }
      },      
      {"Chapter10", new Dictionary<string, OutputType>() 
        {
          {"Barcodes", OutputType.pdf},
          {"ClippingPath", OutputType.zip},
          {"CompressAwt", OutputType.zip},
          {"CompressImage", OutputType.zip},
          {"DeviceColor", OutputType.pdf},
          {"ImageMask", OutputType.zip},
          {"ImageTypes", OutputType.zip},
          {"PagedImages", OutputType.zip},
          {"RawImage", OutputType.pdf},
          {"SeparationColor", OutputType.pdf},
          {"ShadingPatternColor", OutputType.pdf},
          {"TemplateClip", OutputType.zip},
          {"TilingPatternColor", OutputType.zip},
          {"Transparency1", OutputType.pdf},
          {"Transparency2", OutputType.pdf},
          {"TransparentAwt", OutputType.zip},
          {"TransparentImage", OutputType.zip},
          {"TransparentOverlay", OutputType.zip}
        }
      },      
      {"Chapter11", new Dictionary<string, OutputType>() 
        {
          {"CJKExample", OutputType.pdf},
          {"Diacritics1", OutputType.pdf},
          {"Diacritics2", OutputType.pdf},
          {"EncodingExample", OutputType.pdf},
          {"EncodingNames", OutputType.pdf},
          {"ExtraCharSpace", OutputType.pdf},
          {"FontFactoryExample", OutputType.pdf},
          {"FontFileAndSizes", OutputType.zip},
          {"FontSelectionExample", OutputType.pdf},
          {"FontTypes", OutputType.pdf},
          {"Ligatures1", OutputType.pdf},
          {"Ligatures2", OutputType.pdf},
          {"Monospace", OutputType.pdf},
          {"Peace", OutputType.zip},
          {"RightToLeftExample", OutputType.pdf},
          {"SayPeace", OutputType.zip},
          {"TTCExample", OutputType.pdf},
          {"Type3Example", OutputType.pdf},
          {"UnicodeExample", OutputType.pdf},
          {"VerticalTextExample1", OutputType.pdf},
          {"VerticalTextExample2", OutputType.pdf}
        }
      },
      {"Chapter12", new Dictionary<string, OutputType>() 
        {
          {"EncryptionPdf", OutputType.zip},
          {"EncryptWithCertificate", OutputType.not_implemented},
          {"HelloWorldCompression", OutputType.zip},
          {"MetadataPdf", OutputType.zip},
          {"MetadataXmp", OutputType.zip},
          {"SignatureExternalHash", OutputType.not_implemented},
          {"SignatureField", OutputType.not_implemented},
          {"Signatures", OutputType.not_implemented},
          {"SignWithBC", OutputType.not_implemented},
          {"TimestampOCSP", OutputType.not_implemented},
        }
      },
      {"Chapter13", new Dictionary<string, OutputType>() 
        {
          {"AddJavaScriptToForm", OutputType.zip},
          {"AppendMode", OutputType.zip},
          {"Bookmarks2NamedDestinations", OutputType.zip},
          {"CropPages", OutputType.zip},
          {"FixBrokenForm", OutputType.zip},
          {"InspectForm", OutputType.zip},
          {"PageLabelExample", OutputType.zip},
          {"PageLayoutExample", OutputType.zip},
          {"PdfXPdfA", OutputType.zip},
          {"PrintPreferencesExample", OutputType.pdf},
          {"RemoveLaunchActions", OutputType.zip},
          {"ReplaceURL", OutputType.zip},
          {"RotatePages", OutputType.zip},
          {"ViewerPreferencesExample", OutputType.zip}
        }
      },
      {"Chapter14", new Dictionary<string, OutputType>() 
        {
          {"DirectorCharts", OutputType.not_implemented},
          {"GetContentStream", OutputType.zip},
          {"Graphics2DFonts", OutputType.not_implemented},
          {"GraphicsStateOperators", OutputType.pdf},
          {"PathConstructionAndPainting", OutputType.pdf},
          {"PearExample", OutputType.not_implemented},
          {"PearToPdf", OutputType.not_implemented},
          {"Text1ToPdf1", OutputType.not_implemented},
          {"Text1ToPdf2", OutputType.not_implemented},
          {"Text2ToPdf1", OutputType.not_implemented},
          {"Text2ToPdf2", OutputType.not_implemented},
          {"Text2ToPdf3", OutputType.not_implemented},
          {"Text3ToPdf", OutputType.not_implemented},
          {"Text4ToPdf", OutputType.not_implemented},
          {"TextExample1", OutputType.not_implemented},
          {"TextExample2", OutputType.not_implemented},
          {"TextExample3", OutputType.not_implemented},
          {"TextExample4", OutputType.not_implemented},
          {"TextMethods", OutputType.pdf},
          {"TextStateOperators", OutputType.pdf},
          {"TransformationMatrix1", OutputType.zip},
          {"TransformationMatrix2", OutputType.zip},
          {"TransformationMatrix3", OutputType.zip}
        }
      },         
      {"Chapter15", new Dictionary<string, OutputType>() 
        {
          {"ContentParser", OutputType.not_implemented},
          {"ExtractImages", OutputType.zip},
          {"ExtractPageContent", OutputType.zip},
          {"ExtractPageContentArea", OutputType.zip},
          {"ExtractPageContentSorted1", OutputType.zip},
          {"ExtractPageContentSorted2", OutputType.zip},
          {"InspectPageContent", OutputType.zip},
          {"LayerMembershipExample1", OutputType.pdf},
          {"LayerMembershipExample2", OutputType.pdf},
          {"MyImageRenderListener", OutputType.none},
          {"MyTextRenderListener", OutputType.none},
          {"ObjectData", OutputType.pdf},
          {"OptionalContentActionExample", OutputType.pdf},
          {"OptionalContentExample", OutputType.pdf},
          {"ParseTaggedPdf", OutputType.zip},
          {"ParsingHelloWorld", OutputType.zip},
          {"PeekABoo", OutputType.zip},
          {"ReadOutLoud", OutputType.pdf},
          {"ShowTextMargins", OutputType.zip},
          {"StructuredContent", OutputType.zip},
          {"StructureParser", OutputType.not_implemented},
          {"SvgLayers", OutputType.not_implemented},
          {"SvgToPdf", OutputType.not_implemented}
        }
      },
      {"Chapter16", new Dictionary<string, OutputType>() 
        {
          {"EmbedFontPostFacto", OutputType.zip},
          {"FestivalCalendar1", OutputType.pdf},
          {"FestivalCalendar2", OutputType.pdf},
          {"FileAttachmentEvent", OutputType.none},
          {"KubrickBox", OutputType.pdf},
          {"KubrickCollection", OutputType.pdf},
          {"KubrickDocumentary", OutputType.zip},
          {"KubrickDvds", OutputType.zip},
          {"KubrickMovies", OutputType.pdf},
          {"ListUsedFonts", OutputType.zip},
          {"LocalDestinationEvent", OutputType.none},
          {"MovieAnnotation", OutputType.pdf},
          {"Pdf3D", OutputType.pdf},
          {"PdfActionEvent", OutputType.none},
          {"ResizeImage", OutputType.zip},
          {"SpecialId", OutputType.zip},
        }
      }    
    };
// ===========================================================================
  }
}