/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2026 iText Group NV
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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace iTextSharp.testutils
{
    public class CompareToolUtil
    {
        private static int tempFileCounter = 0;
        private const int stdOutputIndex = 0;
        private const int ErrOutputIndex = 1;
        private const String splitRegex = "((\".+?\"|[^'\\s]|'.+?')+)\\s*";
        
        public static String CreateTempCopy(String file, String tempFilePrefix, String tempFilePostfix) {
            string copiedFile = null;
            try {
                copiedFile = Path.Combine(Path.GetTempPath(), 
                    tempFilePrefix + Guid.NewGuid() + Interlocked.Increment(ref tempFileCounter) + tempFilePostfix);
                Copy(file, copiedFile);
            } catch (IOException e) {
                RemoveFiles(new Object[] {copiedFile});
                throw e;
            }
            return copiedFile;
        }
        
        public static void Copy(String fileToCopy, String copiedFile) {
            File.Copy(fileToCopy, copiedFile, true);
        }
        
        public static String CreateTempDirectory(String tempFilePrefix) {
            string temporaryDirectory = null;
            try {
                temporaryDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + tempFilePrefix + 
                                                                      Interlocked.Increment(ref tempFileCounter));
                Directory.CreateDirectory(temporaryDirectory);
            } catch (IOException e) {
                RemoveFiles(new Object[] {temporaryDirectory});
                throw e;    
            }
            return temporaryDirectory;
        }
        
        public static bool RemoveFiles(Object[] paths) {
            bool allFilesAreRemoved = true;
            foreach (String path in paths) {
                try {
                    if (null != path) {
                        File.Delete(path);
                    }
                } catch (Exception e) {
                    if (Directory.Exists(path)) {
                        try {   
                            Directory.Delete(path);
                        }
                        catch (Exception exc) {
                            allFilesAreRemoved = false;
                        }
                    }
                    else {
                        allFilesAreRemoved = false;
                    }
                }
            }
            return allFilesAreRemoved;
        }
          internal static void SetProcessStartInfo(Process proc, String exec, String @params, String workingDir) {
            String[] processArguments = PrepareProcessArguments(exec, @params);
            proc.StartInfo = new ProcessStartInfo(processArguments[0], processArguments[1]);
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.WorkingDirectory = workingDir;
        }
        
        internal static String[] PrepareProcessArguments(String exec, String @params) {
            bool isExcitingFile;
            try
            {   
                if (exec.EndsWith("compare"))
                {
                    @params = "compare " + @params;
                    exec = exec.Replace("compare", "");
                }

                isExcitingFile = new FileInfo(exec).Exists;
            }
            catch (Exception)
            {
                isExcitingFile = false;
            }

            return isExcitingFile
                ? new String[] {exec, @params.Replace("'", "\"")}
                : SplitIntoProcessArguments(exec, @params);
        }
        
        internal static String[] SplitIntoProcessArguments(String command, String @params) {
            Regex regex = new Regex(splitRegex);
            MatchCollection matches = regex.Matches(command);
            String processCommand = "";
            String processArguments = "";
            if (matches.Count > 0)
            {
                processCommand = matches[0].Value.Trim();
                for (int i = 1; i < matches.Count; i++)
                {
                    Match match = matches[i];
                    processArguments += match.Value;
                }

                processArguments = processArguments + " " + @params;
                processArguments = processArguments.Replace("'", "\"").Trim();
            }

            return new String[] {processCommand, processArguments};
        }
        
        internal static String GetProcessOutput(Process p) {
            StringBuilder[] builders = GetProcessOutputBuilders(p);

            return builders[stdOutputIndex].ToString() 
                   + '\n' 
                   + builders[ErrOutputIndex].ToString();
        }
        
        internal static StringBuilder[] GetProcessOutputBuilders(Process p) {
            StringBuilder bri = new StringBuilder();
            StringBuilder bre = new StringBuilder();
            do {
                bri.Append(p.StandardOutput.ReadToEnd());
                bre.Append(p.StandardError.ReadToEnd());
            } while (!p.HasExited);
                
            Console.Out.WriteLine(bre.ToString());

            StringBuilder[] resultOutputArray = new StringBuilder[] { bri, bre };
            return resultOutputArray;
        }
    }
}