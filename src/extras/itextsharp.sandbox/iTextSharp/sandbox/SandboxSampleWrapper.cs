using System;
using System.Collections.Generic;
using System.Reflection;
using iTextSharp.text.log;
using NUnit.Framework;

namespace iTextSharp.sandbox
{
    /// <summary>
    /// Wraps samples from dlls into tests. Dll file name, name of class and name of namespace must be the same.
    /// </summary>
    public class SandboxSampleWrapper : GenericTest
    {
        /// <summary>
        /// The logger class
        /// </summary>
        private static readonly ILogger LOGGER = LoggerFactory.GetLogger(typeof (GenericTest));

        public override IEnumerable<TestCaseData> Data()
        {
            String dll = Environment.CurrentDirectory + "/itextsharp.sandbox.dll";
            List<TestCaseData> testCases = GetTestTypes(dll);
            return testCases;
        }

        private static List<TestCaseData> GetTestTypes(String dllPath)
        {
            List<TestCaseData> testCases = new List<TestCaseData>();
            Assembly testAssembly = Assembly.LoadFile(dllPath);
            Type wrapToTest = testAssembly.GetType("iTextSharp.sandbox.WrapToTestAttribute");
            foreach (Type type in testAssembly.GetTypes())
            {
                Attribute wrapToTestInstance = Attribute.GetCustomAttribute(type, wrapToTest);
                if (wrapToTestInstance != null)
                {
                    PropertyInfo compareRendersProp = wrapToTest.GetProperty("CompareRenders");
                    bool compareRenders = (bool)compareRendersProp.GetValue(wrapToTestInstance, null);
                    testCases.Add(new TestCaseData(type, compareRenders));
                }
            }

            return testCases;
        }
    }
}
