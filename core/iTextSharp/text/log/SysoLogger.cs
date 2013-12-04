using System;
using System.Text;
/*
 * $Id: Logger.java 4847 2011-05-05 19:46:13Z redlab_b $
 *
 * This file is part of the iText (R) project. Copyright (c) 1998-2011 1T3XT
 * BVBA Authors: Balder Van Camp, Emiel Ackermann, et al.
 *
 * This program is free software; you can redistribute it and/or modify it under
 * the terms of the GNU Affero General License version 3 as published by the
 * Free Software Foundation with the addition of the following permission added
 * to Section 15 as permitted in Section 7(a): FOR ANY PART OF THE COVERED WORK
 * IN WHICH THE COPYRIGHT IS OWNED BY 1T3XT, 1T3XT DISCLAIMS THE WARRANTY OF NON
 * INFRINGEMENT OF THIRD PARTY RIGHTS.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 * FOR A PARTICULAR PURPOSE. See the GNU Affero General License for more
 * details. You should have received a copy of the GNU Affero General License
 * along with this program; if not, see http://www.gnu.org/licenses or write to
 * the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
 * MA, 02110-1301 USA, or download the license from the following URL:
 * http://itextpdf.com/terms-of-use/
 *
 * The interactive user interfaces in modified source and object code versions
 * of this program must display Appropriate Legal Notices, as required under
 * Section 5 of the GNU Affero General License.
 *
 * In accordance with Section 7(b) of the GNU Affero General License, a covered
 * work must retain the producer line in every PDF that is created or
 * manipulated using iText.
 *
 * You can be released from the requirements of the license by purchasing a
 * commercial license. Buying such a license is mandatory as soon as you develop
 * commercial activities involving the iText software without disclosing the
 * source code of your own applications. These activities include: offering paid
 * services to customers as an ASP, serving PDFs on the fly in a web
 * application, shipping iText with a closed source product.
 *
 * For more information, please contact iText Software Corp. at this address:
 * sales@itextpdf.com
 */
namespace iTextSharp.text.log {

    /**
     * A Simple System.out logger.
     * @author redlab_be
     *
     */
    public class SysoLogger : ILogger {

        private String name;
        private int shorten;

        /**
         * Defaults packageReduce to 1.
         */
        public SysoLogger() : this(1) {
        }
        /**
         * Amount of characters each package name should be reduced with.
         * @param packageReduce
         *
         */
        public SysoLogger(int packageReduce) {
            this.shorten = packageReduce;
        }

        /**
         * @param klass
         * @param shorten
         */
        protected SysoLogger(String klass, int shorten) {
            this.shorten = shorten;
            this.name = klass;
        }

        public ILogger GetLogger(Type klass) {
            return new SysoLogger(klass.FullName, shorten);
        }

        /* (non-Javadoc)
         * @see com.itextpdf.text.log.Logger#getLogger(java.lang.String)
         */
        public ILogger GetLogger(String name) {
            return new SysoLogger("[itext]", 0);
        }

        public bool IsLogging(Level level) {
            return true;
        }

        public void Warn(String message) {
            Console.Out.WriteLine("{0} WARN  {1}", Shorten(name), message);
        }

        /**
         * @param name2
         * @return
         */
        private String Shorten(String className) {
            if (shorten != 0) {
                StringBuilder target = new StringBuilder();
                String name = className;
                int fromIndex = className.IndexOf('.');
                while (fromIndex != -1) {
                    int parseTo = (fromIndex < shorten) ? (fromIndex) : (shorten);
                    target.Append(name.Substring(0, parseTo));
                    target.Append('.');
                    name = name.Substring(fromIndex + 1);
                    fromIndex = name.IndexOf('.');
                }
                target.Append(className.Substring(className.LastIndexOf('.') + 1));
                return target.ToString();
            }
            return className;
        }

        public void Trace(String message) {
            Console.Out.WriteLine("{0} TRACE {1}", Shorten(name), message);
        }

        public void Debug(String message) {
            Console.Out.WriteLine("{0} DEBUG {1}", Shorten(name), message);
        }

        public void Info(String message) {
            Console.Out.WriteLine("{0} INFO  {1}", Shorten(name), message);
        }

        public void Error(String message) {
            Console.Out.WriteLine("{0} ERROR {1}", name, message);
        }

        public void Error(String message, Exception e) {
            Console.Out.WriteLine("{0} ERROR {1}", name, message);
            Console.Out.WriteLine(e.StackTrace);
        }
    }
}