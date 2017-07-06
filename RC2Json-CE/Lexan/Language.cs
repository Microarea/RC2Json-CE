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

namespace RC2Json
{
	/// ================================================================================
	public class TkSymbolTable : Hashtable
	{
		public TkSymbolTable()
            :
            base(StringComparer.InvariantCultureIgnoreCase)
		{
		}
	}

    public class TkSymbolTable2 : TkSymbolTable
    {
        private TkSymbolTable reverseMap;

        public TkSymbolTable2(TkSymbolTable revMap)
            :
            base()
        {
            reverseMap = revMap;
        }

        public new virtual void Add(Object key, Object value)
        {
            base.       Add(key,    value);
            reverseMap. Add(value,  key);
        }
    }

	/// <summary>
	/// Summary description for Language.
	/// </summary>
	/// ================================================================================
	public class Language
	{
		public const string UnknownToken = "UnknowToken";

        static internal TkSymbolTable reverseMaps;

        static internal TkSymbolTable2 brackets;
		static internal TkSymbolTable2 operators;
		static internal TkSymbolTable2 keywords; 

		//------------------------------------------------------------------------------
		static Language()
		{
            reverseMaps = new TkSymbolTable();

			LoadBrackets();
			LoadOperators();
			LoadKeywords();
		}

		//------------------------------------------------------------------------------
		internal static Token GetTokenFromMap (TkSymbolTable map, string lexeme)
		{
			Object o = map[lexeme];
			return o == null ? Token.NOTOKEN : (Token)o;
		}

		//------------------------------------------------------------------------------
		public static string GetTokenString (Token token)
		{
            Object o = reverseMaps[token];
            return o == null ? UnknownToken : o.ToString();
 		}

		//------------------------------------------------------------------------------
		public static Token GetBracketsToken (string lexeme) { return GetTokenFromMap(brackets, lexeme); } 
		public static Token GetOperatorsToken(string lexeme) { return GetTokenFromMap(operators, lexeme); }
		public static Token GetKeywordsToken (string lexeme) { return GetTokenFromMap(keywords, lexeme); } 

		//------------------------------------------------------------------------------
		public static bool	ExistToken (string lexeme) 
		{
			return
				Language.GetKeywordsToken(lexeme) != Token.NOTOKEN	||
				Language.GetOperatorsToken(lexeme) != Token.NOTOKEN;
		} 

		//------------------------------------------------------------------------------
		static internal void LoadBrackets ()
		{
            brackets = new TkSymbolTable2(reverseMaps);

			brackets.Add("(",Token.ROUNDOPEN);
			brackets.Add(")",Token.ROUNDCLOSE);
			brackets.Add("[",Token.SQUAREOPEN);
			brackets.Add("]",Token.SQUARECLOSE);
			brackets.Add("{",Token.BRACEOPEN);
			brackets.Add("}",Token.BRACECLOSE);
			brackets.Add(",",Token.COMMA);
			brackets.Add(":",Token.COLON);
		}

		//------------------------------------------------------------------------------
		static internal void LoadOperators ()
		{
            operators = new TkSymbolTable2(reverseMaps);

			operators.Add("=",Token.ASSIGN);
			operators.Add("?",Token.QUESTION_MARK);
			operators.Add("+",Token.PLUS);  
			operators.Add("-",Token.MINUS);           
			operators.Add("*",Token.STAR);          
			operators.Add("/",Token.SLASH);          
			operators.Add("^^",Token.EXP);          
			operators.Add("%",Token.PERC);
			operators.Add("<",Token.LT);           
			operators.Add(">",Token.GT);             
			operators.Add("<=",Token.LE);             
			operators.Add(">=",Token.GE);             
			operators.Add("==",Token.EQ);             
			operators.Add("!=",Token.NE);             
			operators.Add("<>",Token.DIFF);
			operators.Add("++",Token.INC);           
			operators.Add("--",Token.DEC);            
			operators.Add("+=",Token.INCASS);
			operators.Add("-=",Token.DECASS);         
			operators.Add("!",Token.OP_NOT);         
			operators.Add("&&",Token.OP_AND);         
			operators.Add("||",Token.OP_OR);         
			operators.Add("~",Token.BW_NOT);
			operators.Add("&",Token.BW_AND);
			operators.Add("|",Token.BW_OR);
			operators.Add("^",Token.BW_XOR);

			// dummy operator utili solo nella valutazione di espressioni 
			// compilate dalla classe Expression
			operators.Add("expr1",Token.EXPR_UNNARY_MINUS);
			operators.Add("expr2",Token.EXPR_ESCAPED_LIKE);
			operators.Add("expr3",Token.EXPR_IS_NULL);
			operators.Add("expr4",Token.EXPR_IS_NOT_NULL);
			operators.Add("expr5",Token.EXPR_NULL);
		}

		//------------------------------------------------------------------------------
		static internal void LoadKeywords ()
		{
            keywords = new TkSymbolTable2(reverseMaps);
			
			keywords.Add("Abort"				,Token.ABORT);
			keywords.Add("Abs"					,Token.ABS);
			keywords.Add("After"				,Token.AFTER);
			keywords.Add("Alias"				,Token.ALIAS);
			keywords.Add("Aliases"				,Token.ALIASES);
			keywords.Add("Align"				,Token.ALIGN);
			keywords.Add("All"					,Token.ALL);
			keywords.Add("Always"				,Token.ALWAYS);
			
			keywords.Add("AnchorPageLeft"		,Token.ANCHOR_PAGE_LEFT);
			keywords.Add("AnchorPageRight"		,Token.ANCHOR_PAGE_RIGHT);
			keywords.Add("AnchorColumn"			,Token.ANCHOR_COLUMN_ID);
			keywords.Add("AnchorLeft"			,Token.COLUMN_ANCHOR_LEFT);
			keywords.Add("AnchorRight"			,Token.COLUMN_ANCHOR_RIGHT);
			
			keywords.Add("And"					,Token.AND);
			keywords.Add("Appdate"				,Token.APPDATE);
			keywords.Add("Appyear"				,Token.APPYEAR);
			keywords.Add("Append"				,Token.APPEND);
			keywords.Add("Archive"				,Token.ARCHIVE);

			keywords.Add("Array"				,Token.ARRAY);
			keywords.Add("Array_Attach"			,Token.ARRAY_ATTACH);
			keywords.Add("Array_Clear"			,Token.ARRAY_CLEAR);
            keywords.Add("Array_Copy"           ,Token.ARRAY_COPY);
            keywords.Add("Array_Detach"         ,Token.ARRAY_DETACH);
            keywords.Add("Array_Find"           ,Token.ARRAY_FIND);
			keywords.Add("Array_GetAt"			,Token.ARRAY_GETAT);
			keywords.Add("Array_Size"			,Token.ARRAY_SIZE);
			keywords.Add("Array_SetAt"			,Token.ARRAY_SETAT);
			keywords.Add("Array_Sort"			,Token.ARRAY_SORT);

            keywords.Add("Array_Add"            ,Token.ARRAY_ADD);
            keywords.Add("Array_Append"         ,Token.ARRAY_APPEND);
            keywords.Add("Array_Insert"         ,Token.ARRAY_INSERT);
            keywords.Add("Array_Remove"         ,Token.ARRAY_REMOVE);
            keywords.Add("Array_Contains"       ,Token.ARRAY_CONTAINS);
            keywords.Add("Array_Create"         ,Token.ARRAY_CREATE);
            keywords.Add("Array_Sum"            ,Token.ARRAY_SUM);

			keywords.Add("As"					,Token.AS);
			keywords.Add("Asc"					,Token.ASC);
			keywords.Add("Ascending"			,Token.ASCENDING);
			keywords.Add("Ask"					,Token.ASK);
			keywords.Add("AutoIncremental"		,Token.AUTOINCREMENTAL);
			keywords.Add("Author"				,Token.AUTHOR);
			keywords.Add("BarcodeStrip"			,Token.BARCODE);
			keywords.Add("Before"				,Token.BEFORE);
			keywords.Add("Begin"				,Token.BEGIN);
			keywords.Add("Between"				,Token.BETWEEN);
			keywords.Add("Bitmap"				,Token.BITMAP);
			keywords.Add("Metafile"				,Token.METAFILE);
			keywords.Add("Proportional"			,Token.PROPORTIONAL);
			keywords.Add("Bkgcolor"				,Token.BKGCOLOR);
			keywords.Add("Blob"					,Token.BLOB);
			keywords.Add("Body"					,Token.BODY);
			keywords.Add("Bold"					,Token.BOLD);
			keywords.Add("Bool"					,Token.BOOLEAN);
			keywords.Add("Borders"				,Token.BORDERS);
			keywords.Add("Bottom"				,Token.BOTTOM);
			keywords.Add("Break"				,Token.BREAK);
			keywords.Add("Breaking"				,Token.BREAKING);
			keywords.Add("Build_ids"			,Token.BUILD_IDS);
			keywords.Add("By"					,Token.BY);
			keywords.Add("Call"					,Token.CALL);
			keywords.Add("Calldll"				,Token.CALL_DLL);
			keywords.Add("Case"					,Token.CASE);
			keywords.Add("Cdow"					,Token.CDOW);
			keywords.Add("Ccat"					,Token.CCAT);
			keywords.Add("Ceiling"				,Token.CEILING);
			keywords.Add("Cell"					,Token.CELL);
			keywords.Add("Cfg"					,Token.CFG);
            keywords.Add("Chart"                ,Token.CHART);
            keywords.Add("Check"                ,Token.CHECK);
			keywords.Add("Chr"					,Token.CHR);
            keywords.Add("Close"                ,Token.CLOSE);
			keywords.Add("Cmax"					,Token.CMAX);
			keywords.Add("Cmd"					,Token.CMD);
			keywords.Add("Cmin"					,Token.CMIN);
			keywords.Add("Cmonth"				,Token.CMONTH);
			keywords.Add("Col"					,Token.COL);
			keywords.Add("Color"				,Token.COLOR);
			keywords.Add("ColTotal"				,Token.COLTOTAL);
			keywords.Add("Column"				,Token.COLUMN);
			keywords.Add("ColumnPen"			,Token.COLUMN_PEN);
			keywords.Add("Columns"				,Token.COLUMNS);
			keywords.Add("ColumnTitles"			,Token.COLUMN_TITLES);
            keywords.Add("ColTitleBottom"       ,Token.COLTITLE_BOTTOM);
            keywords.Add("Comments"             ,Token.COMMENTS);
			keywords.Add("//==============================================================================", Token.COMMENT_SEP);		
			keywords.Add("Conditional"			,Token.CONDITIONAL);
			keywords.Add("Const"				,Token.CONST);
			keywords.Add("Contains"				,Token.CONTAINS);
			keywords.Add("Continue"				,Token.CONTINUE);
			keywords.Add("Controls"				,Token.CONTROLS);
			keywords.Add("ContentOf"			,Token.CONTENTOF);
            keywords.Add("// Woorm code behind", Token.COPYRIGHT);
            keywords.Add("Create"               ,Token.CREATE);
			keywords.Add("CreateSchema"			,Token.CREATE_SCHEMA);
			keywords.Add("Csum"					,Token.CSUM);
			keywords.Add("Ctod"					,Token.CTOD);
			keywords.Add("Ctype"				,Token.CTYPE);
			keywords.Add("Cupdate"				,Token.CUPDATE);
			keywords.Add("Datasource"			,Token.DATASOURCE);
			keywords.Add("Date"					,Token.DATE);
			keywords.Add("Datetime"				,Token.DATETIME);
			keywords.Add("Day"					,Token.DAY);
			keywords.Add("DayOfWeek"			,Token.DAYOFWEEK);
			keywords.Add("DayOfYear"			,Token.DAYOFYEAR);
			keywords.Add("Dde"					,Token.DDE);
            keywords.Add("Debug"                ,Token.DEBUG);
            keywords.Add("Decode"               ,Token.DECODE);
            keywords.Add("Define"               ,Token.DEFINE);
			keywords.Add("Default"				,Token.DEFAULT);
			keywords.Add("DefaultSecurityRoles"	,Token.DEFAULTSECURITYROLES);
			keywords.Add("Delete"				,Token.DELETE);
			keywords.Add("Desc"					,Token.DESCENDING);
			keywords.Add("Development"			,Token.DEVELOPMENT);
			keywords.Add("Dialog"				,Token.DIALOG);
			keywords.Add("Dialogs"				,Token.DIALOGS);
			keywords.Add("Dynamic"				,Token.DYNAMIC);
			keywords.Add("Display"				,Token.DISPLAY);
			keywords.Add("DisplayFreeFields"	,Token.DISPLAY_FREE_FIELDS);
			keywords.Add("DisplayTableRow"		,Token.DISPLAY_TABLE_ROW);
			keywords.Add("Distinct"				,Token.DISTINCT);
            keywords.Add("Do"                   ,Token.DO);
            keywords.Add("DropShadow"           ,Token.DROPSHADOW);
			keywords.Add("Double"				,Token.DOUBLE_PRECISION);
			keywords.Add("Dtoc"					,Token.DTOC);
			keywords.Add("ElapsedTime"			,Token.ELAPSED_TIME);
			keywords.Add("Else"					,Token.ELSE);
			keywords.Add("End"					,Token.END);
			keywords.Add("Enum"					,Token.ENUM);
			keywords.Add("Eof"					,Token.EOF);
			keywords.Add("Escape"				,Token.ESCAPE);
			keywords.Add("Eval"					,Token.EVAL);
			keywords.Add("Events"				,Token.EVENTS);
			keywords.Add("Exec"					,Token.EXEC);
			keywords.Add("Expand"				,Token.EXPAND);
			keywords.Add("Export"				,Token.EXPORT);
			keywords.Add("Extension"			,Token.EXTENSION);
			keywords.Add("Facename"				,Token.FACENAME);
			keywords.Add("False"				,Token.FALSE);
			keywords.Add("Field"				,Token.FIELD);
			keywords.Add("File"					,Token.FILE);
            keywords.Add("FileExists"           ,Token.FILEEXISTS);
			keywords.Add("Finalize"				,Token.FINALIZE);
			keywords.Add("Find"					,Token.FIND);
			keywords.Add("Fint"					,Token.FINT);
			keywords.Add("Fixed"				,Token.COLUMN_FIXED);
			keywords.Add("FiscalEnd"			,Token.FISCAL_END);
			keywords.Add("Float"				,Token.SINGLE_PRECISION);
			keywords.Add("Flong"				,Token.FLONG);
			keywords.Add("Floor"				,Token.FLOOR);
			keywords.Add("FontStyle"			,Token.FONTSTYLE);
			keywords.Add("FontStyles"			,Token.FONTSTYLES);
			keywords.Add("For"					,Token.FOR);
            keywords.Add("Force"                ,Token.FORCE);
            keywords.Add("Format"               ,Token.FORMAT);
			keywords.Add("FormatStyle"			,Token.FORMATSTYLE);
			keywords.Add("FormatStyles"			,Token.FORMATSTYLES);
			keywords.Add("FormFeed"				,Token.FORMFEED);

			keywords.Add("Framework.TbWoormViewer.TbWoormViewer.GetOwnerNamespace"			,Token.GETOWNERNAMESPACE);
			keywords.Add("Framework.TbWoormViewer.TbWoormViewer.GetReportModuleNamespace"	,Token.GETREPORTMODULENAMESPACE);
			keywords.Add("Framework.TbWoormViewer.TbWoormViewer.GetReportNamespace"			,Token.GETREPORTNAMESPACE);
			keywords.Add("Framework.TbWoormViewer.TbWoormViewer.GetReportPath"				,Token.GETREPORTPATH);
            keywords.Add("Framework.TbWoormViewer.TbWoormViewer.GetReportName"              ,Token.GETREPORTNAME);
            keywords.Add("Framework.TbWoormViewer.TbWoormViewer.IsAutoPrint"                ,Token.ISAUTOPRINT);

            keywords.Add("From"                 ,Token.FROM);
            keywords.Add("Full"                 ,Token.FULL);
            keywords.Add("FuncPrototypes"       ,Token.FUNCPROTOTYPES);

			keywords.Add("GetApplicationTitleFromNs" ,Token.GETAPPTITLE);
            keywords.Add("GetBarCodeID"         , Token.GETBARCODE_ID);
			keywords.Add("GetCompanyName"		,Token.GETCOMPANYNAME);
			keywords.Add("GetComputerName"		,Token.GETCOMPUTERNAME);
			keywords.Add("GetCulture"			,Token.GETCULTURE);
			keywords.Add("GetDocumentTitleFromNs", Token.GETDOCTITLE);
			keywords.Add("GetDatabaseType"		,Token.GETDATABASETYPE);
			keywords.Add("GetEdition"			,Token.GETEDITION);
			keywords.Add("GetInstallationName"	,Token.GETINSTALLATIONNAME);
            keywords.Add("GetInstallationPath"  ,Token.GETINSTALLATIONPATH);
			keywords.Add("GetInstallationVersion" ,Token.GETINSTALLATIONVERSION);
			keywords.Add("GetLoginName"			, Token.GETLOGINNAME);
			keywords.Add("GetModuleTitleFromNs"	,Token.GETMODTITLE);
            keywords.Add("GetNewGuid"           ,Token.GETNEWGUID);
            keywords.Add("GetNsFromPath"        ,Token.GETNSFROMPATH);
            keywords.Add("GetPathFromNs"        ,Token.GETPATHFROMNS);
			keywords.Add("GetProductLanguage"	,Token.GETPRODUCTLANGUAGE);
			keywords.Add("GetSetting"			,Token.GETSETTING);
			keywords.Add("GetTitle"	            ,Token.GETTITLE);
            keywords.Add("GetThreadContextVar"  ,Token.GETTHREADCONTEXT);
            keywords.Add("OwnThreadContextVar"  ,Token.OWNTHREADCONTEXT);
            keywords.Add("GetUpperLimit"        ,Token.GETUPPERLIMIT);
            keywords.Add("GetUserDescription"   ,Token.GETUSERDESCRIPTION);
            keywords.Add("GetWindowUser"        ,Token.GETWINDOWUSER);
			keywords.Add("GiulianDate"			,Token.GIULIANDATE);
			keywords.Add("Goto"					,Token.GOTO);
			keywords.Add("Group"				,Token.GROUP);
			keywords.Add("Uuid"					,Token.UUID);
			keywords.Add("Having"				,Token.HAVING);
			keywords.Add("Heights"				,Token.HEIGHTS);
			keywords.Add("Help"					,Token.HELP);
			keywords.Add("Hidden"				,Token.HIDDEN);
			keywords.Add("HideLS0"				,Token.HIDE_LS0);
			keywords.Add("HideMS0"				,Token.HIDE_MS0);
			keywords.Add("HideWhenEmpty"		,Token.COLUMN_HIDE_WHEN_EMPTY);
			keywords.Add("HotLink"				,Token.HOTLINK);
			keywords.Add("HPageSplitter"		,Token.PAGE_HSPLITTER);
			keywords.Add("HtmlFile"				,Token.HTMLFILE);
			keywords.Add("If"					,Token.IF);
			keywords.Add("In"					,Token.IN);
			keywords.Add("Inch"					,Token.INCH);
			keywords.Add("Include"				,Token.INCLUDE);
			keywords.Add("Index"				,Token.INDEX);
			keywords.Add("Init"					,Token.INIT);
			keywords.Add("Input"				,Token.INPUT);
			keywords.Add("Insert"				,Token.INSERT);
			keywords.Add("Integer"				,Token.INTEGER);
			keywords.Add("InterLine"			,Token.INTERLINE);
			keywords.Add("Into"					,Token.INTO);
			keywords.Add("Invalid"				,Token.INVALID);
			keywords.Add("Is"					,Token.IS);
			keywords.Add("IsActivated"			,Token.ISACTIVATED);
            keywords.Add("IsAdmin"              ,Token.ISADMIN);
            keywords.Add("IsDatabaseUnicode"    ,Token.ISDATABASEUNICODE);
            keywords.Add("IsEmpty"              ,Token.ISEMPTY);
            keywords.Add("IsNull"               ,Token.ISNULL);
            keywords.Add("IsRemoteInterface"    ,Token.ISREMOTEINTERFACE);
            keywords.Add("IsRunningFromExternalController", Token.ISRUNNINGFROMEXTERNALCONTROLLER);
            keywords.Add("IsWeb"                ,Token.ISWEB);
            keywords.Add("Italic"               ,Token.ITALIC);
			keywords.Add("Key"					,Token.KEY);
			keywords.Add("Join"					,Token.JOIN);
			keywords.Add("Label"				,Token.LABEL);
			keywords.Add("Landscape"			,Token.LANDSCAPE);
			keywords.Add("LastMonthDay"			,Token.LAST_MONTH_DAY);
			keywords.Add("Left"					,Token.LEFT);
			keywords.Add("Len"					,Token.LEN);
			keywords.Add("Like"					,Token.LIKE);
			keywords.Add("Links"				,Token.LINKS);
			keywords.Add("LinkForm"				,Token.LINKFORM);
			keywords.Add("LinkFunction"			,Token.LINKFUNCTION);
			keywords.Add("LinkRadar"			,Token.LINKRADAR);
			keywords.Add("LinkReport"			,Token.LINKREPORT);
			keywords.Add("LinkUrl"				,Token.LINKURL);
			keywords.Add("LoadText"				,Token.LOADTEXT);
			keywords.Add("Localize"				,Token.LOCALIZE);
			keywords.Add("Logic"				,Token.LOGIC);
			keywords.Add("Long"					,Token.LONG_INTEGER);
			keywords.Add("LongString"			,Token.LONG_STRING);
			keywords.Add("Lower"				,Token.LOWER);
			keywords.Add("LowerLimit"			,Token.LOWER_LIMIT);
			keywords.Add("Ltrim"				,Token.LTRIM);
			keywords.Add("Mail"					,Token.MAIL);
			keywords.Add("Margins"				,Token.MARGINS);
			keywords.Add("Max"					,Token.MAX);
			keywords.Add("Maximized"			,Token.MAXIMIZED);
			keywords.Add("Menu"					,Token.MENU);
			keywords.Add("Menuitem"				,Token.MENUITEM);
			keywords.Add("Message"				,Token.MESSAGE);
			keywords.Add("Min"					,Token.MIN);
			keywords.Add("Minimized"			,Token.MINIMIZED);
			keywords.Add("Mod"					,Token.MOD);
			keywords.Add("Money"				,Token.MONEY);
			keywords.Add("Month"				,Token.MONTH);
			keywords.Add("MonthDays"			,Token.MONTH_DAYS);
			keywords.Add("MonthName"			,Token.MONTH_NAME);
            keywords.Add("MultiSelections"      ,Token.MULTI_SELECTIONS);
            keywords.Add("HideTitle"            ,Token.HIDE_TABLE_TITLE);
			keywords.Add("HideAllTitles"		,Token.HIDE_ALL_TABLE_TITLE);
			keywords.Add("EasyView"				,Token.EASYVIEW);
			keywords.Add("MakeLowerLimit"		,Token.MAKELOWERLIMIT);
			keywords.Add("MakeUpperLimit"		,Token.MAKEUPPERLIMIT);
			keywords.Add("Native"				,Token.NATIVE);
			keywords.Add("NextLine"				,Token.NEXTLINE);
			keywords.Add("NoBodyBottom"			,Token.NO_BOB);
			keywords.Add("NoBodyLeft"			,Token.NO_BOL);
			keywords.Add("NoBodyRight"			,Token.NO_BOR);
			keywords.Add("NoBodyTop"			,Token.NO_BOT);
			keywords.Add("NoBorders"			,Token.NO_BORDERS);
			keywords.Add("NoColsep"				,Token.NO_CSE);
			keywords.Add("NoColTitleLeft"		,Token.NO_CTL);
			keywords.Add("NoColTitleRight"		,Token.NO_CTR);
			keywords.Add("NoColTitleSep"		,Token.NO_CTS);
			keywords.Add("NoColTitleTop"		,Token.NO_CTT);
			keywords.Add("Nodup"				,Token.NODUPKEY);
			keywords.Add("NoHRuler"				,Token.NO_HRULER);
			keywords.Add("NoInterface"			,Token.NO_INTERFACE);
			keywords.Add("NoPrinterBkgnBitmap"	,Token.NO_PRN_BKGN_BITMAP);
			keywords.Add("NoPrinterBorders"		,Token.NO_PRN_BORDERS);
			keywords.Add("NoPrinterLabels"		,Token.NO_PRN_LABELS);
			keywords.Add("NoPrinterTitles"		,Token.NO_PRN_TITLES);
			keywords.Add("NoStatusBar"			,Token.NO_STATUSBAR);
			keywords.Add("Not"					,Token.NOT);
			keywords.Add("NoIconbar"			,Token.NO_TOOLBAR);
			keywords.Add("NoTitleLeft"			,Token.NO_TTL);
			keywords.Add("NoTitleRight"			,Token.NO_TTR);
			keywords.Add("NoTitleTop"			,Token.NO_TTT);
			keywords.Add("NoToken"				,Token.NOTOKEN);
			keywords.Add("NoTotalBottom"		,Token.NO_TOB);
			keywords.Add("NoTotalLeft"			,Token.NO_TOL);
			keywords.Add("NoTotalRight"			,Token.NO_TOR);
			keywords.Add("NoVideoBkgnBitmap"	,Token.NO_CON_BKGN_BITMAP);
			keywords.Add("NoVideoBorders"		,Token.NO_CON_BORDERS);
			keywords.Add("NoVideoLabels"		,Token.NO_CON_LABELS);
			keywords.Add("NoVideoTitles"		,Token.NO_CON_TITLES);
			keywords.Add("NoVRuler"				,Token.NO_VRULER);
			keywords.Add("NoWeb"				,Token.NO_WEB);
            keywords.Add("NoXml"                ,Token.NO_XML);
            keywords.Add("NULL"                 ,Token.NULL);
			keywords.Add("Numeric"				,Token.NUMERIC);
			keywords.Add("Objects"				,Token.OBJECTS);
			keywords.Add("Of"					,Token.OF);
			keywords.Add("On"					,Token.ON);
			keywords.Add("OnlyGraphInfo"		,Token.ONLY_GRAPH);
			keywords.Add("Open"					,Token.OPEN);
			keywords.Add("Optional"				,Token.OPTIONAL);
			keywords.Add("OrderFindField"		,Token.ORDER_FIND_FIELD);
			keywords.Add("OptimizeWidth"		,Token.COLUMN_OPTIMIZE_WIDTH);
			keywords.Add("Options"				,Token.OPTIONS);
			keywords.Add("Or"					,Token.OR);
			keywords.Add("Order"				,Token.ORDER);
			keywords.Add("Origin"				,Token.ORIGIN);
			keywords.Add("Out"					,Token.OUT);
            keywords.Add("Outer"                ,Token.OUTER);
            //keywords.Add("OwnerId"              ,Token.OWNER_ID);
			keywords.Add("Padded"				,Token.PADDED);
			keywords.Add("PageInfo"				,Token.PAGE_INFO);
			keywords.Add("PageLayout"			,Token.PAGE_LAYOUT);
			keywords.Add("Path"					,Token.PATH);
			keywords.Add("Pen"					,Token.PEN);
			keywords.Add("Percent"				,Token.PERCENT);
			keywords.Add("Popup"				,Token.POPUP);
			keywords.Add("Postfix"				,Token.POSTFIX);
			keywords.Add("Precision"			,Token.PRECISION);
			keywords.Add("Prefix"				,Token.PREFIX);
			keywords.Add("PrinterPageInfo"		,Token.PAGE_PRINTER_INFO);
			keywords.Add("Procedure"			,Token.PROCEDURE);
			keywords.Add("Procedures"			,Token.PROCEDURES);
			keywords.Add("Prompt"				,Token.PROMPT);
			keywords.Add("Properties"			,Token.PROPERTIES);
			keywords.Add("Quantity"				,Token.QUANTITY);
			keywords.Add("Query"				,Token.QUERY);
			keywords.Add("Queries"				,Token.QUERIES);
            keywords.Add("Quit"                 ,Token.QUIT);
            keywords.Add("Rand"                 ,Token.RAND);
			keywords.Add("Ratio"				,Token.RATIO);
			keywords.Add("ReadOnly"				,Token.READ_ONLY);
			keywords.Add("Rect"					,Token.RECT);
			keywords.Add("Ref"					,Token.REF);
			keywords.Add("Reinit"				,Token.REINIT);
			keywords.Add("Release"				,Token.RELEASE);
            //keywords.Add("Rem"                  ,Token.REM);
            keywords.Add("RemoveNewLine"        ,Token.REMOVENEWLINE);
			keywords.Add("Replace"				,Token.REPLACE);
			keywords.Add("Report"				,Token.REPORT);
			keywords.Add("ReportProducer"		,Token.REPORTPRODUCER);
			keywords.Add("Reports"				,Token.REPORTS);
			keywords.Add("Reset"				,Token.RESET);
			keywords.Add("ReverseFind"			,Token.REVERSEFIND);
			keywords.Add("Rgb"					,Token.RGB);
			keywords.Add("Right"				,Token.RIGHT);
			keywords.Add("Rndrect"				,Token.RNDRECT);
			keywords.Add("Round"				,Token.ROUND);
			keywords.Add("Row"					,Token.ROW);
			keywords.Add("RowSep"				,Token.YE_RSE);
			keywords.Add("RTrim"				,Token.RTRIM);
			keywords.Add("Rules"				,Token.RULES);
			keywords.Add("RunReport"			,Token.RUNREPORT);
			keywords.Add("Return"				,Token.RETURN);
            keywords.Add("Repeater"             ,Token.REPEATER);
			keywords.Add("Save"					,Token.SAVE);
            keywords.Add("SaveText"             ,Token.SAVETEXT);
            keywords.Add("Segmented"            ,Token.SEGMENTED);
			keywords.Add("Select"				,Token.SELECT);
			keywords.Add(";"					, Token.SEP);
			keywords.Add("Separator"			,Token.SEPARATOR);
			keywords.Add("Set"					,Token.SET);
            keywords.Add("SetCulture"           ,Token.SETCULTURE);
            keywords.Add("SetSetting"           ,Token.SETSETTING);
            keywords.Add("Sign"                 ,Token.SIGN);
			keywords.Add("Size"					,Token.SIZE);
			keywords.Add("Sizeof"				,Token.SIZEOF);
			keywords.Add("Space"				,Token.SPACE);
			keywords.Add("SpaceLine"			,Token.SPACELINE);
			keywords.Add("Spawn"				,Token.SPAWN);
			keywords.Add("Special"				,Token.SPECIAL_FIELD);
			keywords.Add("Splitter"				,Token.COLUMN_SPLITTER);
			keywords.Add("SqlExec"				,Token.SQL_EXEC);
			keywords.Add("SqrRect"				,Token.SQRRECT);
			keywords.Add("Static"				,Token.STATIC);
			keywords.Add("Shortcut"				,Token.CMDSHORTCUT);
			keywords.Add("Subject"				,Token.SUBJECT);
			keywords.Add("Status"				,Token.STATUS);
			keywords.Add("Str"					,Token.STR);
			keywords.Add("String"				,Token.STRING);
			keywords.Add("StringTable"			,Token.STRINGTABLE);
			keywords.Add("Strikeout"			,Token.STRIKEOUT);
			keywords.Add("Struct"				,Token.STRUCT);
			keywords.Add("Style"				,Token.STYLE);
			keywords.Add("Substr"				,Token.SUBSTR);
			keywords.Add("SubstrWW"				,Token.SUBSTRWW);
			keywords.Add("SubTotal"				,Token.SUBTOTAL);
			keywords.Add("SubTotals"			,Token.SUBTOTALS);
			keywords.Add("Switch"				,Token.SWITCH);
			keywords.Add("\t"					,Token.TAB);
			keywords.Add("Table"				,Token.TABLE);
			keywords.Add("TableExists"			,Token.TABLEEXISTS);
			keywords.Add("Tables"				,Token.TABLES);
            keywords.Add("ReportTemplate"       ,Token.TEMPLATE);
            keywords.Add("Text"                 ,Token.TEXT);
			keywords.Add("TextColor"			,Token.TEXTCOLOR);
			keywords.Add("Thousand"				,Token.THOUSAND);
			keywords.Add("Time"					,Token.TIME);
			keywords.Add("Title"				,Token.TITLE);
			keywords.Add("TitleBottom"			,Token.TITLE_BOTTOM);
			keywords.Add("Then"					,Token.THEN);
			keywords.Add("Top"					,Token.TOP);
			keywords.Add("Total"				,Token.TOTAL);
            keywords.Add("TotalTop"             ,Token.TOTAL_TOP);
			keywords.Add("Totals"				,Token.TOTALS);
            keywords.Add("Tooltip"              ,Token.TOOLTIP);
            keywords.Add("Transparent"          ,Token.TRANSPARENT);
			keywords.Add("Trim"					,Token.TRIM);
			keywords.Add("True"					,Token.TRUE);
			keywords.Add("Type"					,Token.TYPE);
			keywords.Add("TypedBarCode"			,Token.TYPED_BARCODE);
			keywords.Add("Typedef"				,Token.TYPEDEF);
			keywords.Add("Undef"				,Token.UNDEF);
			keywords.Add("Underline"			,Token.UNDERLINE);
			keywords.Add("Update"				,Token.UPDATE);
			keywords.Add("Upper"				,Token.UPPER);
			keywords.Add("UpperLimit"			,Token.UPPER_LIMIT);
			keywords.Add("DraftFont"			,Token.USE_DRAFT_FONT);
			keywords.Add("Val"					,Token.VAL);
			keywords.Add("ValueOf"				,Token.VALUEOF);
			keywords.Add("Var"					,Token.TVAR);
			keywords.Add("Variables"			,Token.VAR);
            keywords.Add("VMergeEmptyCell"      ,Token.VMERGE_EMPTY_CELL);
            keywords.Add("VMergeEqualCell"      ,Token.VMERGE_EQUAL_CELL);
            keywords.Add("VMergeTailCell"       ,Token.VMERGE_TAIL_CELL);
            keywords.Add("VPageSplitter"		,Token.PAGE_VSPLITTER);
			keywords.Add("Weekday"				,Token.WEEKDAY);
			keywords.Add("WeekOfMonth"			,Token.WEEKOFMONTH);
			keywords.Add("WeekOfYear"			,Token.WEEKOFYEAR);
			keywords.Add("When"					,Token.WHEN);
			keywords.Add("Where"				,Token.WHERE);
			keywords.Add("While"				,Token.WHILE);
			keywords.Add("Width"				,Token.WIDTH);
            keywords.Add("WildcardMatch"        ,Token.WILDCARD_MATCH);

            keywords.Add("Year"                 , Token.YEAR);

            keywords.Add("DateAdd"              , Token.DateAdd);
            keywords.Add("WeekStartDate"        , Token.WeekStartDate);
            keywords.Add("IsLeapYear"           , Token.IsLeapYear);
            keywords.Add("EasterSunday"         , Token.EasterSunday);

            keywords.Add("SendBalloon"          , Token.SendBalloon);
            keywords.Add("FormatTbLink"         , Token.FormatTbLink);

            //new tokes post 3.0 - THEY ARE NOT PUBLIC
            keywords.Add("Convert"              ,Token.CONVERT);
            keywords.Add("TypeOf"               ,Token.TYPEOF);
            keywords.Add("AddressOf"            ,Token.ADDRESSOF);
            keywords.Add("ExecuteScript"        ,Token.EXECUTESCRIPT);
        }
	}

	//------------------------------------------------------------------------------
	public enum Token
	{
		SEP,
		ID,
		CMT,
		TEXTSTRING,

		// -------------------------------------------------  tipi base start
		SBYTE,
		BYTE,
		SHORT,
		USHORT,
		INT,
		UINT,
		LONG,
		ULONG,
		BOOL,
		FLOAT,
		DOUBLE,
		// -------------------------------------------------  tipi base end

		// -------------------------------------------------  operators start
		ASSIGN,
		QUESTION_MARK,
		PLUS,
		MINUS,
		STAR,
		SLASH,
		EXP,
		PERC,
		LT,
		GT,
		LE,
		GE,
		EQ,
		NE,
		DIFF,
		INC,
		DEC,
		INCASS,
		DECASS,
		OP_NOT,
		OP_AND,
		OP_OR,
		BW_NOT,
		BW_AND,
		BW_OR,
		BW_XOR,
		EXPR_UNNARY_MINUS,
		EXPR_ESCAPED_LIKE,
		EXPR_IS_NULL,
		EXPR_IS_NOT_NULL,
		EXPR_NULL,
		// -------------------------------------------------  operators end

		// -------------------------------------------------  brackets start
		ROUNDOPEN,
		ROUNDCLOSE,
		SQUAREOPEN,
		SQUARECLOSE,
		BRACEOPEN,
		BRACECLOSE,
		COMMA,
		COLON,
		// -------------------------------------------------  brackets end

		// -------------------------------------------------  keywords start
		ABORT,
		ABS,
		AFTER,
		ALIAS,
		ALIASES,
		ALIGN,
		ALL,
		ALWAYS,

		ANCHOR_PAGE_LEFT,
		ANCHOR_PAGE_RIGHT,
		ANCHOR_COLUMN_ID,

		AND,
		APPDATE,
		APPYEAR,
		APPEND,
		ARCHIVE,
		ARRAY,

		ARRAY_ATTACH,
		ARRAY_CLEAR,
		ARRAY_COPY,
		ARRAY_DETACH,
		ARRAY_FIND,
		ARRAY_GETAT,
		ARRAY_SIZE,
		ARRAY_SETAT,
		ARRAY_SORT,

		ARRAY_ADD,
		ARRAY_APPEND,
		ARRAY_INSERT,
		ARRAY_REMOVE,
        ARRAY_CONTAINS,
        ARRAY_CREATE,
        ARRAY_SUM,

		AS,
		ASC,
		ASCENDING,
		ASK,
		AUTHOR,
		AUTOINCREMENTAL,

		BARCODE,
		BEFORE,
		BEGIN,
		BETWEEN,
        BITMAP,
        BKGCOLOR,
		BLOB,
		BODY,
		BOLD,
		BOOLEAN,
		BORDERS,
		BOTTOM,
		BREAK,
		BREAKING,
		BUILD_IDS,
		BY,

		CALL,
		CALL_DLL,
		CASE,
		CDOW,
		CCAT,
		CEILING,
		CELL,
		CFG,
        CHART,
		CHECK,
		CHR,
		CLOSE,
		CMAX,
		CMD,
		CMDSHORTCUT,
		CMIN,
		CMONTH,
		COL,
		COLOR,
		COLTITLE_BOTTOM,
		COLTOTAL,
		COLUMN,
		COLUMN_PEN,
		COLUMNS,
		COLUMN_TITLES,

		COLUMN_ANCHOR_LEFT,
		COLUMN_ANCHOR_RIGHT,
		COLUMN_FIXED,
		COLUMN_HIDE_WHEN_EMPTY,
		COLUMN_OPTIMIZE_WIDTH,
		COLUMN_SPLITTER,

		COMMENTS,
		COMMENT_SEP,
		CONDITIONAL,
		CONST,
		CONTAINS,
		CONTENTOF,
		CONTINUE,
		CONTROLS,
		COPYRIGHT,
		CREATE,
		CREATE_SCHEMA,
		CSUM,
		CTOD,
		CTYPE,
		CUPDATE,

        DECODE,
		DATASOURCE,
		DATE,
		DATETIME,
		DAY,
		DAYOFWEEK,
		DAYOFYEAR,
		DDE,
		DEBUG,
		DEFINE,
		DEFAULT,
		DEFAULTSECURITYROLES,
		DELETE,
		DESCENDING,
		DEVELOPMENT,
		DIALOG,
		DIALOGS,
		DYNAMIC,
		DISPLAY,
		DISPLAY_FREE_FIELDS,
		DISPLAY_TABLE_ROW,
		DISTINCT,
		DROPSHADOW,
		DO,
		DOUBLE_PRECISION,
		DTOC,

		ELAPSED_TIME,
		ELSE,
		END,
		ENUM,
		EOF,
		ESCAPE,
		EVAL,
		EVENTS,
		EXEC,
		EXPAND,
		EXPORT,
		EXTENSION,

		FACENAME,
		FALSE,
		FIELD,
		FILE,
		FILEEXISTS,
		FINALIZE,
		FIND,
		FINT,
		FISCAL_END,
		FLONG,
		FLOOR,
		FONTSTYLE,
		FONTSTYLES,
		FOR,
        FORCE,
		FORMAT,
		FORMATSTYLE,
		FORMATSTYLES,
		FORMFEED,
		FROM,
		FULL,
		FUNCPROTOTYPES,

		GETAPPTITLE,
		GETBARCODE_ID,
		GETCOMPANYNAME,
		GETCOMPUTERNAME,
		GETCULTURE,
		GETDATABASETYPE,
		GETDOCTITLE,
		GETEDITION,
		GETINSTALLATIONNAME,
		GETINSTALLATIONPATH,
		GETINSTALLATIONVERSION,
		GETLOGINNAME,
		GETMODTITLE,
		GETNEWGUID,
		GETNSFROMPATH,
		GETPATHFROMNS,
		GETOWNERNAMESPACE,
		GETPRODUCTLANGUAGE,
		GETREPORTMODULENAMESPACE,
        GETREPORTNAME,
        GETREPORTNAMESPACE,
		GETREPORTPATH,
		GETSETTING,
        GETTITLE,
        GETTHREADCONTEXT,
        OWNTHREADCONTEXT,
        GETUPPERLIMIT,
		GETUSERDESCRIPTION,
		GETWINDOWUSER,
		GIULIANDATE,
		GOTO,
		GROUP,

		HAVING,
		HEIGHTS,
		HELP,
		HIDDEN,
		HIDE_ALL_TABLE_TITLE,
		HIDE_LS0,
		HIDE_MS0,
		HIDE_TABLE_TITLE,
		HOTLINK,
		HTMLFILE,

		IF,
		IN,
		INCH,
		INCLUDE,
		INDEX,
		INIT,
		INPUT,
		INSERT,
		INTEGER,
		INTERLINE,
		INTO,
		INVALID,
		IS,
		ISACTIVATED,
        ISADMIN,
		ISAUTOPRINT,
		ISDATABASEUNICODE,
        ISEMPTY,
        ISNULL,
        ISREMOTEINTERFACE,
		ISRUNNINGFROMEXTERNALCONTROLLER,
        ISWEB,
		ITALIC,

		KEY,
		JOIN,

		LABEL,
		LANDSCAPE,
		LAST_MONTH_DAY,
		LEFT,
		LEN,
		EASYVIEW,
		LIKE,
		LINKS,
		LINKFORM,
		LINKFUNCTION,
		LINKRADAR,
		LINKREPORT,
		LINKURL,
		LOADTEXT,
		LOCALIZE,
		LOGIC,
		LONG_INTEGER,
		LONG_STRING,
		LOWER,
		LOWER_LIMIT,
		LTRIM,

		MAIL,
		MAKELOWERLIMIT,
		MAKEUPPERLIMIT,
		MARGINS,
		MAX,
		MAXIMIZED,
		MENU,
		MENUITEM,
		METAFILE,
		MESSAGE,
		MIN,
		MINIMIZED,
		MOD,
		MONEY,
		MONTH,
		MONTH_DAYS,
		MONTH_NAME,
        MULTI_SELECTIONS,

		NATIVE,
		NEXTLINE,
		NO_BOB,
		NO_BOL,
		NO_BOR,
		NO_BOT,
		NO_BORDERS,
		NO_CSE,
		NO_CTL,
		NO_CTR,
		NO_CTS,
		NO_CTT,
		NODUPKEY,
		NO_HRULER,
		NO_INTERFACE,
		NO_PRN_BKGN_BITMAP,
		NO_PRN_BORDERS,
		NO_PRN_LABELS,
		NO_PRN_TITLES,
		NO_STATUSBAR,
		NOT,
		NO_TOOLBAR,
		NO_TTL,
		NO_TTR,
		NO_TTT,
		NOTOKEN,
		NO_TOB,
		NO_TOL,
		NO_TOR,
		NO_CON_BKGN_BITMAP,
		NO_CON_BORDERS,
		NO_CON_LABELS,
		NO_CON_TITLES,
		NO_VRULER,
		NO_WEB,
		NO_XML,
		NULL,
		NUMERIC,

		OBJECTS,
		OF,
		ON,
		ONLY_GRAPH,
		OPEN,
		OPTIONS,
		OR,
		OPTIONAL,
		ORDER,
		ORDER_FIND_FIELD,
		ORIGIN,
		OUT,
		OUTER,
		//OWNER_ID,

		PADDED,
		PAGE_INFO,
		PAGE_LAYOUT,

		PAGE_HSPLITTER,
		PAGE_PRINTER_INFO,
		PAGE_VSPLITTER,

		PATH,
		PEN,
		PERCENT,
		POPUP,
		POSTFIX,
		PRECISION,
		PREFIX,
		PROCEDURE,
		PROCEDURES,
		PROMPT,
		PROPERTIES,
		PROPORTIONAL,

		QUANTITY,
		QUERIES,
		QUERY,
		QUIT,

		RAND,
		RATIO,
		READ_ONLY,
		RECT,
		REF,
		REINIT,
		RELEASE,
		REPLACE,
		REPORT,
		REPORTPRODUCER,
		REPORTS,
		RESET,
        //REM,
		REMOVENEWLINE,
		REVERSEFIND,
		RGB,
		RIGHT,
		RNDRECT,
		ROUND,
		ROW,
		RTRIM,
		RULES,
		RUNREPORT,
		RETURN,
		REPEATER,

		SAVE,
        SAVETEXT,
        SEGMENTED,
		SELECT,
		SEPARATOR,
		SET,
		SETCULTURE,
        SETSETTING,
		SIGN,
		SINGLE_PRECISION,
		SIZE,
		SIZEOF,
		SPACE,
		SPACELINE,
		SPAWN,
		SPECIAL_FIELD,
		SQL_EXEC,
		SQRRECT,
		STATIC,
		STATUS,
		STR,
		STRING,
		STRINGTABLE,
		STRIKEOUT,
		STRUCT,
		STYLE,
		SUBJECT,
		SUBSTR,
		SUBSTRWW,
		SUBTOTAL,
		SUBTOTALS,
		SWITCH,

		TAB,
		TABLE,
		TABLEEXISTS,
		TABLES,
		TEMPLATE,
		TEXT,
		TEXTCOLOR,
		THOUSAND,
		TIME,
		TITLE,
		TITLE_BOTTOM,
		THEN,
		TOP,
		TOTAL,
		TOTAL_TOP,
		TOTALS,
		TOOLTIP,
		TRANSPARENT,
		TRIM,
		TRUE,
		TYPE,
		TYPED_BARCODE,
		TYPEDEF,
		TVAR,

		UNDEF,
		UNDERLINE,
		UPDATE,
		UPPER,
		UPPER_LIMIT,
		USE_DRAFT_FONT,
		UUID,

		VAL,
		VALUEOF,
		VAR,
		VMERGE_EMPTY_CELL,
        VMERGE_EQUAL_CELL,
        VMERGE_TAIL_CELL,

		WEEKDAY,
		WHEN,
		WHERE,
		WHILE,
		WIDTH,
		WEEKOFMONTH,
		WEEKOFYEAR,
        WILDCARD_MATCH,

		YE_RSE,
		YEAR,

        DateAdd,
        WeekStartDate,
        IsLeapYear,
        EasterSunday,

        SendBalloon,
        FormatTbLink,

		// -------------------------------------------------  keywords end
		//new tokens post 3.0
		CONVERT,
		TYPEOF,
		ADDRESSOF,
		EXECUTESCRIPT,

		// -------------------------------------------------  user defined start
		USR00, USR01, USR02, USR03, USR04, USR05, USR06, USR07, USR08, USR09,
		USR10, USR11, USR12, USR13, USR14, USR15, USR16, USR17, USR18, USR19,
		USR20, USR21, USR22, USR23, USR24, USR25, USR26, USR27, USR28, USR29,
		USR30, USR31, USR32, USR33, USR34, USR35, USR36, USR37, USR38, USR39,
		USR40, USR41, USR42, USR43, USR44, USR45, USR46, USR47, USR48, USR49,
		USR50, USR51, USR52, USR53, USR54, USR55, USR56, USR57, USR58, USR59,
		USR60, USR61, USR62, USR63, USR64, USR65, USR66, USR67, USR68, USR69,
		USR70, USR71, USR72, USR73, USR74, USR75, USR76, USR77, USR78, USR79,
		USR80, USR81, USR82, USR83, USR84, USR85, USR86, USR87, USR88, USR89,
		USR90, USR91, USR92, USR93, USR94, USR95, USR96, USR97, USR98, USR99,

		// -------------------------------------------------  user defined end
	};
}
