using System;
using Newtonsoft.Json;

namespace RC2Json
{
	internal class MyJProperty
	{
		public string Comment { get; internal set; }
		public string Name { get; internal set; }
		public object Value { get; internal set; }
		public override string ToString()
		{
			return string.Concat(Name, ": ", Value);
		}
		internal void ToString(JsonTextWriter jtw)
		{
			jtw.WritePropertyName(Name);
			if (Value is MyJArray)
			{
				((MyJArray)Value).ToString(jtw);
			}
			else if (Value is MyJObject)
			{
				((MyJObject)Value).ToString(jtw);
			}
			else
			{
				jtw.WriteValue(Value);
			}

			if (Comment != null)
				jtw.WriteComment(Comment);
		}
	}
}