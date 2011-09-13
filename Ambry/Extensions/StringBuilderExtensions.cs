using System;
using System.Text;

namespace Ambry.Extensions
{
	internal static class StringBuilderExtensions
	{

		public static void AppendLine(this StringBuilder self, String format, params Object[] args)
		{
			Check.Argument(self, "self");

			self.AppendFormat(format, args);
			self.AppendLine();
		}
	}
}
