using System;

namespace AnimeFeedManager.SourceGenerators.TableNames
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class WithTableNameAttribute(string tableName) : Attribute
    {
        public string TableName { get; } = tableName;
    }

}