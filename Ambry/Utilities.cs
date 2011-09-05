using System;
using System.Linq.Expressions;

namespace Ambry
{
	public class Utilities
	{
		public static string PropertyName<T>(Expression<Func<T, Object>> property)
		{
			var lambda = (LambdaExpression)property;

			MemberExpression memberExpression;

			if (lambda.Body is UnaryExpression)
			{
				var unaryExpression = (UnaryExpression)lambda.Body;
				memberExpression = (MemberExpression)unaryExpression.Operand;
			}
			else
			{
				memberExpression = (MemberExpression)lambda.Body;
			}

			return memberExpression.Member.Name;
		}
	}
}