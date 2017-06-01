using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RC2Json
{

	//************ATTENZIONE************
	//TENERE ALLINEATO QUESTO ENUMERATIVO CON QUELLO IN TaskBuilder\Framework\TbGeneric\WndObjDescription.h
	
    public enum WndObjType
	{
		Undefined = 0, View = 1, Label = 2, Button = 3, PdfButton = 4, BeButton = 5, BeButtonRight = 6,
		SaveFileButton = 7, Image = 8, Group = 9, Radio = 10, Check = 11, Combo = 12, Edit = 13, Toolbar = 14,
		ToolbarButton = 15, Tabber = 16, Tab = 17, BodyEdit = 18, Radar = 19, HotLink = 20, Table = 21,
		ColTitle = 22, Cell = 23, List = 24, CheckList = 25, Tree = 26, TreeNode = 27, Menu = 28, MenuItem = 29,
		ListCtrl = 30, ListCtrlItem = 31, ListCtrlDetails = 32, Spin = 33, /*Report = 34,*/ StatusBar = 35, SbPane = 36,
		/*Title = 37 non più usata, il titolo è nella proprietà text del'oggetto, il rettangolo del titolo non ci interessa più*/
		MainMenu = 38, AuxRadarToolbar = 39, Frame = 40, RadarFrame = 41, PrintDialog = 42, Dialog = 43,
		PropertyDialog = 44, GenericWndObj = 45, RadarHeader = 46, FileDialog = 47, BETreeCell = 48, BtnImageAndText = 49,
		MultiChart = 50, TreeAdv = 51, TreeNodeAdv = 52, MailAddressEdit = 53, WebLinkEdit = 54, AddressEdit = 55,
		/*FieldReport = 56, TableReport = 57,*/
		EasyBuilderToolbar = 58, FloatingToolbar = 59, MSCombo = 60, UploadFileButton = 61, Thread = 62,
		ProgressBar = 63, CaptionBar = 64, RadioGroup = 65, Panel = 66, TabbedToolbar = 67, NamespaceEdit = 68,
		Constants = 69,
		//tile description type
		Tile = 71,
		//tile group type
		TileGroup = 72,
		//smaller element contained in a tile 
		TilePart = 73,
		//static section of tile part (usually contains labels)
		TilePartStatic = 74,
		//content section of tile part (usually contains input control)
		TilePartContent = 75,
		TileManager = 76,
		TilePanel = 77,
		LayoutContainer = 78,
		HeaderStrip = 79,
		PropertyGrid = 80,
		PropertyGridItem = 81,
		TreeBodyEdit = 82,
		StatusTile = 83,
		HotFilter = 84,
		StatusTilePanel = 85,
		Splitter = 86,
		DockingPane = 87
	}
	enum SectionType { NONE, DIALOG, STRINGTABLE };
	enum SelectionType { SINGLE, NOSEL, EXTENDEDSEL, MULTIPLESEL };
	enum OwnerDrawType { ODNO, ODFIXED, ODVARIABLE };
	enum EtchedFrameType { EFNO=-1, EFALL= 0, EFHORZ = 1, EFVERT = 2};
	enum ListCtrlAlign { LC_TOP = 0, LC_LEFT = 1 };
	enum ListCtrlViewMode { LC_ICON = -1, LC_SMALLICON = 0, LC_LIST = 1, LC_REPORT = 2 };
	enum SpinCtrlAlignment { UNATTACHED = -1, SC_LEFT = 0, SC_RIGHT = 1 };
	[Flags]
	enum AcceleratorModifier { NONE = 0, }
	public enum HrcType { IDD = 0, IDC = 1, IDR_ACCELERATOR = 2, ID = 3, IDC_CURSOR = 4, IDB = 5, IDI = 6, PNG = 7, OTHER = 8 }

	/// <summary>
	/// Tabelle sql
	/// </summary>
	class TableStructure
	{
		List<string> columns = new List<string>();

		public List<string> Columns
		{
			get { return columns; }
		}
		public string Name { get; set; }

		internal void WriteTo(RCJsonWriter writer)
		{
			foreach (string col in columns)
			{
				writer.WriteValue(Name + "." + col);
			}
		}
	}
	class ParsedControlsStructure
	{
		public string Name { get; set; }
	}

	
	public class JsonConstants
	{
		public const string JSON_FOLDER_NAME = "JsonForms";
		public const string NAME = "name";
		public const string ID = "id";
		public const string TYPE = "type";
		public const string STYLE = "style";
		public const string EXSTYLE = "exStyle";
		public const string TEXT = "text";
		public const string ITEMS = "items";


		public const string CONTROLS = "controls";
		public const string TABLES = "tables";
		public const string COLUMNS = "columns";
		public const string DATA_TYPE = "dataType";

		public const string X = "x";
		public const string Y = "y";
		public const string WIDTH = "width";
		public const string HEIGHT = "height";
		public const string VISIBLE = "visible";
		public const string ENABLED = "enabled";
		public const string JSON_HEADER_EXT = ".hjson";
		public const string JSON_FORM_EXT = ".tbjson";
		public const string INTELLI_FILE_DB = "intellidb.json";
		public const string INTELLI_FILE_CONTROLS = "intellicontrols.json";
		public static string INTELLI_FILE_DBTS = "intellidbts.json";
		public static string HJSON_DOC_OWNER = "jsonusers.xml";
		public const string COL = "col";
		public const string CTRL = "ctrl";
		public static string TEXT_ALIGN = "textAlign";
		public static string VERT_ALIGN = "vertAlign";
		public static string MULTILINE = "multiline";
		public static string DEFAULT = "default";
		public static string OWNER_DRAW = "ownerDraw";
		public static string THREE_STATE = "threeState";
		public static string COMBO_TYPE = "comboType";
		public static string DBTS = "dbts";
		public static string FIELDS = "fields";
		public static string VARIABLE = "variable";
		public static string TAB_STOP = "tabStop";
		public static string RC_ID = "rcId";
		public static string FONT_SIZE = "fontSize";
		public static string FONT_NAME = "fontName";
		public static string NO_SORT_HEADER = "noSortHeader";
		public static string AUTO = "auto";
		public static string GROUP = "group";
		public static string LABEL_ON_LEFT = "labelOnLeft";
		public static string KEY = "key";
		public static string VIRTUAL_KEY = "virtualKey";
		public static string CONTROL = "control";
		public static string SHIFT = "shift";
		public static string ALT = "alt";
		public static string NO_INVERT = "noInvert";
		public static string CONTEXT = "context";
		public static string CONTROL_CLASS = "controlClass";
		public static string GLOBAL_CONTEXT = "Global context";
		public static string JSON_USERS_FILE = "jsonusers.xml";
	}
	public static class ExtendedObject
	{
		public static int Int(this object objects)
		{
			return int.Parse(objects.ToString());
		}

		public static ushort Int16(this object objects)
		{
			return ushort.Parse(objects.ToString());
		}

		public static bool Bool(this object objects)
		{
			return bool.Parse(objects.ToString());
		}
	}

	public static class ExtendedString
	{
		public static bool IsEmpty(this string strings)
		{
			return string.IsNullOrEmpty(strings);
		}

		public static int Int(this string strings)
		{
			return int.Parse(strings);
		}

		public static ushort Int16(this string strings)
		{
			return ushort.Parse(strings);
		}

		public static bool Bool(this string strings)
		{
			return bool.Parse(strings);
		}

		public static string TrimAll(this string strings)
		{
			return strings.TrimStart().TrimEnd();
		}

		public static string ConvertCRLF(this string strings)
		{
			return Regex.Replace(strings, "\\\\n|\\\\r|\\\\t", Replace);
		}
		static string Replace(Match m)
		{
			switch (m.Value)
			{
				case "\\r": return "\r";
				case "\\n": return "\n";
				case "\\t": return "\t";
				default: return m.Value;
			}
		}
		private static bool AdjustValue(string s, out bool not, out string val)
		{
			not = false;
			val = s.Trim();
			if (val.IsEmpty())
				return false;
			if (val.StartsWith("NOT "))
			{
				not = true;
				val = val.Substring(4).Trim();
			}
			return true;
		}
		public static int DecodeStyle(this string strings)
		{
			string[] tokens = strings.Split('|');
			WindowStyles ws = 0;
			foreach (string s in tokens)
			{
				bool not;
				string val;
				if (!AdjustValue(s, out not, out val))
					continue;
				if (not)
					ws &= ~(WindowStyles)Enum.Parse(typeof(WindowStyles), val);
				else
					ws |= (WindowStyles)Enum.Parse(typeof(WindowStyles), val);
			}
			return (int)ws;
		}
		public static int DecodeExStyle(this string strings)
		{
			string[] tokens = strings.Split('|');
			WindowExStyles ws = 0;
			foreach (string s in tokens)
			{
				bool not;
				string val;
				if (!AdjustValue(s, out not, out val))
					continue;
				if (not)
					ws &= ~(WindowExStyles)Enum.Parse(typeof(WindowExStyles), val);
				else
					ws |= (WindowExStyles)Enum.Parse(typeof(WindowExStyles), val);
			}
			return (int)ws;
		}

	}
	internal class HRCStructure
	{
		public HrcType Type = HrcType.OTHER;
		public string Owner = JsonConstants.GLOBAL_CONTEXT;
		public bool InSpecificFile = false;//va salvato in un hjson specifico, e non in quello globale
		public string Name = "";
		public int Value = 0;
		public override string ToString()
		{
			return string.Concat(Name, " - ", Value, " - ", Type);
		}
	}

	class MyComparer : IComparer<HRCStructure>
	{
		public int Compare(HRCStructure x, HRCStructure y)
		{
			int i = x.Type.CompareTo(y.Type);
			if (i != 0)
				return i;
			return x.Name.CompareTo(y.Name);
		}
	}

	internal class ParsedControlList : Dictionary<string, List<ParsedControlsStructure>>
	{}		

	internal class RCJsonWriter : JsonTextWriter
	{
		public RCJsonWriter(StreamWriter sw)
			: base(sw)
		{
		}

		public string RCFile { get; set; }
	}
}
