using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DarkDocumentStore.Extensions;

namespace DarkDocumentStore
{
	public class Record
	{
		private readonly IRecord _content;

		public Record(IRecord content)
		{
			_content = content.IfNotNull("content");

			//eventually a nameStore or similar for this, not sure on inheritence implications yet.
			Name = content.GetType().Name;
		}

		public DateTime? Updated { get; private set; }
		internal String Name { get; private set; }

	}

}
