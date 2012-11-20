/*
 * This class is __NOT__ part of the book "iText in Action - 2nd Edition".
 * it's a helper class to build the examples using VS2008 or higher
 * on your local machine, either in a web context or command line
 * common interface for all classes build the example result file(s)
*/
using System.IO;
using System.Web;

namespace kuujinbo.iTextInAction2Ed {
  public interface IWriter {
// ===========================================================================
    void Write(Stream stream);
// ===========================================================================    
  }
}