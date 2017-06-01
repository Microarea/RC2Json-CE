using System;
using System.Collections;
using System.Collections.Generic;

namespace RC2Json
{
	internal class MyJObjectComparer : IComparer<MyJObject>, IComparer
	{
		public int Compare(object x, object y)
		{
			return Compare((MyJObject)x, (MyJObject)y);
		}

		public int Compare(MyJObject x, MyJObject y)
		{
			int x1 = Convert.ToInt32(x["x"]);
			int y1 = Convert.ToInt32(x["y"]);
			int x2 = Convert.ToInt32(y["x"]);
			int y2 = Convert.ToInt32(y["y"]);
			//riporto i controlli nell'area statica di destra come se fossero sotto all'area statica di sinistra
			if (x1 >= 327)
			{
				y1 += 10000;
			}
			if (x2 >= 327)
			{
				y2 += 10000;
			}
			int cmp = y1.CompareTo(y2);
			if (cmp != 0)
				return cmp;
			return x1.CompareTo(x2);
		}
	}
}