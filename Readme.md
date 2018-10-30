# Skandia.TestTools

## Xunit.DataSource.TestCase
Fetches data from a TFS/Azure Devops Test Case and matches parameters to XUnit Theory method parameters.

Usage:
``` CSharp
using Skandia.TestTools.Xunit.DataSource.TestCase;
using Xunit;

...

[Theory]
[TfsTestCaseData(TestCaseId, TfsUri)]
public void Test1(string parameter, string anotherParameter)
{
    // Test code
}
```

This will fetch all Test Case parameters and feed each row to the Theory. The method parameter names must match the parameter names in the Test Case. The ordering of parameters is not important. The Theory does not need to declare every parameter found the Test case, but will throw an exception if the parameter name is not found in the Test Case.

Note that all parameters are instantiated as `string`. You will need to do your own parsing and casting to other data types.

### Static URI or custom authentication
Should you wish to save some space in your attributes, or use custom authentication, you can inherit `TfsTestCaseData` and call the protected constructor, for example:

```CSharp
public class TfsTestCaseDataWithCustomParamsAttribute : TfsTestCaseDataAttribute
{
    public TfsTestCaseDataWithCustomParamsAttribute(int testCaseId) 
        : base(testCaseId, "https://tfs.local/tfs/DefaultCollection", new VssCredentials())
    {}
}
```
Change the credentials value to whatever type of `VssCredentials` you require.