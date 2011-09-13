using System;
using System.Linq.Expressions;

namespace Ambry
{
	public class Utilities
	{

		public static string GetPropertyName<T>(Expression<Func<T, Object>> property)
		{
			var memberExpression = GetMemberExpression(property);

			return memberExpression.Member.Name;
		}

		public static Type GetPropertyType<T>(Expression<Func<T, Object>> property)
		{

			var memberExpression = GetMemberExpression(property);

			return memberExpression.Type;
		}

		private static MemberExpression GetMemberExpression<T>(Expression<Func<T, object>> property)
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
			return memberExpression;
		}
	}
}