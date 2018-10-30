using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Xunit.Sdk;

[assembly:InternalsVisibleTo("XUnitTest-Core21")]
namespace Skandia.TestTools.Xunit.DataSource.TestCase
{
    /// <summary>
    /// To set credentials, inherit this and use the protected ctor./>.
    /// </summary>
    public class TfsTestCaseDataAttribute : DataAttribute
    {
        protected readonly Uri CollectionUri;
        protected readonly int TestCaseId;
        protected readonly VssCredentials Credentials;

        private const string TestCaseWorkItemTypeKey = "System.WorkItemType";
        private const string TestCaseWorkItemTypeValue = "Test Case";
        private const string TestCaseDataSourceKey = "Microsoft.VSTS.TCM.LocalDataSource";

        public TfsTestCaseDataAttribute(int testCaseId, string collectionUri)
            : this(testCaseId, new Uri(collectionUri), new VssCredentials())
        { }

        protected TfsTestCaseDataAttribute(int testCaseId, Uri collectionUri, VssCredentials credentials)
        {
            CollectionUri = collectionUri;
            TestCaseId = testCaseId;
            Credentials = credentials;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            // Connect to TFS
            var xmlData = GetXmlDataFromTfsAsync().Result;

            // Create a dictionary with data from the Test case
            var testDataDictionaryList = TfsXmlDataToDictionaryList(xmlData);

            // Create array of parameters to the current Theory
            var parametersNames = MethodParameterNamesToArray(testMethod);

            // Assemble a list of object arrays
            var result = DataToObjectArrayList(testDataDictionaryList, parametersNames);

            return result;
        }

        internal async Task<XElement> GetXmlDataFromTfsAsync()
        {
            using (var visualStudioServicesConnection = new VssConnection(CollectionUri, Credentials))
            using (var workItemTrackingHttpClient = visualStudioServicesConnection.GetClient<WorkItemTrackingHttpClient>())
            {
                var workItemInstance = await workItemTrackingHttpClient.GetWorkItemAsync(TestCaseId);

                var workItemType = (string) workItemInstance.Fields[TestCaseWorkItemTypeKey];
                if (!workItemType.Equals(TestCaseWorkItemTypeValue))
                {
                    throw new TestClassException($"Work Item '{TestCaseId}' is not a Test Case, but a '{workItemType}'");
                }

                var data = (string) workItemInstance.Fields[TestCaseDataSourceKey];
                return XElement.Parse(data);
            }
        }

        internal static List<Dictionary<string, string>> TfsXmlDataToDictionaryList(XElement xmlData)
        {
            var testDataNodes = xmlData.Nodes().First().ElementsAfterSelf().ToArray();
            var testDataDictionaryList = testDataNodes.Select(row => row.Elements().ToDictionary(e => e.Name.ToString().ToLowerInvariant(), e => e.Value)).ToList();
            return testDataDictionaryList;
        }

        internal static string[] MethodParameterNamesToArray(MethodInfo testMethod)
        {
            var parametersNames = testMethod.GetParameters().Select(p => p.Name.ToLowerInvariant()).ToArray();
            return parametersNames;
        }

        internal List<object[]> DataToObjectArrayList(List<Dictionary<string, string>> testDataDictionaryList, string[] parametersNames)
        {
            // Traverse all the rows in the Test Case data 
            var result = new List<object[]>();
            foreach (var row in testDataDictionaryList)
            {
                // For every Theory parameter name, add the corresponding Test Case data value. 
                // The order of the parameters has to match the order in the object array.
                var resultRow = new object[parametersNames.Length];
                for (var i = 0; i < parametersNames.Length; i++)
                {
                    try
                    {
                        resultRow[i] = row[parametersNames[i]];
                    }
                    catch (KeyNotFoundException)
                    {
                        throw new TestClassException($"Parameter '{parametersNames[i]}' does not exist in TFS Test Case {TestCaseId}.");
                    }
                }
                result.Add(resultRow);
            }

            return result;
        }

    }
}
