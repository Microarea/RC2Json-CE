using System;
using System.Collections;
using Newtonsoft.Json;

namespace RC2Json
{
	internal class MyJArray : ArrayList
	{
		internal static MyJArray Parse(JsonReader reader)
		{
			MyJArray obj = new MyJArray();
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
					case JsonToken.StartObject:
						obj.Add(MyJObject.Parse(reader));
						break;
					case JsonToken.EndArray:
						return obj;
				}
			}
			return obj;
		}

		internal void ToString(JsonTextWriter jtw)
		{
			jtw.WriteStartArray();
			foreach (MyJObject obj in this)
				obj.ToString(jtw);
			jtw.WriteEndArray();
		}
	}
}