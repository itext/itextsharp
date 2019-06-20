/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using iTextSharp.text.log;
using iTextSharp.tool.xml.exceptions;

namespace iTextSharp.tool.xml.net {

    /**
     * @author redlab_b
     *
     */
    public class FileRetrieveImpl : IFileRetrieve {

        private static ILogger LOGGER = LoggerFactory.GetLogger(typeof(FileRetrieveImpl));
        private IList<string> rootdirs;
        private IList<string> urls;

        /**
         *
         */
        public FileRetrieveImpl() {
            rootdirs = new List<string>();
            urls = new List<String>();
        }

        /**
         * Constructs a new FileRetrieveImpl with the given root url's and
         * directories
         *
         * @param strings an array of strings, if the String starts with http or
         *            https it's taken as URL otherwise we check if it's a directory
         *            with
         *
         *            <pre>
         * File f = new File(str);
         * f.IsDirectory()
         * </pre>
         */
        public FileRetrieveImpl(String[] strings) : this() {
            foreach (String s in strings) {
                if (s.StartsWith("http") || s.StartsWith("https")) {
                    urls.Add(s);
                } else {
                    if (Directory.Exists(s)) {
                        rootdirs.Add(s);
                    }
                }
            }
        }

        public FileRetrieveImpl(String rootdir) : this() {
            if (Directory.Exists(rootdir)) {
                rootdirs.Add(rootdir);
            }
        }

        /**
         * ProcessFromHref first tries to create an {@link URL} from the given <code>href</code>,
         * if that throws a {@link MalformedURLException}, it will prepend the given
         * root URLs to <code>href</code> until a valid URL is found.<br />If by then there is
         * no valid url found, this method will see if the given <code>href</code> is a valid file
         * and can read it.<br />If it's not a valid file or a file that can't be read,
         * the given root directories will be set as root path with the given <code>href</code> as
         * file path until a valid file has been found.
         */
        virtual public void ProcessFromHref(String href, IReadingProcessor processor) {
            if (LOGGER.IsLogging(Level.DEBUG)) {
                LOGGER.Debug(String.Format(LocaleMessages.GetInstance().GetMessage("retrieve.file.from"), href));
            }
            Uri url = null;
            bool isfile = false;
            string f = href;
            try {
                url = new Uri(href);
            } catch (UriFormatException) {
                try {
                    url = DetectWithRootUrls(href);
                } catch (UriFormatException) {
                    // its probably a file, try to detect it.
                    isfile = true;
                    if (!(File.Exists(href))) {
                        isfile = false;
                        foreach (string root in rootdirs) {
                            f = Path.Combine(root, href);
                            if (File.Exists(f)) {
                                isfile = true;
                                break;
                            }

                        }
                    }
                }
            }
            Stream inp = null;
            if (null != url) {
                WebRequest w = WebRequest.Create(url);
                try {
                    inp = w.GetResponse().GetResponseStream();
                } catch (WebException) {
                    throw new IOException(LocaleMessages.GetInstance().GetMessage("retrieve.file.from.nothing"));
                }
            } else if (isfile) {
                inp = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.Read);
            } else {
                throw new IOException(LocaleMessages.GetInstance().GetMessage("retrieve.file.from.nothing"));
            }
            Read(processor, inp);
        }

        /**
         * @param href the reference
         * @throws MalformedURLException if no valid URL could be found.
         */
        private Uri DetectWithRootUrls(String href) {
            foreach (String root in urls) {
                try {
                    return new Uri(root + href);
                } catch (UriFormatException) {
                }
            }
            throw new UriFormatException();
        }

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.net.FileRetrieve#processFromStream(java.io.Stream, com.itextpdf.tool.xml.net.ReadingProcessor)
         */
        virtual public void ProcessFromStream(Stream inp, IReadingProcessor processor) {
            Read(processor, inp);
        }

        /**
         * @param processor
         * @param in
         * @throws IOException
         */
        private void Read(IReadingProcessor processor, Stream inp) {
            try {
                int inbit = -1;
                while ((inbit = inp.ReadByte()) != -1) {
                    processor.Process(inbit);
                }
            } catch (IOException e) {
                throw e;
            } finally {
                try {
                    if (null != inp) {
                        inp.Close();
                    }
                } catch (IOException e) {
                    throw new RuntimeWorkerException(e);
                }
            }
        }

        /**
         * Add a root directory.
         * @param dir the root directory
         */
        virtual public void AddRootDir(string dir) {
            rootdirs.Add(dir);
        }

        /**
         * Add a root URL.
         * @param url the URL
         */
        virtual public void AddURL(String url) {
            urls.Add(url);
        }

    }
}
