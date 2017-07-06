/*
RC2Json CE - Tool di trasformazione .RC in TB Json 
Copyright (C) 2017 Microarea s.p.a.

This program is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software Foundation, 
either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, 
but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE. 

See the GNU General Public License for more details.
*/

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