namespace AnimeFeedManager.Application.Test;

[Trait("Category", "Playground")]
public class Playground
{

    // [Fact]
    // public void Parse_Live_Chart_Dates()
    // {
    //     var sample = "Jul 21, 2022 at 1:00pm UTC".Replace(" at", string.Empty).Replace("UTC", "GMT");
    //     var result = DateTime.TryParse(sample, out var date);
    //     Assert.True(result);
    //     Assert.Equal(new DateTime(2022,7,21,9,0,0).ToUniversalTime(), date.ToUniversalTime());
    // }
    
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