using System.Collections;
using System;
namespace Yu.Tools
{
	[AttributeUsage(AttributeTargets.Field)]
	public class ColumnMatchingAttribute : Attribute {

		public string[] ColumnNames;

		public ColumnMatchingAttribute(params string[] matchingColumnNames)
		{
			ColumnNames = matchingColumnNames;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class ColumnKeyMatchingAttribute : Attribute {

	}

	[AttributeUsage(AttributeTargets.Field)]
	public class ColumnMatchingArrayAttribute : Attribute {

		public string[] ColumnNames;

		public ColumnMatchingArrayAttribute(params string[] matchingColumnNames)
		{
			ColumnNames = matchingColumnNames;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class ColumnMatchingClassAttribute : Attribute {
		public string[] ColumnNames;

		public ColumnMatchingClassAttribute(params string[] matchingColumnNames)
		{
			ColumnNames = matchingColumnNames;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class ColumnMatchingClassArrayAttribute : Attribute {
		public string[] ColumnNames;

		public ColumnMatchingClassArrayAttribute(params string[] matchingColumnNames)
		{
			ColumnNames = matchingColumnNames;
		}
	}
}