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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;
using RC2Json;
using LexanParser = RC2Json.Parser;

namespace RC2Json
{
	class RCParser
	{
		/// <summary>parser di lexan che riconosce i token</summary>
		internal LexanParser lex;
		string prevLexeme;
		Token prevToken;

		internal List<DialogStructure> Dialogs { get { return dialogs; } }
		internal List<AcceleratorBlockStructure> Accelerators { get { return acceleratorBlocks; } }
		internal Dictionary<string, string> StringTable { get { return stringTable; } }
		internal Dictionary<string, string> Cursors { get { return cursors; } }
		public Dictionary<string, string> Bitmaps { get { return bitmaps; } }
		public Dictionary<string, string> Pngs { get { return pngs; } }
		public Dictionary<string, string> Icons { get { return icons; } }

		private Dictionary<string, string> stringTable = new Dictionary<string, string>();

		private Dictionary<string, string> bitmaps = new Dictionary<string, string>();
		private Dictionary<string, string> pngs = new Dictionary<string, string>();

		private Dictionary<string, string> cursors = new Dictionary<string, string>();
		private Dictionary<string, string> icons = new Dictionary<string, string>();

		private List<DialogStructure> dialogs = new List<DialogStructure>();
		private List<AcceleratorBlockStructure> acceleratorBlocks = new List<AcceleratorBlockStructure>();


		public List<string> Styles = new List<string>();
		private HRCParser hrcParser;

		/// <summary>Token da rilevare nel file per la localizzazione delle stringhe</summary>
		public enum RCToken
		{
			DIALOG = Token.USR00, DIALOGEX = Token.USR01, CHARACTERISTICS = Token.USR02, CLASS = Token.USR03, CAPTION = Token.USR04, CONTROL = Token.USR05,
			LTEXT = Token.USR06, RTEXT = Token.USR07, PUSHBUTTON = Token.USR08,
			LANGUAGE = Token.USR09, TEXTINCLUDE = Token.USR10, DESIGNINFO = Token.USR11,
			DEFPUSHBUTTON = Token.USR12, GROUPBOX = Token.USR13, EDITTEXT = Token.USR14,
			COMBOBOX = Token.USR15, LISTBOX = Token.USR16, STRINGTABLE = Token.USR17,
			AVI = Token.USR18, ICON = Token.USR19, BITMAP = Token.USR20,
			ACCELERATORS = Token.USR21, TOOLBAR = Token.USR22, MENU = Token.USR23,
			MENUITEM = Token.USR24, POPUP = Token.USR25, MENUEX = Token.USR26,
			CTEXT = Token.USR27, STYLE = Token.USR28, EXSTYLE = Token.USR29, FONT = Token.USR30, VERSION = Token.USR31,
			RADIOBUTTON = Token.USR32,
			CHECKBOX = Token.USR33,
			AUTO3STATE = Token.USR34,
			AUTOCHECKBOX = Token.USR35,
			AUTORADIOBUTTON = Token.USR36,
			PUSHBOX = Token.USR37,
			SCROLLBAR = Token.USR38,
			STATE3 = Token.USR39,
			DISCARDABLE = Token.USR40,
			CURSOR = Token.USR41,
			PNG = Token.USR42,
			NULLTOKEN = Token.USR99
		}

		//---------------------------------------------------------------------
		//---------------------------------------------------------------------
		public RCParser(HRCParser hrcParser)
		{
			this.hrcParser = hrcParser;
			lex = new LexanParser(LexanParser.SourceType.FromFile);
#if DEBUG
			lex.DoAudit = true;
#endif
			lex.PreprocessorDisabled = true;
			//Si aggiunge questa userKeyword per evitare conflitti
			lex.UserKeywords.Add("TEXT", Token.ID);

			//Aggiungo tutti i miei token che considero interessanti al fine di 
			//riconoscere le stringhe da esternalizzare
			lex.UserKeywords.Add("Dialog", RCToken.DIALOG);
			lex.UserKeywords.Add("DialogEx", RCToken.DIALOGEX);
			lex.UserKeywords.Add("Caption", RCToken.CAPTION);
			lex.UserKeywords.Add("CLASS", RCToken.CLASS);
			lex.UserKeywords.Add("CHARACTERISTICS", RCToken.CHARACTERISTICS);
			lex.UserKeywords.Add("FONT", RCToken.FONT);
			lex.UserKeywords.Add("VERSION", RCToken.VERSION);

			lex.UserKeywords.Add("EditText", RCToken.EDITTEXT);
			lex.UserKeywords.Add("ComboBox", RCToken.COMBOBOX);
			lex.UserKeywords.Add("ListBox", RCToken.LISTBOX);
			lex.UserKeywords.Add("PushButton", RCToken.PUSHBUTTON);
			lex.UserKeywords.Add("DefPushButton", RCToken.DEFPUSHBUTTON);
			lex.UserKeywords.Add("RadioButton", RCToken.RADIOBUTTON);
			lex.UserKeywords.Add("AUTO3STATE", RCToken.AUTO3STATE);

			lex.UserKeywords.Add("AUTOCHECKBOX", RCToken.AUTOCHECKBOX);
			lex.UserKeywords.Add("AUTORADIOBUTTON", RCToken.AUTORADIOBUTTON);
			lex.UserKeywords.Add("PUSHBOX", RCToken.PUSHBOX);
			lex.UserKeywords.Add("SCROLLBAR", RCToken.SCROLLBAR);
			lex.UserKeywords.Add("STATE3", RCToken.STATE3);
			lex.UserKeywords.Add("CheckBox", RCToken.CHECKBOX);
			lex.UserKeywords.Add("Groupbox", RCToken.GROUPBOX);
			lex.UserKeywords.Add("RText", RCToken.RTEXT);
			lex.UserKeywords.Add("LText", RCToken.LTEXT);
			lex.UserKeywords.Add("CText", RCToken.CTEXT);
			lex.UserKeywords.Add("Control", RCToken.CONTROL);

			lex.UserKeywords.Add("Language", RCToken.LANGUAGE);
			lex.UserKeywords.Add("TextInclude", RCToken.TEXTINCLUDE);
			lex.UserKeywords.Add("DesignInfo", RCToken.DESIGNINFO);
			lex.UserKeywords.Add("StringTable", RCToken.STRINGTABLE);
			lex.UserKeywords.Add("Avi", RCToken.AVI);
			lex.UserKeywords.Add("Icon", RCToken.ICON);
			lex.UserKeywords.Add("Bitmap", RCToken.BITMAP);
			lex.UserKeywords.Add("DISCARDABLE", RCToken.DISCARDABLE);
			lex.UserKeywords.Add("Accelerators", RCToken.ACCELERATORS);
			lex.UserKeywords.Add("Toolbar", RCToken.TOOLBAR);
			lex.UserKeywords.Add("Menu", RCToken.MENU);
			lex.UserKeywords.Add("MenuEx", RCToken.MENUEX);
			lex.UserKeywords.Add("MenuItem", RCToken.MENUITEM);
			lex.UserKeywords.Add("Popup", RCToken.POPUP);
			lex.UserKeywords.Add("Style", RCToken.STYLE);
			lex.UserKeywords.Add("ExStyle", RCToken.EXSTYLE);
			lex.UserKeywords.Add("Cursor", RCToken.CURSOR);
			lex.UserKeywords.Add("PNG", RCToken.PNG);


		}



		//---------------------------------------------------------------------
		public bool Parse(string currentFile)
		{
			try
			{

				lex.Open(currentFile);
				while (!lex.Eof)
				{
					prevLexeme = lex.CurrentLexeme;
					prevToken = lex.currentToken;
					switch (lex.LookAhead())
					{
						case (Token)RCToken.ICON:
							{
								ParseIcon();
								continue;
							}
						case (Token)RCToken.BITMAP:
							{
								ParseBitmap();
								continue;
							}
						case (Token)RCToken.PNG:
							{
								ParsePng();
								continue;
							}
						case (Token)RCToken.CURSOR:
							{
								ParseCursor();
								continue;
							}
						case (Token)RCToken.STRINGTABLE:
							{
								ParseStringTable();
								continue;
							}
						case (Token)RCToken.ACCELERATORS:
							{
								ParseAccelerators();
								continue;
							}
						case (Token)RCToken.DIALOG:
						case (Token)RCToken.DIALOGEX:
							{
								if (prevToken == Token.ID)
									ParseDialog();
								else
									lex.SkipToken();
								continue;
							}
						default:
							{
								lex.SkipToken();
								break;
							}
					}// switch
				}
			}
			finally
			{
				lex.Close();
			}
			//combino la caption presenti nelle stringtable
			foreach (DialogStructure dlg in dialogs)
			{
				string text;
				if (stringTable.TryGetValue(dlg.Id, out text))
				{
					dlg.Text = text.ConvertCRLF();
					stringTable.Remove(dlg.Id);
				}

				ExtractStyles(dlg);
			}

			return !lex.Error;
		}

		private void ParseAccelerators()
		{
			string id = prevLexeme;
			if (id == String.Empty)
				return;

			AcceleratorBlockStructure block = new AcceleratorBlockStructure { Id = id };

			lex.SkipToken();//ACCELERATORS
			while (!lex.Eof && !lex.Parsed(Token.BEGIN) && !lex.Parsed(Token.BRACEOPEN))
				lex.SkipToken();

			while (!lex.Eof && !lex.Parsed(Token.END) && !lex.Parsed(Token.BRACECLOSE))
			{
				AcceleratorStructure acc = new AcceleratorStructure();
				if (lex.LookAhead() == Token.TEXTSTRING)
				{
					string sEvent;
					if (!lex.ParseString(out sEvent))
						return;
					acc.Key = sEvent;
				}
				else
				{
					lex.SkipToken();
					acc.Key = lex.CurrentLexeme;
				}

				if (!lex.ParseComma())
					return;
				lex.SkipToken();
				acc.Id = lex.CurrentLexeme;

				while (lex.Parsed(Token.COMMA))
				{
					lex.SkipToken();

					if (lex.CurrentLexeme.Equals("ASCII", StringComparison.InvariantCultureIgnoreCase))
					{

					}
					else if (lex.CurrentLexeme.Equals("VIRTKEY", StringComparison.InvariantCultureIgnoreCase))
					{
						acc.VirtualKey = true;
					}
					else if (lex.CurrentLexeme.Equals("NOINVERT", StringComparison.InvariantCultureIgnoreCase))
					{
						acc.NoInvert = true;
					}
					else if (lex.CurrentLexeme.Equals("ALT", StringComparison.InvariantCultureIgnoreCase))
					{
						acc.Alt = true;
					}
					else if (lex.CurrentLexeme.Equals("SHIFT", StringComparison.InvariantCultureIgnoreCase))
					{
						acc.Shift = true;
					}
					else if (lex.CurrentLexeme.Equals("CONTROL", StringComparison.InvariantCultureIgnoreCase))
					{
						acc.Control = true;
					}
					else
					{
						lex.SetError("Invalid acceleration option: " + lex.CurrentLexeme);
					}
				}

				block.Accelerators.Add(acc);
			}
			if (block.Accelerators.Count > 0)
				acceleratorBlocks.Add(block);
		}

		public bool Check(string currentFile)
		{
			try
			{

				lex.Open(currentFile);
				while (!lex.Eof)
				{
					switch (lex.LookAhead())
					{
						case Token.INCLUDE:
							{
								CheckInclude();
								continue;
							}
						default:
							{
								lex.SkipToken();
								break;
							}
					}// switch
				}
			}
			finally
			{
				lex.Close();
			}
			return !lex.Error;
		}
		private void CheckInclude()
		{
			lex.SkipToken();//include
			string incl = "";
			if (lex.Parsed(Token.LT))
			{
				while (!lex.Parsed(Token.GT))
				{
					if (incl.Length > 0)
						incl += "\\";
					incl += lex.CurrentLexeme;
					lex.SkipToken();

				}
			}
			else
			{
				lex.ParseString(out incl);
			}
			if (incl.Equals("afxres.h", StringComparison.InvariantCultureIgnoreCase)
				|| incl.EndsWith(".rc", StringComparison.InvariantCultureIgnoreCase)
				|| incl.EndsWith(".h", StringComparison.InvariantCultureIgnoreCase)
				|| incl.EndsWith("extdoc.hrc", StringComparison.InvariantCultureIgnoreCase)
				|| incl.EndsWith("parsres.hrc", StringComparison.InvariantCultureIgnoreCase))
				return;
			if (!Path.GetFileNameWithoutExtension(incl).Equals(Path.GetFileNameWithoutExtension(lex.Filename), StringComparison.InvariantCultureIgnoreCase))
			{
				Diagnostic.WriteLine(string.Concat("Found ", incl, " in ", lex.Filename));
			}
		}

		private void ParseBitmap()
		{
			string IDD = prevLexeme;
			if (IDD == String.Empty)
				return;
			lex.SkipToken();//BITMAP
			lex.Parsed((Token)RCToken.DISCARDABLE);
			string bmp;
			if (lex.ParseFlatString(out bmp))
			{
				bitmaps[IDD] = bmp;
				int id = hrcParser.FindId(IDD);
				if (id != 0)
					bitmaps[id.ToString()] = bmp;
			}

		}
		private void ParsePng()
		{
			string IDD = prevLexeme;
			if (IDD == String.Empty)
				return;
			lex.SkipToken();//PNG
			lex.Parsed((Token)RCToken.DISCARDABLE);
			string bmp;
			if (lex.ParseFlatString(out bmp))
			{
				pngs[IDD] = bmp;
				int id = hrcParser.FindId(IDD);
				if (id != 0)
					pngs[id.ToString()] = bmp;
			}

		}

		private void ParseCursor()
		{
			string IDD = prevLexeme;
			if (IDD == String.Empty)
				return;
			lex.SkipToken();//CURSOR
			lex.Parsed((Token)RCToken.DISCARDABLE);
			string cursor;
			if (lex.ParseFlatString(out cursor))
			{
				cursors[IDD] = cursor;
			}

		}

		private void ParseIcon()
		{
			string IDD = prevLexeme;
			if (IDD == String.Empty)
				return;
			lex.SkipToken();//ICON
			lex.Parsed((Token)RCToken.DISCARDABLE);
			string ico;
			if (lex.ParseFlatString(out ico))
			{
				icons[IDD] = ico;

				int id = hrcParser.FindId(IDD);
				if (id != 0)
					icons[id.ToString()] = ico;
			}

		}

		////
		//---------------------------------------------------------------------
		bool ParseRect(BaseControlStructure c)
		{
			int x, y, w = 0, h = 0;
			if (!lex.ParseInt(out x))
				return false;
			if (!lex.ParseComma())
				return false;
			if (!lex.ParseInt(out y))
				return false;

			if (lex.Parsed(Token.COMMA))
			{
				if (!lex.ParseInt(out w))
					return false;
				if (!lex.ParseComma())
					return false;
				if (!lex.ParseInt(out h))
					return false;
			}
			c.Location = new Point(x, y);
			c.Size = new Size(w, h);
			return true;
		}


		enum StyleState { NOT, OR, STYLE };
		//---------------------------------------------------------------------
		bool ParseStyle(BaseControlStructure c, bool ex)
		{
			lex.SkipToken();//STYLE o EXSTYLE o ,

			StyleState state = StyleState.OR;//significa che mi aspetto uno style
			while (!lex.Eof)
			{
				switch (lex.LookAhead())
				{
					case Token.NOT:
						{
							//il not deve essere per forza dopo un OR
							if (state != StyleState.OR)
								return false;
							lex.SkipToken();
							state = StyleState.NOT;
							break;
						}

					case Token.ID:
						{
							//se ho appena parsato un token di stile, adesso deve esserci un OR, se non c'è significa che ho terminato la sezione di stili
							if (state == StyleState.STYLE)
								return true;//true, non è da considerarsi errore
							lex.SkipToken();
							c.AddStyle(lex.CurrentLexeme, state == StyleState.NOT, ex);

							state = StyleState.STYLE;
							break;
						}
					case Token.INTEGER:
					case Token.SHORT:
					case Token.BYTE:
						{
							//se ho appena parsato un token di stile, adesso deve esserci un OR, se non c'è significa che ho terminato la sezione di stili
							if (state == StyleState.STYLE)
								return true;//true, non è da considerarsi errore
							int i;
							lex.ParseInt(out i);
							if (i != 0)
								c.AddStyle(lex.CurrentLexeme, state == StyleState.NOT, ex);

							state = StyleState.STYLE;
							break;
						}
					case Token.BW_OR:
						{
							//l'OR deve per forza essere dopo uno style
							if (state != StyleState.STYLE)
								return false;
							lex.SkipToken();
							state = StyleState.OR;
							break;
						}
					default://se trovo un token non atteso, significa che ho terminato la sezione degli style
						return true;
				}
			}
			return true;
		}


		/// <summary>
		/// Parso la struttura della dialog.
		/// nameID DIALOGEX x, y, width, height [ , helpID] [optional-statements]  {control-statements}
		/// nameID DIALOG x, y, width, height  [optional-statements] {control-statements}
		/// </summary>
		//---------------------------------------------------------------------
		private void ParseDialog()
		{
			string IDD = prevLexeme;
			if (IDD == String.Empty)
				return;
			bool dialogEx = (RCToken)lex.currentToken == RCToken.DIALOGEX;
			lex.SkipToken();//DIALOG
			DialogStructure dlg = new DialogStructure(Helper.GetContextName(lex.Filename));
			dlg.Id = IDD;
			int dummy;
			lex.Parsed((Token)RCToken.DISCARDABLE);
			//parso il rettangolo (c'è sempre)
			if (!ParseRect(dlg))
				return;
			//parso l'help id (può non esserci, in caso di DIALOG)
			if (lex.LookAhead() == Token.COMMA) // [ , helpID] 
			{
				lex.SkipToken();
				if (!lex.ParseInt(out dummy))
					return;
			}
			//parso gli statements opzionali
			do
			{
				switch ((int)lex.LookAhead())
				{
					//ho trovto begin: fine degli statements opzionalied inizio del controlli
					case (int)Token.BEGIN:
						{
							ParseControls(dlg);
							dialogs.Add(dlg);
							return;//fine della dialog
						}
					case (int)RCToken.STYLE:
						{
							if (!ParseStyle(dlg, false))
								return;

							break;
						}
					case (int)RCToken.EXSTYLE:
						{
							if (!ParseStyle(dlg, true))
								return;
							break;
						}
					case (int)RCToken.CAPTION:
						{
							lex.SkipToken();
							string text;
							if (!lex.ParseString(out text))
								return;
							dlg.Text = text.ConvertCRLF();
							break;
						}
					case (int)RCToken.CHARACTERISTICS:
					case (int)RCToken.VERSION:
						{
							lex.SkipToken();
							if (!lex.Parsed(Token.INTEGER))
								return;
							break;
						}
					case (int)RCToken.FONT:
						{
							lex.SkipToken();
							int size, weight, italic, charset;
							string faceName;
							if (!lex.ParseInt(out size) ||
								!lex.ParseComma() ||
								!lex.ParseString(out faceName))
								return;
							dlg.FontName = faceName;
							if (lex.Parsed(Token.COMMA) && !lex.ParseInt(out weight))
								return;
							dlg.FontSize = size;
							if (lex.Parsed(Token.COMMA) && !lex.ParseInt(out italic))
								return;
							if (lex.Parsed(Token.COMMA) && !lex.ParseInt(out charset))
								return;
							break;
						}
					case (int)RCToken.CLASS:
						{
							lex.SkipToken();
							int i;
							string s;
							if (!lex.ParseInt(out i) && !lex.ParseString(out s))
								return;
							break;
						}
					default:
						{
							lex.SkipToken();
							break;
						}
				}
			} while (!lex.Eof);

		}


		//mette da parte tutti gli stili trovati
		//---------------------------------------------------------------------
		private void ExtractStyles(DialogStructure dlg)
		{
			//travasa alcuni stili nella proprietà corrispondenti, rimuovendoli dall'array degli stili
			dlg.StylesToProperty();
			foreach (var s in dlg.Styles)
				if (!Styles.Contains(s.Name))
					Styles.Add(s.Name);
			foreach (var item in dlg.Controls)
			{
				item.StylesToProperty();
				if (!string.IsNullOrEmpty(item.Text))
				{
					if (item.IsBitmap)
					{
						string s;
						if (bitmaps.TryGetValue(item.Text, out s))
						{
							item.Text = MakeAbsolutePath(s);
						}
						else
						{
							Diagnostic.WriteLine(string.Concat("ERROR - bitmap resource not found: ", item.Text, " - file: ", lex.Filename));
						}

					}
					else if (item.IsIcon)
					{
						string s;
						if (icons.TryGetValue(item.Text, out s))
						{
							item.Text = MakeAbsolutePath(s);
						}
						else
						{
							Diagnostic.WriteLine(string.Concat("ERROR - icon resource not found: ", item.Text, " - file: ", lex.Filename));
						}
					}
				}
				foreach (var s in item.Styles)
					if (!Styles.Contains(s.Name))
						Styles.Add(s.Name);
			}
		}

		//---------------------------------------------------------------------
		private string MakeAbsolutePath(string s)
		{
			return "";//temporaneamente disabilitato
			/*
			string rcFolder = Path.GetDirectoryName(lex.Filename);
			string file = Path.Combine(rcFolder, s);
			FileInfo fi = new FileInfo(file);
			if (fi.Exists)
				return fi.FullName;
			//TODOPERASSO errore
			return "";*/
		}


		/// <summary>
		/// Parso la struttura della stringtable .
		/// </summary>
		//---------------------------------------------------------------------
		private void ParseStringTable()
		{
			lex.SkipToken();//STRINGTABLE
			while (!lex.Eof && !lex.Parsed(Token.BEGIN))
				lex.SkipToken();
			while (!lex.Eof && !lex.Parsed(Token.END))
			{
				string id, text;
				if (!lex.ParseID(out id) || !lex.ParseString(out text))
					return;
				stringTable[id] = text;
			}


		}

		/// <summary>
		/// Parsa i controlli della dialog.
		/// </summary>
		//---------------------------------------------------------------------
		private void ParseControls(DialogStructure dlg)
		{
			lex.SkipToken();// BEGIN
			while (!lex.Eof)
			{
				switch ((int)lex.LookAhead())
				{
					case (int)Token.END:
						lex.SkipToken();
						return;//fine dei controlli
					case (int)RCToken.COMBOBOX:
						{
							ControlStructure c = ParseSpecificControl(false, WndObjType.Combo);
							if (c == null)
								return;
							dlg.Controls.Add(c);
							break;
						}
					case (int)RCToken.LISTBOX:
						{
							ControlStructure c = ParseSpecificControl(false, WndObjType.List);
							if (c == null)
								return;
							dlg.Controls.Add(c);
							break;
						}
					case (int)RCToken.EDITTEXT:
						{
							ControlStructure c = ParseSpecificControl(false, WndObjType.Edit);
							if (c == null)
								return;
							dlg.Controls.Add(c);
							break;
						}
					case (int)RCToken.GROUPBOX:
						{
							ControlStructure c = ParseSpecificControl(true, WndObjType.Group);
							if (c == null)
								return;
							dlg.Controls.Add(c);
							break;
						}
					case (int)RCToken.RADIOBUTTON:
						{
							ControlStructure c = ParseSpecificControl(true, WndObjType.Radio);
							if (c == null)
								return;
							dlg.Controls.Add(c);
							break;
						}
					case (int)RCToken.AUTORADIOBUTTON:
						{
							ControlStructure c = ParseSpecificControl(true, WndObjType.Radio);
							if (c == null)
								return;
							c.Automatic = true;
							dlg.Controls.Add(c);
							break;
						}
					case (int)RCToken.CHECKBOX:
						{
							ControlStructure c = ParseSpecificControl(true, WndObjType.Check);
							if (c == null)
								return;
							dlg.Controls.Add(c);
							break;
						}

					case (int)RCToken.AUTO3STATE:
						{
							ControlStructure c = ParseSpecificControl(true, WndObjType.Check);
							if (c == null)
								return;
							c.Automatic = true;
							c.ThreeState = true;
							dlg.Controls.Add(c);
							break;
						}

					case (int)RCToken.STATE3:
						{
							ControlStructure c = ParseSpecificControl(true, WndObjType.Check);
							if (c == null)
								return;
							c.ThreeState = true;
							dlg.Controls.Add(c);
							break;
						}
					case (int)RCToken.AUTOCHECKBOX:
						{
							ControlStructure c = ParseSpecificControl(true, WndObjType.Check);
							if (c == null)
								return;
							c.Automatic = true;
							dlg.Controls.Add(c);
							break;
						}

					case (int)RCToken.PUSHBOX:
						{
							ControlStructure c = ParseSpecificControl(true, WndObjType.Button);
							if (c == null)
								return;
							dlg.Controls.Add(c);
							break;
						}
					case (int)RCToken.PUSHBUTTON:
						{
							ControlStructure c = ParseSpecificControl(true, WndObjType.Button);
							if (c == null)
								return;
							dlg.Controls.Add(c);
							break;
						}
					case (int)RCToken.DEFPUSHBUTTON:
						{
							ControlStructure c = ParseSpecificControl(true, WndObjType.Button);
							if (c == null)
								return;
							c.Default = true;
							dlg.Controls.Add(c);
							break;
						}
					case (int)RCToken.LTEXT:
						{
							ControlStructure c = ParseSpecificControl(true, WndObjType.Label);
							if (c == null)
								return;
							c.TextAlign = TextAlignment.LEFT;
							dlg.Controls.Add(c);
							break;
						}
					case (int)RCToken.CTEXT:
						{
							ControlStructure c = ParseSpecificControl(true, WndObjType.Label);
							if (c == null)
								return;
							dlg.Controls.Add(c);
							c.TextAlign = TextAlignment.CENTER;
							break;
						}
					case (int)RCToken.RTEXT:
						{
							ControlStructure c = ParseSpecificControl(true, WndObjType.Label);
							if (c == null)
								return;
							c.TextAlign = TextAlignment.RIGHT;
							dlg.Controls.Add(c);
							break;
						}
					case (int)RCToken.ICON:
						{
							ControlStructure c = ParseIconControl();
							if (c == null)
								return;
							dlg.Controls.Add(c);
							break;
						}
					case (int)RCToken.CONTROL:
						{
							ControlStructure c = ParseGenericControl();
							if (c == null)
								return;
							dlg.Controls.Add(c);
							break;
						}
					case (int)RCToken.SCROLLBAR:
					default:
						{
							lex.SetError("Unsupported control: " + lex.CurrentLexeme);
							return;
						}
				}
			}
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Parse i controlli specifici che hanno la forma: <TIPO> [testo] id, x, y, width, height [, style [, extended-style]]
		/// </summary>
		/// <returns></returns>
		private ControlStructure ParseSpecificControl(bool hasText, WndObjType type)
		{
			lex.SkipToken();//control ID
			ControlStructure c = new ControlStructure();
			c.Type = type;

			if (hasText)
			{
				string text;
				if (!lex.ParseString(out text))
					return null;
				c.Text = text.ConvertCRLF();

				if (!lex.ParseComma())
					return null;
			}
			string id;
			if (!ParseControlName(out id))
				return null;
			c.Id = id;
			if (!lex.ParseComma())
				return null;
			if (!ParseRect(c))
				return null;
			if (lex.LookAhead(Token.COMMA) && !ParseStyle(c, false))
				return null;

			if (lex.LookAhead(Token.COMMA) && !ParseStyle(c, true))
				return null;

			return c;

		}

		//---------------------------------------------------------------------
		private bool ParseControlName(out string id)
		{
			id = "";
			if (lex.LookAhead(Token.ID))
				return lex.ParseID(out id);
			int i;
			if (!lex.ParseSignedInt(out i))
				return false;
			id = (i == -1) ? "IDC_STATIC" : i.ToString();
			return true;

		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Parse i controlli icona che hanno la forma: ICON text, id, x, y [, width, height, style [, extended-style]]
		/// </summary>
		/// <returns></returns>
		private ControlStructure ParseIconControl()
		{
			lex.SkipToken();//ICON
			ControlStructure c = new ControlStructure();
			c.Type = WndObjType.Image;
			string text;
			if (!ParseString(out text))
				return null;
			c.Text = text;
			if (!lex.ParseComma())
				return null;
			string id;
			if (!ParseControlName(out id))
				return null;
			c.Id = id;

			if (!lex.ParseComma())
				return null;

			if (!ParseRect(c))
				return null;
			if (lex.LookAhead(Token.COMMA) && !ParseStyle(c, false))
				return null;

			if (lex.LookAhead(Token.COMMA) && !ParseStyle(c, true))
				return null;

			return c;
		}
		//---------------------------------------------------------------------
		/// <summary>
		/// Parse i controlli generici che hanno la forma: CONTROL text, id, class, style, x, y, width, height [, extended-style]
		/// </summary>
		/// <returns></returns>
		private ControlStructure ParseGenericControl()
		{
			lex.SkipToken();//CONTROL
			ControlStructure c = new ControlStructure();
			c.AddStyle("WS_TABSTOP", true, false);
			string text;
			if (!ParseString(out text))
				return null;
			c.Text = text.ConvertCRLF();
			if (!lex.ParseComma())
				return null;
			string id;
			if (!ParseControlName(out id))
				return null;
			c.Id = id;

			if (!lex.ParseComma())
				return null;
			string sClass;
			if (!ParseClass(out sClass))
				return null;
			if (!lex.LookAhead(Token.COMMA))//la ParseStyle mangia il comma
				return null;
			if (!ParseStyle(c, false))
				return null;
			c.CalculateType(sClass);
			if (c.Type == WndObjType.GenericWndObj)
				c.ControlClass = sClass;
			if (!lex.ParseComma())
				return null;
			if (!ParseRect(c))
				return null;

			if (lex.LookAhead(Token.COMMA) && !ParseStyle(c, true))
				return null;

			return c;
		}

		private bool ParseString(out string text)
		{
			text = "";
			if (lex.LookAhead(Token.TEXTSTRING))
			{
				return lex.ParseString(out text);
			}

			if (lex.LookAhead(Token.ID))
			{
				string id;
				if (!lex.ParseID(out id))
					return false;

				text = id;
				return true;
			}
			int nText;
			if (!lex.ParseInt(out nText))
				return false;

			text = nText.ToString();
			foreach (var h in hrcParser.HrcStructure)
			{
				if (h.Value.Equals(text))
				{
					text = h.Name;
					break;
				}
			}
			return true;

		}

		private bool ParseClass(out string sClass)
		{
			if (lex.LookAhead(Token.TEXTSTRING))

				return lex.ParseString(out sClass);
			sClass = "";
			int nClass;
			if (!lex.ParseInt(out nClass))
				return false;
			//TODOPERASSO classe come ATOM
			return true;
		}
	}
	class HRCParser
	{

		List<HRCStructure> hrcStructure = new List<HRCStructure>();

		internal List<HRCStructure> HrcStructure
		{
			get { return hrcStructure; }
		}


		public void Parse(string hrcText)
		{
			string line;

			using (StringReader sReader = new StringReader(hrcText))
			{
				while ((line = sReader.ReadLine()) != null)
				{
					HRCStructure hLine = DecodeHRCLine(line);
					if (hLine != null)
						hrcStructure.Add(hLine);
				}
			}
			hrcStructure.Sort(new MyComparer());
		}

		private HRCStructure DecodeHRCLine(string line)
		{
			string[] token = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			if (token.Length == 0)
				return null;

			if (string.Compare(token[0], "#define", true) != 0)
				return null;

			if (!token[1].StartsWith("ID"))
				return null;

			HRCStructure hrcStruct = new HRCStructure();

			hrcStruct.Name = token[1];
			int val;
			if (int.TryParse(token[2], out val))
			{
				hrcStruct.Value = val;
			}
			else
			{
				foreach (var v in hrcStructure)
					if (v.Name.Equals(token[2]))
					{
						hrcStruct.Value = v.Value;
						break;
					}
			}

			return hrcStruct;
		}



		internal void AssignTypes(RCParser rcParser)
		{
			foreach (var dlg in rcParser.Dialogs)
			{
				HRCStructure hrc = Find(dlg.Id);
				if (hrc != null)
				{
					hrc.Type = HrcType.IDD;
					hrc.Owner = dlg.Id;
					hrc.InSpecificFile = true;
				}
				foreach (var c in dlg.Controls)
				{

					hrc = Find(c.Id);
					if (hrc != null)
					{
						hrc.Type = HrcType.IDC;
						hrc.Owner = dlg.Id;
						hrc.InSpecificFile = true;
					}
					//il controllo è una bitmap: vado nella lista delle bitmap e vedo se lo trovo
					//e assegno il path della bitmap al text
					if (c.IsBitmap && string.IsNullOrEmpty(c.Text))
					{
						string path;
						if (rcParser.Bitmaps.TryGetValue(c.Id, out path))
						{
							c.Text = path;
							rcParser.Bitmaps.Remove(c.Id);
						}
					}
					//il controllo è una icona: vado nella lista delle icone e vedo se lo trovo
					//e assegno il path della bitmap al text
					if (c.IsIcon && string.IsNullOrEmpty(c.Text))
					{
						string path;
						if (rcParser.Icons.TryGetValue(c.Id, out path))
						{
							c.Text = path;
							rcParser.Icons.Remove(c.Id);
						}
					}
					//vedo se ci sono conflitti di id (IDC_STATIC ad esempio può essere presente più volte)
					bool changed = false;
					foreach (var c1 in dlg.Controls)
					{
						if (c1.Id.Equals(c.Id) && c1 != c)
						{
							//ne ho trovato un altro con lo stesso id, devo calcolarne uno nuovo
							int i = 0; 
							string newId = c.Id;
							bool conflict = true;
							while (conflict)
							{
								conflict = false;
								newId = c.Id + '_' + (++i);
								foreach (var c2 in dlg.Controls)
								{
									if (c2.Id.Equals(newId))
									{
										conflict = true;//esiste tuttora
										break;
									}
								}
							}
							c.Id = newId;
							changed = true;
						}
					}
					if (changed)
					{
						HRCStructure newItem = new HRCStructure();
						newItem.Name = c.Id;
						newItem.Owner = dlg.Id;
						newItem.Type = HrcType.IDC;
						newItem.InSpecificFile = true;
						if (changed)
							hrcStructure.Add(newItem);
					}
				}
			}

			foreach (var acc in rcParser.Accelerators)
			{
				HRCStructure hrc = Find(acc.Id);
				if (hrc != null)
				{
					hrc.Type = HrcType.IDR_ACCELERATOR;
					hrc.Owner = acc.Id;
					hrc.InSpecificFile = true;
				}
				foreach (var a in acc.Accelerators)
				{
					hrc = Find(a.Id);
					if (hrc != null)
					{
						hrc.Type = HrcType.ID;
						hrc.Owner = acc.Id;
						hrc.InSpecificFile = true;
					}
				}
			}

			foreach (var cur in rcParser.Cursors.Keys)
			{
				HRCStructure hrc = Find(cur);
				if (hrc != null)
				{
					hrc.Owner = JsonConstants.GLOBAL_CONTEXT;
					hrc.Type = HrcType.IDC_CURSOR;
				}

			}
			foreach (var bmp in rcParser.Bitmaps.Keys)
			{
				HRCStructure hrc = Find(bmp);
				if (hrc != null )
				{
					if (hrc.Type != HrcType.OTHER)
					{
						Diagnostic.WriteLine(string.Concat("WARNING: ID ", bmp, " already has type ", hrc.Type, ", cannot assign type ", HrcType.IDB));
						continue;
					}
					hrc.Owner = JsonConstants.GLOBAL_CONTEXT;
					hrc.Type = HrcType.IDB;
				}
			}
			foreach (var png in rcParser.Pngs.Keys)
			{
				HRCStructure hrc = Find(png);
				if (hrc != null)
				{
					if (hrc.Type != HrcType.OTHER)
					{
						Diagnostic.WriteLine(string.Concat("WARNING: ID ", png, " already has type ", hrc.Type, ", cannot assign type ", HrcType.PNG));
						continue;
					}
					hrc.Owner = JsonConstants.GLOBAL_CONTEXT;
					hrc.Type = HrcType.PNG;
				}
			}
			foreach (var ico in rcParser.Icons.Keys)
			{
				HRCStructure hrc = Find(ico);
				if (hrc != null)
				{
					if (hrc.Type != HrcType.OTHER)
					{
						Diagnostic.WriteLine(string.Concat("WARNING: ID ", ico, " already has type ", hrc.Type, ", cannot assign type ", HrcType.IDI));
						continue;
					}
					hrc.Owner = JsonConstants.GLOBAL_CONTEXT;
					hrc.Type = HrcType.IDI;
				}
			}
			foreach (var hrc in hrcStructure)
			{
				if (hrc.Type == HrcType.OTHER)
				{
					if (hrc.Name.StartsWith("IDC_"))
						hrc.Type = HrcType.IDC;
					else if (hrc.Name.StartsWith("ID_"))
						hrc.Type = HrcType.ID;
				}
			}
			hrcStructure.Sort(new HRCStructureComparer());
		}

		private HRCStructure Find(string name)
		{
			foreach (var hrc in hrcStructure)
				if (hrc.Name.Equals(name))
					return hrc;
			return null;
		}

		internal int FindId(string name)
		{
			HRCStructure h = Find(name);
			return h == null ? 0 : h.Value;
		}
	}
	class HRCStructureComparer : IComparer<HRCStructure>
	{
		public int Compare(HRCStructure x, HRCStructure y)
		{
			int i = x.InSpecificFile.CompareTo(y.InSpecificFile);
			if (i != 0)
				return -i;
			i = x.Owner.CompareTo(y.Owner);
			if (i != 0)
			{
				if (x.Owner == JsonConstants.GLOBAL_CONTEXT)
					return 1;
				if (y.Owner == JsonConstants.GLOBAL_CONTEXT)
					return -1;
				return i;
			}
			i = ((int)x.Type).CompareTo(((int)y.Type));
			if (i != 0)
			{
				return i;
			}
			return x.Name.CompareTo(y.Name);
		}
	}

	class SQLParser
	{

		LexanParser lex;
		List<TableStructure> tables = new List<TableStructure>();

		internal List<TableStructure> Tables
		{
			get { return tables; }
		}
		enum SQLToken
		{
			TABLE = Token.USR00,
			CREATE = Token.USR01,
			DBO = Token.USR02,
			CONSTRAINT = Token.USR03,
			PRIMARY = Token.USR04,
			FOREIGN = Token.USR05,
			KEY = Token.USR06,
			REFERENCES = Token.USR07,
			OBJECT_ID = Token.USR08,
			ALTER = Token.USR09,
			GO = Token.USR10,
			ADD = Token.USR11,
			END = Token.USR12,
			DELETE = Token.USR13,
			INSERT = Token.USR14,
			INTO = Token.USR15,
			DOT = Token.USR16,
			INDEX = Token.USR17,
			VIEW = Token.USR18,
			AS = Token.USR19,
			SELECT = Token.USR20
		}



		//---------------------------------------------------------------------
		public SQLParser()
		{


			lex = new LexanParser(LexanParser.SourceType.FromFile);
			lex.PreprocessorDisabled = true;
#if DEBUG
			lex.DoAudit = true;
#endif

			lex.UserKeywords.Add("TABLE", SQLToken.TABLE);
			lex.UserKeywords.Add("CREATE", SQLToken.CREATE);
			lex.UserKeywords.Add("dbo", SQLToken.DBO);
			lex.UserKeywords.Add("CONSTRAINT", SQLToken.CONSTRAINT);
			lex.UserKeywords.Add("PRIMARY", SQLToken.PRIMARY);
			lex.UserKeywords.Add("FOREIGN", SQLToken.FOREIGN);
			lex.UserKeywords.Add("KEY", SQLToken.KEY);
			lex.UserKeywords.Add("REFERENCES", SQLToken.REFERENCES);
			lex.UserKeywords.Add("object_id", SQLToken.OBJECT_ID);
			lex.UserKeywords.Add("ALTER", SQLToken.ALTER);
			lex.UserKeywords.Add("GO", SQLToken.GO);
			lex.UserKeywords.Add("ADD", SQLToken.ADD);
			lex.UserKeywords.Add("END", SQLToken.END);
			lex.UserKeywords.Add("DELETE", SQLToken.DELETE);
			lex.UserKeywords.Add("INSERT", SQLToken.INSERT);
			lex.UserKeywords.Add("INTO", SQLToken.INTO);
			lex.UserKeywords.Add(".", SQLToken.DOT);
			lex.UserKeywords.Add("INDEX", SQLToken.INDEX);
			lex.UserKeywords.Add("VIEW", SQLToken.VIEW);
			lex.UserKeywords.Add("AS", SQLToken.AS);
			lex.UserKeywords.Add("SELECT", SQLToken.SELECT);
		}

		//---------------------------------------------------------------------
		public bool Parse(string currentFile)
		{
			try
			{

				lex.Open(currentFile);
				while (!lex.Eof)
				{
					switch (lex.LookAhead())
					{
						case (Token)SQLToken.CREATE:
							{
								if (!ParseCreateTable())
									return false;
								continue;
							}


						default:
							{
								lex.SkipToken();
								break;
							}
					}// switch
				}
			}
			finally
			{
				lex.Close();
			}

			return !lex.Error;
		}

		//---------------------------------------------------------------------------------------------------
		private bool ParseTable(out string tableName)
		{
			tableName = string.Empty;

			// mi aspetto i seguenti tokens: 
			// [dbo].[<nome_tabella>]
			return lex.ParseSquareOpen() &&
				lex.ParseTag((Token)SQLToken.DBO) &&
				lex.ParseSquareClose() &&
				lex.ParseTag((Token)SQLToken.DOT) &&
				lex.ParseSquareOpen() &&
				lex.ParseID(out tableName) &&
				lex.ParseSquareClose();
		}
		//---------------------------------------------------------------------------------------------------
		private bool ParseColumn(string tableName, out string columnName)
		{
			columnName = String.Empty;
			switch (lex.LookAhead())
			{
				case Token.ID:
					if (!lex.ParseID(out columnName)) return false;
					break;
				case Token.SQUARECLOSE:
					if (!lex.ParseSquareClose()) return false;
					break;
				default:
					//serve per evitare di andare in syntax error se una colonna si chiama come
					//uno dei nostri token (es: path, mail, note, Quantity,  etc.)
					columnName = lex.LookAhead().ToString();
					lex.SkipToken();
					break;
			}
			int roundToClose = 1;

			bool goOn = true;
			while (goOn && !lex.Eof)
			{
				switch (lex.LookAhead())
				{
					case (Token)SQLToken.GO: goOn = false; break;
					case (Token)SQLToken.CONSTRAINT: goto default;//goOn = false;					break;
					case (Token)SQLToken.END: goOn = false; break;
					case Token.COMMA: goOn = (roundToClose) > 1; goto default;
					case Token.ROUNDOPEN: roundToClose++; goto default;
					case Token.ROUNDCLOSE: goOn = (--roundToClose) > 0; goto default;
					default: lex.SkipToken(); break;
				};
			}

			return columnName != string.Empty;
		}
		//---------------------------------------------------------------------------------------------------
		private bool ParseCreateTable()
		{
			TableStructure table = new TableStructure();
			lex.SkipToken();//CREATE
			if (!lex.Parsed((Token)SQLToken.TABLE))
				return false;
			string tableName = string.Empty, columnName;
			try
			{
				if (!ParseTable(out tableName)) return false;

				table.Name = tableName;
				if (!lex.ParseOpen()) return false;

				Token t;

				while ((t = lex.LookAhead()) != Token.ROUNDCLOSE && !lex.Eof)
				{
					lex.SkipToken();
					switch (t)
					{
						case (Token)SQLToken.CONSTRAINT:
							tables.Add(table);
							return true;

						case Token.SQUAREOPEN:
							if (!ParseColumn(tableName, out columnName))
								return false;
							table.Columns.Add(columnName);
							break;
					}
				}
				tables.Add(table);
				return true;
			}
			catch
			{
				return false;
			}

		}
	}

	class CPPParser
	{
		string[] prevLexemes = Enumerable.Repeat("", 30).ToArray();
		LexanParser lex;
		ParsedControlList controls;
		CppInfo cppInfo;
		enum CPPToken
		{
			BEGIN_CONTROLS_TYPE = Token.USR00,
			DATATYPE = Token.USR01,
			END_CONTROLS_TYPE = Token.USR02,
			IMPLEMENT_DYNCREATE = Token.USR03,
			IMPLEMENT_DYNAMIC = Token.USR04,
			IMPLEMENT_SERIAL = Token.USR05,
			ON_ATTACH_DATA = Token.USR06,
			NEW = Token.USR07,
			RUNTIME_CLASS = Token.USR08,
			_NS_DBT = Token.USR09,
			_NS_FLD = Token.USR10,
			_NS_LFLD = Token.USR11,
			BIND_RECORD = Token.USR12,
			CLASS = Token.USR13,
			TB_EXPORT = Token.USR14,
			BEGIN_DOCUMENT = Token.USR15,
			END_DOCUMENT = Token.USR16,
			REGISTER_MASTER_TEMPLATE = Token.USR17,
			_NS_DOC = Token.USR18,
			INCLUDE = Token.USR19
		}



		//---------------------------------------------------------------------
		public CPPParser(ParsedControlList controls, CppInfo cppInfo)
		{
			this.controls = controls;
			this.cppInfo = cppInfo;
			lex = new LexanParser(LexanParser.SourceType.FromFile);
			lex.PreprocessorDisabled = true;
#if DEBUG
			lex.DoAudit = true;
#endif

			lex.UserKeywords.Add("BEGIN_CONTROLS_TYPE", CPPToken.BEGIN_CONTROLS_TYPE);
			lex.UserKeywords.Add("END_CONTROLS_TYPE", CPPToken.END_CONTROLS_TYPE);
			lex.UserKeywords.Add("DataType", CPPToken.DATATYPE);
			lex.UserKeywords.Add("IMPLEMENT_DYNCREATE", CPPToken.IMPLEMENT_DYNCREATE);
			lex.UserKeywords.Add("IMPLEMENT_DYNAMIC", CPPToken.IMPLEMENT_DYNAMIC);
			lex.UserKeywords.Add("IMPLEMENT_SERIAL", CPPToken.IMPLEMENT_SERIAL);
			lex.UserKeywords.Add("OnAttachData", CPPToken.ON_ATTACH_DATA);
			lex.UserKeywords.Add("new", CPPToken.NEW);
			lex.UserKeywords.Add("RUNTIME_CLASS", CPPToken.RUNTIME_CLASS);
			lex.UserKeywords.Add("_NS_DBT", CPPToken._NS_DBT);
			lex.UserKeywords.Add("_NS_FLD", CPPToken._NS_FLD);
			lex.UserKeywords.Add("_NS_LFLD", CPPToken._NS_LFLD);
			lex.UserKeywords.Add("BindRecord", CPPToken.BIND_RECORD);
			lex.UserKeywords.Add("class", CPPToken.CLASS);
			lex.UserKeywords.Add("TB_EXPORT", CPPToken.TB_EXPORT);
			lex.UserKeywords.Add("BEGIN_DOCUMENT", CPPToken.BEGIN_DOCUMENT);
			lex.UserKeywords.Add("END_DOCUMENT", CPPToken.END_DOCUMENT);
			lex.UserKeywords.Add("REGISTER_MASTER_TEMPLATE", CPPToken.REGISTER_MASTER_TEMPLATE);
			lex.UserKeywords.Add("_NS_DOC", CPPToken._NS_DOC);
			lex.UserKeywords.Add("INCLUDE", CPPToken.INCLUDE);


		}

		//---------------------------------------------------------------------
		public bool Parse(string currentFile)
		{
			try
			{

				lex.Open(currentFile);
				while (!lex.Eof)
				{
					for (int i = prevLexemes.Length - 1; i > 0; i--)
					{
						prevLexemes[i] = prevLexemes[i - 1];
					}
					prevLexemes[0] = lex.CurrentLexeme;

					switch (lex.LookAhead())
					{
						case (Token)CPPToken.BEGIN_CONTROLS_TYPE:
							{
								if (!ParseControlSection())
									return false;
								continue;
							}
						case (Token)CPPToken.IMPLEMENT_DYNAMIC:
						case (Token)CPPToken.IMPLEMENT_DYNCREATE:
						case (Token)CPPToken.IMPLEMENT_SERIAL:
							ParseImplementMacro();
							break;
						case (Token)CPPToken.ON_ATTACH_DATA:
							ParseOnAttachData();
							break;
						case (Token)CPPToken._NS_DBT:
							ParseDBTConstructor();
							break;
						case (Token)CPPToken.BIND_RECORD:
							ParseBindRecord();
							break;
						case (Token)CPPToken.CLASS:
							ParseClassDeclatarion();
							break;
						case (Token)CPPToken.BEGIN_DOCUMENT:
							ParseDocumentInterface();
							break;
						case (Token)CPPToken.INCLUDE:
							ParseInclude();
							break;
						default:
							{
								lex.SkipToken();
								break;
							}
					}// switch
				}
			}
			finally
			{
				lex.Close();
			}

			return !lex.Error;
		}

		private void ParseInclude()
		{
			lex.SkipToken();//include
			string include = "";
			if (lex.Parsed(Token.LT))
			{
				while (!lex.Parsed(Token.GT))
				{
					if (include.Length > 0)
						include += "\\";
					include += lex.CurrentLexeme;
					lex.SkipToken();
				}
			}
			else
			{
				if (!lex.ParseString(out include))
					return;
			}
			if (include.EndsWith(".hjson", StringComparison.InvariantCultureIgnoreCase))
			{
				cppInfo.AddInclude(lex.Filename, include);
			}
		}


		private void ParseDocumentInterface()
		{
			lex.SkipToken();//BEGIN_DOCUMENT
			if (!lex.ParseOpen())
				return;
			if (lex.Parsed((Token)CPPToken._NS_DOC) && !lex.ParseOpen())
				return;
			string docName;
			if (!lex.ParseFlatString(out docName))
				return;
			while (!lex.Parsed((Token)CPPToken.REGISTER_MASTER_TEMPLATE))
			{
				if (lex.currentToken == (Token)CPPToken.END_DOCUMENT)
					return;
				lex.SkipToken();
			}
			if (!lex.ParseOpen())
				return;
			string view = "";
			while (!lex.Parsed(Token.ROUNDCLOSE))
			{
				view = lex.CurrentLexeme;
				lex.SkipToken();
			}
			if (string.IsNullOrEmpty(view))
				return;

			CppClass c = cppInfo.GetClass(view);
			c.DocName = docName;
		}

		private void ParseClassDeclatarion()
		{
			lex.SkipToken();//class
			lex.Parsed((Token)CPPToken.TB_EXPORT);

			lex.SkipToken();//class name
			string className = lex.CurrentLexeme;
			if (lex.Parsed(Token.SEP))//dichiarazione di classe forward
				return;

			cppInfo.GetClass(className).DeclarationFile = lex.Filename;

			//mi porto all'apertura della dichiarazione di classe
			while (!lex.Eof && !lex.Parsed(Token.BRACEOPEN))
				lex.SkipToken();
			int openCount = 1;//ne ho aperta una
			string fieldName, dataObj;
			while (!lex.Eof && openCount > 0)
			{
				if (lex.Parsed(Token.BRACEOPEN))
					openCount++;
				else if (lex.Parsed(Token.BRACECLOSE))
				{
					openCount--;
				}
				else
				{
					//ho trovato una potenziale dichiarazione di variabile di tipo DataObj
					if (lex.CurrentLexeme.StartsWith("Data"))
					{
						dataObj = lex.CurrentLexeme;
						lex.SkipToken();//DataObj
						List<string> vars = new List<string>();
						while (!lex.Eof)
						{
							if (!lex.Parsed(Token.ID))
							{
								vars.Clear();
								break;
							}
							fieldName = lex.CurrentLexeme;
							//dopo devo avere o un'assegnazione, o una virgola di separazione, o la fine di istruzione
							//diversamente, non sono nel caso di dichiarazione di variabile
							if (lex.Parsed(Token.SEP))
							{
								//la metto da parte, non so ancora se è buona
								vars.Add(fieldName);
								break;//ho trovato fine istruzione; 
							}
							else if (lex.Parsed(Token.COMMA))//più variabili dichiarate in cascata
							{
								//la metto da parte, non so ancora se è buona
								vars.Add(fieldName);
							}
							else
							{
								//trovato token non valido: falso allarme!
								vars.Clear();
								break;
							}
						}
						foreach (var v in vars)
							cppInfo.AddRecordDataObj(className, dataObj, v, lex.Filename, lex.CurrentPos, lex.CurrentLine);
					}
					else
					{
						lex.SkipToken();
					}
				}

			}

		}

		//---------------------------------------------------------------------------------------------------
		private void ParseBindRecord()
		{
			lex.SkipToken();//BindRecord
			if (!lex.Parsed(Token.ROUNDOPEN) || !lex.Parsed(Token.ROUNDCLOSE) || !lex.Parsed(Token.BRACEOPEN))
				return;
			string className = prevLexemes[2];

			int openCount = 1;//ne ho aperta una
			string colName;
			while (!lex.Eof && openCount > 0)
			{

				if (lex.Parsed(Token.BRACEOPEN))
					openCount++;
				else if (lex.Parsed(Token.BRACECLOSE))
				{
					openCount--;
				}
				else if (lex.Parsed((Token)CPPToken._NS_FLD) || lex.Parsed((Token)CPPToken._NS_LFLD))
				{
					if (lex.Parsed(Token.ROUNDOPEN) && lex.ParseFlatString(out colName) && lex.Parsed(Token.ROUNDCLOSE) && lex.Parsed(Token.COMMA) && lex.Parsed(Token.ID))
					{
						cppInfo.AddRecordColumn(className, colName, lex.CurrentLexeme, lex.Filename, lex.CurrentPos, lex.CurrentLine);
					}
				}
				else
				{
					lex.SkipToken();
				}

			}
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseDBTConstructor()
		{
			//sono nel costruttore del dbt, ho già parsato i token con le informazioni che mi servono (la classe del dbt)
			//e ho davanti a me il nome del dbt
			lex.SkipToken();//_NS_DBT
			string dbtName;
			if (!lex.ParseOpen() || !lex.ParseFlatString(out dbtName))
				return;
			//risalco i token parsati finché non trovo la parentesi di apertura del costruttore,
			//subito prima trovo il nome della classe
			//da tenere presente che il costruttore potrebbe trovarsi nel .h, quindi non posso apettarmi il costrutto <nome>::<nome>
			int upper = prevLexemes.Length - 2;
			for (int i = 0; i < upper; i++)
			{
				if (prevLexemes[i].Equals("CRuntimeClass") && prevLexemes[i + 1].Equals("("))
				{
					string dbtClass = prevLexemes[i + 2];
					cppInfo.SetDBTName(dbtClass, dbtName);
					return;
				}
			}
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseOnAttachData()
		{
			lex.SkipToken();//OnAttachData
			if (!lex.Parsed(Token.ROUNDOPEN) || !lex.Parsed(Token.ROUNDCLOSE) || !lex.Parsed(Token.BRACEOPEN))
				return;
			string className = prevLexemes[2];
			int openCount = 1;//ne ho aperta una
			while (!lex.Eof && openCount > 0)
			{

				if (lex.Parsed(Token.BRACEOPEN))
					openCount++;
				else if (lex.Parsed(Token.BRACECLOSE))
				{
					openCount--;
				}
				else if (lex.Parsed((Token)CPPToken.NEW))
				{
					ParseDBTCreation(className);
				}
				else
				{
					lex.SkipToken();
				}

			}
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseDBTCreation(string docClassName)
		{
			//new DBTJournalEntries(RUNTIME_CLASS(TJournalEntries), this);
			lex.SkipToken();//dbtName
			string dbtName = lex.CurrentLexeme;
			if (lex.Parsed(Token.ROUNDOPEN) && lex.Parsed((Token)CPPToken.RUNTIME_CLASS) && lex.Parsed(Token.ROUNDOPEN))
			{
				lex.SkipToken();//record class
				string sqlRecord = lex.CurrentLexeme;
				if (!lex.ParseClose())
					return;
				cppInfo.AddDBTToDocumentClass(docClassName, dbtName, sqlRecord);
			}
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseImplementMacro()
		{
			lex.SkipToken();
			if (!lex.ParseOpen())
				return;
			lex.SkipToken();
			string theClass = lex.CurrentLexeme;
			if (!lex.ParseComma())
				return;
			lex.SkipToken();
			string theBaseClass = lex.CurrentLexeme;
			if (!lex.ParseClose())
				return;
			if (theClass.Equals(theBaseClass))
			{
				lex.SetError("Class name equals base class: " + theBaseClass);
				return;
			}
			cppInfo.AddClass(theClass, theBaseClass);

			cppInfo.GetClass(theClass).ImplementationFile = lex.Filename;
		}

		//---------------------------------------------------------------------------------------------------
		private bool ParseControlSection()
		{
			TableStructure table = new TableStructure();
			//BEGIN_CONTROLS_TYPE(DataType::String)
			lex.SkipToken();//BEGIN_CONTROLS_TYPE
			if (!lex.ParseOpen() || !lex.Parsed((Token)CPPToken.DATATYPE) || !lex.ParseColon() | !lex.ParseColon())
				return false;
			string dataType = "", name = "";
			try
			{
				lex.SkipToken();
				dataType = lex.CurrentLexeme;
				if (!lex.ParseClose())
					return false;
				while (!lex.Eof && !lex.Parsed((Token)CPPToken.END_CONTROLS_TYPE))
				{
					lex.SkipToken();//comando
					string cmd = lex.CurrentLexeme;
					if (cmd.Equals("REGISTER_CONTROL"))//questa macro ha un argomento in più (per dirla come Bruna, è general generica :-))
					{
						//REGISTER_CONTROL(szMParsedEdit, MultilineStringEdit, _T("String Edit MultiLine"), CStrEdit, ES_MULTILINE | ES_WANTRETURN | WS_VSCROLL | ES_AUTOVSCROLL, FALSE)
						if (!lex.ParseOpen())
							return false;
						while (!lex.Parsed(Token.COMMA))
						{
							if (lex.Eof)
								return false;
							lex.SkipToken();//ID
						}

						lex.SkipToken();//nome del controllo
						name = lex.CurrentLexeme;

					}
					else if (!cmd.StartsWith("REGISTER_"))
					{
						return false;
					}
					else
					{
						//REGISTER_LISTBOX_CONTROL(IntListBox, _T("Integer Listbox"), CIntListBox, FALSE)
						if (!lex.ParseOpen())
							return false;
						lex.SkipToken();//nome del controllo
						name = lex.CurrentLexeme;
					}
					//adesso devo skippare tutto fino a chiusura parentesi tonda
					int openCount = 1;//ne ho aperrta una
					while (!lex.Eof && openCount > 0)
					{
						if (lex.Parsed(Token.ROUNDOPEN))
							openCount++;
						else if (lex.Parsed(Token.ROUNDCLOSE))
							openCount--;
						else
							lex.SkipToken();
					}
					if (openCount != 0)
						return false;
					List<ParsedControlsStructure> l;
					if (!controls.TryGetValue(dataType, out l))
					{
						l = new List<ParsedControlsStructure>();
						controls[dataType] = l;
					}
					l.Add(new ParsedControlsStructure() { Name = name });
					//salto eventuali ;
					while (lex.LookAhead(Token.SEP))
						lex.SkipToken();
				}
				return true;
			}
			catch
			{
				return false;
			}

		}
	}
}
