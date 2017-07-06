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