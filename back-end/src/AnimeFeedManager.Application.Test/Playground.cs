using System.Linq;
using Xunit;

namespace AnimeFeedManager.Application.Test;

[Trait("Category", "Playground")]
public class Playground
{
    // [Fact]
    // public void Test1()
    // {
    //     var testData = new [] {"1", "2", "3"};
    //     var test = testData.Select(id => $"({TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id)})");
    //     var result = string.Join($" {TableOperators.Or} ", test );
    //     Assert.Equal("(RowKey eq '1') or (RowKey eq '2') or (RowKey eq '3')", result);
    //
    // }
    //
    // [Fact]
    // public void Test2()
    // {
    //     var testData = new[] { "1"};
    //     var test = testData.Select(id => $"({TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id)})");
    //     var result = string.Join(TableOperators.Or, test);
    //     Assert.Equal("(RowKey eq '1')", result);
    // }
    //
    // [Fact]
    // public void Test3()
    // {
    //     var testData = new string[] { };
    //     var test = testData.Select(id => $"({TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id)})");
    //     var result = string.Join(TableOperators.Or, test);
    //     Assert.Equal(result, string.Empty);
    // }
    //
    // [Fact]
    // public void Test4()
    // {
    //     var testData = new[] { "1", "2" };
    //     var test = testData.Select(id => $"({TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id)})");
    //     var result = string.Join($" {TableOperators.Or} ", test);
    //
    //     var combinedResult = TableQuery.CombineFilters(
    //         TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, "1"),
    //         TableOperators.Or,
    //         TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, "2")
    //     );
    //
    //
    //     Assert.Equal(combinedResult, result);
    // }
}