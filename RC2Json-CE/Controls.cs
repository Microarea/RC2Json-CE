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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC2Json
{
	enum TextAlignment { NONE = -1, LEFT, CENTER, RIGHT }
	enum VerticalAlignment { NONE = -1, TOP, CENTER, BOTTOM }
	enum ComboType { SIMPLE, DROPDOWN, DROPDOWNLIST }


	class StyleInfo
	{
		public bool Extended { get; set; }
		public bool Not { get; set; }
		public string Name { get; set; }
		public override string ToString()
		{
			return Name;
		}
	}
	internal abstract class BaseControlStructure
	{
		private List<StyleInfo> styles = new List<StyleInfo>();
		//per non creare tante proprietà, butto tutto qui dentro ciò che estraggo dagli stili
		//in alcuni casi mi torna comodo avere porprietà esplicite
		protected Dictionary<string, object> dynamicProperties = new Dictionary<string, object>();
		internal List<StyleInfo> Styles
		{
			get { return styles; }
		}

        public string Id;
		private string text = "";
		public string Text
		{
			get { return text; }
			set { text = value; }
		}



		public bool IsBitmap
		{
			get
			{
				foreach (var prop in dynamicProperties)
					if (prop.Key.Equals("bitmap"))
						return (bool)prop.Value;
				return false;
			}
		}




		public bool IsIcon { get { return Type == WndObjType.Image; } }

		public WndObjType Type = WndObjType.Undefined;
		public Size Size;
		public Point Location;
		public WindowStyles Style;
		public WindowExStyles ExStyle;
        protected bool useLocation = true;

		public BaseControlStructure()
		{
		}
		public override string ToString()
		{
			return string.Concat(Id, " - ", Text);
		}
		internal virtual void WriteTo(RCJsonWriter writer)
		{
			writer.WritePropertyName(JsonConstants.ID);
			writer.WriteValue(Id);

			writer.WritePropertyName(JsonConstants.TYPE);
			writer.WriteValue(Type.ToString());
			if ((int)Style != 0)
			{
				writer.WritePropertyName(JsonConstants.STYLE);
				writer.WriteValue(Style);
			}
			if ((int)ExStyle != 0)
			{
				writer.WritePropertyName(JsonConstants.EXSTYLE);
				writer.WriteValue(ExStyle);
			}

			
			if (!string.IsNullOrEmpty(Text))
			{
				string s = Text;
				if (IsBitmap || IsIcon)
				{
					s = Path.Combine(Path.GetDirectoryName(writer.RCFile), s);
					s = CopyToDestination(s);
				}
				writer.WritePropertyName(JsonConstants.TEXT);
				writer.WriteValue(s);
			}

            if (useLocation)
            {
                writer.WritePropertyName(JsonConstants.X);
                writer.WriteValue(Location.X);

                writer.WritePropertyName(JsonConstants.Y);
                writer.WriteValue(Location.Y);
            }

            writer.WritePropertyName(JsonConstants.WIDTH);
			writer.WriteValue(Size.Width);

			writer.WritePropertyName(JsonConstants.HEIGHT);
			writer.WriteValue(Size.Height);

			foreach (var prop in dynamicProperties)
			{
				writer.WritePropertyName(prop.Key);
				writer.WriteValue(prop.Value);
			}
		}

		private string CopyToDestination(string bmpFullPath)
		{
			FileInfo source = new FileInfo(bmpFullPath);
			if (!source.Exists)
				return "";
			string folder = Helper.FindBitmapFolder(bmpFullPath);
			if (string.IsNullOrEmpty(folder))
				return "";
			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);
			string file = Path.GetFileName(bmpFullPath);
			string targetFile = Path.Combine(folder, file);
			FileInfo fi = new FileInfo(targetFile);
			if (fi.Exists)
			{
				if (fi.FullName.Equals(source.FullName, StringComparison.InvariantCultureIgnoreCase))
					return file;
				fi.Attributes &= ~FileAttributes.ReadOnly;
			}
			File.Copy(bmpFullPath, targetFile, true);
			return file;
		}

		//rimuove lo stile
		public void AddStyle(string s, bool not, bool extended)
		{
			bool localNot;

			if (HasStyle(s, out localNot))
			{
				if (localNot == not)//se c'è già con lo stesso segno, non faccio nulla
					return;
				//se c'è già con segno opposto, lo rimuovo ed esco
				if (HasStyle(s, out localNot, true))
					return;

			}
			styles.Add(new StyleInfo { Name = s, Not = not, Extended = extended });
		}

		//rimuove lo stile
		public bool RemovePositiveStyle(string s)
		{
			bool not = false;
			return HasStyle(s, out not, true) && !not;

		}
		//rimuove lo stile
		public bool RemoveStyle(string s, out bool not)
		{
			return HasStyle(s, out not, true);

		}
		protected bool HasPositiveStyle(string s)
		{
			bool not = false;
			return HasStyle(s, out not) && !not;
		}
		//controlla se c'è lo stile
		protected bool HasStyle(string s, out bool not, bool remove = false)
		{
			for (int i = 0; i < styles.Count; i++)
			{
				StyleInfo si = styles[i];
				if (si.Name.Equals(s))
				{
					if (remove)
						styles.RemoveAt(i);
					not = si.Not;
					return true;
				}
			}
			not = false;
			return false;
		}

		internal void StylesToProperty()
		{
			StyleToProperty();

			foreach (StyleInfo si in styles)
			{
				if (si.Extended)
				{
					WindowExStyles e;
					if (!Enum.TryParse<WindowExStyles>(si.Name, out e))
					{
						Diagnostic.WriteLine("WARNING! Unknown style name: " + si.Name);
						continue;
					}

					ExStyle |= e;
				}
				else
				{
					WindowStyles e;
					if (!Enum.TryParse<WindowStyles>(si.Name, out e))
					{
						Diagnostic.WriteLine("WARNING! Unknown style name: " + si.Name);
						continue;
					}
					Style |= e;
				}
#if DEBUG
				Diagnostic.WriteLine(string.Concat("WARNING: residual style ", si.Name, " for control ", Id));
#endif
				
			}
		}

		protected virtual void StyleToProperty()
		{
			bool not;
			foreach (StyleMap m in StyleMap.Map)
				if (RemoveStyle(m.Style, out not))
				{
					if (not)
					{
						if (m.JsonValue is bool)
							dynamicProperties[m.JsonProperty] = !(bool)m.JsonValue;
						else
							throw new Exception("NOT operator not supported for style " + m.Style);
					}
					else
					{
						dynamicProperties[m.JsonProperty] = m.JsonValue;
					}
				}
			foreach (var s in StyleMap.IgnoredStyles)
				RemoveStyle(s, out not);
		}

		/// <summary>
		/// combina classe di finestra e style per determinare il tipo di controllo
		/// </summary>
		/// <param name="sClass"></param>
		/// <param name="style"></param>
		/// <returns></returns>
		internal void CalculateType(string sClass)
		{
			switch (sClass.ToUpper())
			{
				case "BUTTON":
					{
						{
							if (HasPositiveStyle("BS_OWNERDRAW"))
							{ Type = WndObjType.Button; break; }
							if (HasPositiveStyle("BS_3STATE"))
							{ Type = WndObjType.Check; break; }
							if (HasPositiveStyle("BS_AUTO3STATE"))
							{ Type = WndObjType.Check; break; }
							if (HasPositiveStyle("BS_AUTOCHECKBOX"))
							{ Type = WndObjType.Check; break; }
							if (HasPositiveStyle("BS_CHECKBOX"))
							{ Type = WndObjType.Check; break; }
							if (HasPositiveStyle("BS_GROUPBOX"))
							{ Type = WndObjType.Group; break; }
							if (HasPositiveStyle("BS_RADIOBUTTON"))
							{ Type = WndObjType.Radio; break; }
							if (HasPositiveStyle("BS_AUTORADIOBUTTON"))
							{ Type = WndObjType.Radio; break; }


							Type = WndObjType.Button; break;
						}

					}
				case "COMBOBOX": Type = WndObjType.Combo; break;
				case "EDIT": Type = WndObjType.Edit; break;
				case "LISTBOX": Type = WndObjType.List; break;
				case "STATIC": Type = WndObjType.Label; break;
				case "SYSTREEVIEW32": Type = WndObjType.Tree; break;
				case "SYSTABCONTROL32": Type = WndObjType.Tabber; break;
				case "SYSLISTVIEW32": Type = WndObjType.ListCtrl; break;
				case "MSCTLS_UPDOWN32": Type = WndObjType.Spin; break;
				case "MSCTLS_PROGRESS32": Type = WndObjType.ProgressBar; break;
				default:

					Type = WndObjType.GenericWndObj; break;

			}

		}
	}
	internal class ControlStructure : BaseControlStructure
	{
		public TextAlignment TextAlign = TextAlignment.NONE;
		public bool LabelOnLeft;//per check e radio
		public bool Automatic;//per check
		public bool Default;//per button
		public bool ThreeState;//per check;

		public string ControlClass;//per controllo generico, non previsto

		public ControlStructure()
		{
		}
		internal override void WriteTo(RCJsonWriter writer)
		{
			writer.WriteStartObject();

			base.WriteTo(writer);
			if (TextAlign != TextAlignment.NONE)
			{
				writer.WritePropertyName(JsonConstants.TEXT_ALIGN);
				writer.WriteValue(TextAlign);
			}

			if (Automatic)
			{
				writer.WritePropertyName(JsonConstants.AUTO);
				writer.WriteValue(true);
			}
			if (Default)
			{
				writer.WritePropertyName(JsonConstants.DEFAULT);
				writer.WriteValue(true);
			}
			if (LabelOnLeft)
			{
				writer.WritePropertyName(JsonConstants.LABEL_ON_LEFT);
				writer.WriteValue(true);
			}
			if (ThreeState)
			{
				writer.WritePropertyName(JsonConstants.THREE_STATE);
				writer.WriteValue(true);
			}
			if (!string.IsNullOrEmpty(ControlClass))
			{
				writer.WritePropertyName(JsonConstants.CONTROL_CLASS);
				writer.WriteValue(ControlClass);
			}

			writer.WriteEndObject();
		}



		/// <summary>
		/// Sposta l'informazione dallo style alla property esplicita
		/// </summary>
		protected override void StyleToProperty()
		{
			bool ex_right = RemovePositiveStyle("WS_EX_RIGHT");
			switch (Type)
			{
				case WndObjType.Check:
					if (RemovePositiveStyle("BS_AUTO3STATE"))
					{
						Automatic = true;
						ThreeState = true;
					}
					if (RemovePositiveStyle("BS_AUTOCHECKBOX"))
					{
						Automatic = true;
					}
					if (RemovePositiveStyle("BS_3STATE"))
					{
						ThreeState = true;
					}
					LabelOnLeft = ex_right;
					if (RemovePositiveStyle("BS_LEFTTEXT"))
					{
						LabelOnLeft = true;
					} if (RemovePositiveStyle("BS_RIGHTBUTTON"))
					{
						LabelOnLeft = true;
					}
					RemoveCommonButtonStyles();
					break;
				case WndObjType.Radio:
					//posizione del testo

					if (RemovePositiveStyle("BS_AUTORADIOBUTTON"))
					{
						Automatic = true;
					}

					LabelOnLeft = ex_right;
					if (RemovePositiveStyle("BS_LEFTTEXT"))
					{
						LabelOnLeft = true;
					} if (RemovePositiveStyle("BS_RIGHTBUTTON"))
					{
						LabelOnLeft = true;
					}
					RemoveCommonButtonStyles();
					break;
				case WndObjType.Group:
					LabelOnLeft = ex_right;
					if (RemovePositiveStyle("BS_LEFTTEXT"))
					{
						LabelOnLeft = true;
					} if (RemovePositiveStyle("BS_RIGHTBUTTON"))
					{
						LabelOnLeft = true;
					}
					RemoveCommonButtonStyles();
					break;
				case WndObjType.Button:

					if (RemovePositiveStyle("BS_DEFPUSHBUTTON"))
					{
						Default = true;
					}

					RemoveCommonButtonStyles();
					break;

				case WndObjType.Label:
					if (RemovePositiveStyle("SS_LEFT"))
					{
						TextAlign = TextAlignment.LEFT;
					}
					else if (RemovePositiveStyle("SS_CENTER"))
					{
						TextAlign = TextAlignment.CENTER;
					}
					else if (RemovePositiveStyle("SS_RIGHT"))
					{
						TextAlign = TextAlignment.RIGHT;
					}


					break;
				case WndObjType.Edit:
					if (RemovePositiveStyle("ES_LEFT"))
					{
						TextAlign = TextAlignment.LEFT;
					}
					else if (RemovePositiveStyle("ES_CENTER"))
					{
						TextAlign = TextAlignment.CENTER;
					}
					else if (RemovePositiveStyle("ES_RIGHT"))
					{
						TextAlign = TextAlignment.RIGHT;
					}

					break;
			}

			if (ex_right)
			{
				TextAlign = TextAlignment.RIGHT;
			}
			base.StyleToProperty();
		}

		private void RemoveCommonButtonStyles()
		{
			//allineamento orrizzontale
			if (RemovePositiveStyle("BS_LEFT"))
			{
				TextAlign = TextAlignment.LEFT;
			}
			else if (RemovePositiveStyle("BS_CENTER"))
			{
				TextAlign = TextAlignment.CENTER;
			}
			else if (RemovePositiveStyle("BS_RIGHT"))
			{
				TextAlign = TextAlignment.RIGHT;
			}



		}

	}


	internal class DialogStructure : BaseControlStructure
	{

		List<ControlStructure> controls = new List<ControlStructure>();
		public int FontSize = 0;
		public string FontName = "";
		private string context;
		public List<ControlStructure> Controls { get { return controls; } }
		public DialogStructure(string context)
		{
			Type = WndObjType.Panel;
			this.context = context;
            useLocation = false;
		}
		internal override void WriteTo(RCJsonWriter writer)
		{
			writer.WriteStartObject();
			base.WriteTo(writer);
			writer.WritePropertyName(JsonConstants.ITEMS);
			writer.WriteStartArray();
			foreach (var item in Controls)
			{
				item.WriteTo(writer);

			}
			writer.WriteEndArray();

			writer.WriteEndObject();
		}
	}

	class AcceleratorBlockStructure
	{ 
		public string Id;
		public List<AcceleratorStructure> Accelerators = new List<AcceleratorStructure>();

		internal void WriteTo(JsonWriter writer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName(JsonConstants.ID);
			writer.WriteValue(Id);

			writer.WritePropertyName(JsonConstants.TYPE);
			writer.WriteValue(WndObjType.Panel.ToString());

			writer.WritePropertyName("accelerators");
			writer.WriteStartArray();
			foreach (var acc in Accelerators)
			{
				acc.WriteTo(writer);

			}
			writer.WriteEndArray();
			writer.WriteEndObject();
		}
	}
	class AcceleratorStructure
	{
		public string Id;
		public bool Control;
		public bool Alt;
		public bool Shift;
		public bool VirtualKey;
		public string Key;
		public bool NoInvert;

		internal void WriteTo(JsonWriter writer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName(JsonConstants.ID);
			writer.WriteValue(Id);

			writer.WritePropertyName(JsonConstants.KEY);
			writer.WriteValue(IntKey);

			if (VirtualKey)
			{
				writer.WritePropertyName(JsonConstants.VIRTUAL_KEY);
				writer.WriteValue(true);
			}
			if (Control)
			{
				writer.WritePropertyName(JsonConstants.CONTROL);
				writer.WriteValue(true);
			}
			if (Shift)
			{
				writer.WritePropertyName(JsonConstants.SHIFT);
				writer.WriteValue(true);
			}
			if (Alt)
			{
				writer.WritePropertyName(JsonConstants.ALT);
				writer.WriteValue(true);
			}
			if (NoInvert)
			{
				writer.WritePropertyName(JsonConstants.NO_INVERT);
				writer.WriteValue(true);
			}
			writer.WriteEndObject();

		}
		

		public int IntKey
		{
			get
			{
				if (VirtualKey)
				{ 
					WindowsVirtualKey vk;
					if (Enum.TryParse<WindowsVirtualKey>(Key, out vk))
						return (int)vk;
					if (Enum.TryParse<WindowsVirtualKey>("K_" + Key, out vk))
						return (int)vk;
					return Char.Parse(Key);
					
				}

				return Char.Parse(Key);
			}
		}
	}
}
