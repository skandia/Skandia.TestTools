using Skandia.TestTools.Xunit.DataSource.TestCase;
using System.Collections.Generic;
using System.Xml.Linq;
using Xunit;
using Xunit.Sdk;

namespace XUnitTestProject1
{
    public class UnitTests
    {
        readonly XElement _xmlData = XElement.Parse(@"<NewDataSet>
    <xs:schema id = ""NewDataSet"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
    <xs:element name = ""NewDataSet"" msdata:IsDataSet=""true"" msdata:Locale="""">
    <xs:complexType>
    <xs:choice minOccurs = ""0"" maxOccurs=""unbounded"">
    <xs:element name = ""Table1"" >
    <xs:complexType>
    <xs:sequence>
    <xs:element name = ""number"" type=""xs:string"" minOccurs=""0"" />
    <xs:element name = ""name"" type=""xs:string"" minOccurs=""0"" />
    </xs:sequence>
    </xs:complexType>
    </xs:element>
    </xs:choice>
    </xs:complexType>
    </xs:element>
    </xs:schema>
    <Table1>
    <number>191212121212</number>
    <name>First Name</name>
    </Table1>
    <Table1>
    <number>Another Number</number>
    <name>New Name</name>
    </Table1>
    </NewDataSet>");

        [Fact]
        public void Tfs_Xml_To_Dictionary()
        {
            // Arrange
            var expected = new[]
            {
                new[] {"191212121212", "First Name"},
                new[] { "Another Number", "New Name" }
            };

            // Act
            var result = TfsTestCaseDataAttribute.TfsXmlDataToDictionaryList(_xmlData);

            // Assert
            Assert.Equal(2, result.Count);
            for (var i = 0; i < result.Count; ++i)
            {
                var set = result[i];

                Assert.Equal(2, set.Count);
                Assert.Contains("number", set.Keys);
                Assert.Contains("name", set.Keys);

                Assert.Equal(expected[i][0], set["number"]);
                Assert.Equal(expected[i][1], set["name"]);
            }
        }

        public void Dummy(string a, string b) { }

        [Fact]
        public void Method_Info_To_Parameter_Name_Array()
        {
            // Arrange
            var info = typeof(UnitTests).GetMethod("Dummy");

            // Act
            var result = TfsTestCaseDataAttribute.MethodParameterNamesToArray(info);

            // Assert
            Assert.Equal(2, result.Length);
            Assert.Equal("a", result[0]);
            Assert.Equal("b", result[1]);
        }

        [Fact]
        public void Parameter_Not_In_Data_Should_Throw()
        {
            // Arrange
            var testCaseData = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    {"number", "123"},
                    {"name", "John"}
                },
                new Dictionary<string, string>
                {
                    {"number", "456"},
                    {"name", "Anna"}
                },
            };
            var parameters = new[] { "number", "name", "iat" };
            var testCaseid = 1234567;
            var attribute = new TfsTestCaseDataAttribute(testCaseid, "http://foo.com");

            // Act
            // Assert
            var exception = Assert.Throws<TestClassException>(() => attribute.DataToObjectArrayList(testCaseData, parameters));

            Assert.Contains(testCaseid.ToString(), exception.Message);
        }

        [Fact]
        public void Parameter_Missing_Should_Not_Throw()
        {
            // Arrange
            var testCaseData = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    {"number", "123"},
                    {"name", "John"},
                    {"iat", "1" }
                },
                new Dictionary<string, string>
                {
                    {"number", "456"},
                    {"name", "Anna"},
                    {"iat", "2" }
                },
            };
            var parameters = new[] {"name", "number"  };
            var attribute = new TfsTestCaseDataAttribute(1, "http://foo.com");

            // Act
            var result = attribute.DataToObjectArrayList(testCaseData, parameters);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("John", result[0][0]);
            Assert.Equal("123", result[0][1]);
            Assert.Equal("Anna", result[1][0]);
            Assert.Equal("456", result[1][1]);

        }
    }
}
