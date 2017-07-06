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
using System.Threading.Tasks;

namespace RC2Json
{
	class StyleMap
	{
		public string Style { get; set; }
		public string JsonProperty { get; set; }
		public object JsonValue { get; set; }
		public StyleMap(string style, string jsonProperty, object jsonValue)
		{
			this.Style = style;
			this.JsonProperty = jsonProperty;
			this.JsonValue = jsonValue;
		}
		public static string[] IgnoredStyles = 
		{
 			"DS_SETFONT"
		};
		public static StyleMap[] Map = 
		{
			//stili estesi
			new StyleMap("WS_EX_CLIENTEDGE",		"border", true),
			new StyleMap("WS_EX_ACCEPTFILES",		"acceptFiles", true),
			new StyleMap("WS_EX_TRANSPARENT",		"transparent", true),
			
			//stili di finestra
			new StyleMap("WS_VISIBLE",				"visible", true),
			new StyleMap("WS_BORDER",				"border", true),
			new StyleMap("WS_CHILD",				"child", true),
			new StyleMap("WS_CHILDWINDOW",			"child", true),
			new StyleMap("WS_DISABLED",				"enabled", false),
			new StyleMap("WS_TABSTOP",				"tabStop", true),
			
			new StyleMap("WS_CLIPCHILDREN",			"clipChildren", true),
			new StyleMap("WS_CLIPSIBLINGS",			"clipSiblings", true),
			new StyleMap("WS_CAPTION",				"caption", true),
			new StyleMap("WS_SYSMENU",				"systemMenu", true),
			new StyleMap("WS_DLGFRAME",				"dialogFrame", true),
			new StyleMap("WS_GROUP",				"group", true),
			new StyleMap("WS_MAXIMIZEBOX",			"maximizeBox", true),
			new StyleMap("WS_MINIMIZEBOX",			"minimizeBox", true),
			new StyleMap("WS_OVERLAPPED",			"overlapped", true),
			new StyleMap("WS_TILED",				"overlapped", true),
			new StyleMap("WS_SIZEBOX",				"thickFrame", true),
			new StyleMap("WS_THICKFRAME",			"thickFrame", true),
			
			new StyleMap("WS_HSCROLL",				"hScroll", true),
			new StyleMap("WS_VSCROLL",				"vScroll", true),
			new StyleMap("WS_POPUP",				"child", false),
			
			//stili di dialog
			new StyleMap("DS_MODALFRAME",			"modalFrame", true),
			new StyleMap("DS_CENTER",				"center", true),
			new StyleMap("DS_CONTROL",				"userControl", true),
			new StyleMap("DS_CENTERMOUSE",			"centerMouse", true),
			//edit text
			new StyleMap("ES_AUTOHSCROLL",			"autoHScroll", true),
			new StyleMap("ES_AUTOVSCROLL",			"autoVScroll", true),
			new StyleMap("ES_UPPERCASE",			"upperCase", true),
			new StyleMap("ES_LOWERCASE",			"lowerCase", true),
			new StyleMap("ES_WANTRETURN",			 "wantReturn", true),
			new StyleMap("ES_READONLY",				"readOnly", true),
			new StyleMap("ES_NUMBER",				"number", true),
			new StyleMap("ES_PASSWORD",				"password", true),
			new StyleMap("ES_NOHIDESEL",			"noHideSelection", true),
			new StyleMap("ES_OEMCONVERT",			"oemConvert", true),
			new StyleMap("ES_MULTILINE",			"multiline", true),
			
			//button
			new StyleMap("BS_BITMAP",				"bitmap", true),
			new StyleMap("BS_FLAT",					"flat", true),
			new StyleMap("BS_ICON",					"icon", true),
			new StyleMap("BS_TEXT",					"isText", true),
			
			new StyleMap("BS_OWNERDRAW",			"ownerDraw", true),
			new StyleMap("BS_MULTILINE",			"multiline", true),
			new StyleMap("BS_TOP",					"vertAlign", VerticalAlignment.TOP),
			new StyleMap("BS_VCENTER",				"vertAlign", VerticalAlignment.CENTER),
			new StyleMap("BS_BOTTOM",				"vertAlign", VerticalAlignment.BOTTOM),

			//static
			new StyleMap("SS_BITMAP",				"bitmap", true),
			new StyleMap("SS_BLACKFRAME",			"blackFrame", true),
			new StyleMap("SS_BLACKRECT",			"blackRect", true),
			new StyleMap("SS_CENTERIMAGE",			"centerImage", true),
			new StyleMap("SS_EDITCONTROL",			"editControl", true),
			new StyleMap("SS_ENDELLIPSIS",			"endEllipsis", true),
			new StyleMap("SS_ETCHEDFRAME",			"etchedFrame", EtchedFrameType.EFALL),
			new StyleMap("SS_ETCHEDHORZ",			"etchedFrame", EtchedFrameType.EFHORZ),
			new StyleMap("SS_ETCHEDVERT",			"etchedFrame", EtchedFrameType.EFVERT),
			new StyleMap("SS_GRAYFRAME",			"grayFrame", true),
			new StyleMap("SS_GRAYRECT",				"grayRect", true),
			new StyleMap("SS_ICON",					"icon", true),
			new StyleMap("SS_NOTIFY",				"notify", true),
			new StyleMap("SS_LEFTNOWORDWRAP",		"leftNoWrap", true),
			new StyleMap("SS_NOPREFIX",				"noPrefix", true),
			new StyleMap("SS_OWNERDRAW",			"ownerDraw", true),
			new StyleMap("SS_PATHELLIPSIS",			"pathEllipsis", true),
			new StyleMap("SS_SIMPLE",				"simple", true),
			new StyleMap("SS_SUNKEN",				"sunken", true),
			new StyleMap("SS_WHITEFRAME",			"whiteFrame", true),
			new StyleMap("SS_WHITERECT",			"whiteRect", true),
			new StyleMap("SS_WORDELLIPSIS",			"wordEllipsis", true),
			new StyleMap("SS_REALSIZEIMAGE",		"realSizeImage", true),

			//combo
			new StyleMap("CBS_DROPDOWN",			"comboType", ComboType.DROPDOWN),
			new StyleMap("CBS_DROPDOWNLIST",		"comboType", ComboType.DROPDOWNLIST),
			new StyleMap("CBS_SIMPLE",				"comboType", ComboType.SIMPLE),
			new StyleMap("CBS_UPPERCASE",			"upperCase", true),
			new StyleMap("CBS_LOWERCASE",			"lowerCase", true),
			new StyleMap("CBS_OEMCONVERT",			"oemConvert", true),
			new StyleMap("CBS_SORT",				"sort", true),
			new StyleMap("CBS_NOINTEGRALHEIGHT",	"noIntegralHeight", true),
			new StyleMap("CBS_AUTOHSCROLL",			"auto", true),
			new StyleMap("CBS_OWNERDRAWFIXED",		"ownerDraw", OwnerDrawType.ODFIXED),
			new StyleMap("CBS_OWNERDRAWVARIABLE",	"ownerDraw", OwnerDrawType.ODVARIABLE),
			
			//list
			new StyleMap("LBS_SORT",				"sort", true),
			new StyleMap("LBS_DISABLENOSCROLL",		"disableNoScroll", true),
		
			new StyleMap("LBS_NOSEL",				"selection", SelectionType.NOSEL),
			new StyleMap("LBS_EXTENDEDSEL",			"selection", SelectionType.EXTENDEDSEL),
			new StyleMap("LBS_MULTIPLESEL",			"selection", SelectionType.MULTIPLESEL),
			new StyleMap("LBS_HASSTRINGS",			"hasStrings", true),
			new StyleMap("LBS_NOINTEGRALHEIGHT",	"noIntegralHeight", true),
			new StyleMap("LBS_OWNERDRAWFIXED",		"ownerDraw", OwnerDrawType.ODFIXED),
			new StyleMap("LBS_OWNERDRAWVARIABLE",	"ownerDraw", OwnerDrawType.ODVARIABLE),
			new StyleMap("LBS_WANTKEYBOARDINPUT",	"wantKeyInput", true),
			new StyleMap("LBS_MULTICOLUMN",			"multiColumn", true),
			
			//list view control
			new StyleMap("LVS_ALIGNTOP",			"alignment", ListCtrlAlign.LC_TOP),
			new StyleMap("LVS_ALIGNLEFT",			"alignment", ListCtrlAlign.LC_LEFT),
			new StyleMap("LVS_AUTOARRANGE",			"autoArrange", true),
			new StyleMap("LVS_NOCOLUMNHEADER",		"noColumnHeader", true),
			new StyleMap("LVS_NOSORTHEADER",		"noSortHeader", true),
			new StyleMap("LVS_SMALLICON",			"view", ListCtrlViewMode.LC_SMALLICON),
			new StyleMap("LVS_LIST",				"view", ListCtrlViewMode.LC_LIST),
			new StyleMap("LVS_REPORT",				"view", ListCtrlViewMode.LC_REPORT),
			new StyleMap("LVS_SHOWSELALWAYS",		"alwaysShowSelection", true),
			new StyleMap("LVS_SINGLESEL",			"singleSelection", true),
			
			//tree
			new StyleMap("TVS_EX_AUTOHSCROLL",		"autoHScroll", true),
			new StyleMap("TVS_CHECKBOXES",			"checkBoxes", true),
			new StyleMap("TVS_DISABLEDRAGDROP",		"disableDragDrop", true),
			new StyleMap("TVS_EDITLABELS",			"editLabels", true),
			new StyleMap("TVS_HASBUTTONS",			"hasButtons", true),
			new StyleMap("TVS_HASLINES",			"hasLines", true),
			new StyleMap("TVS_INFOTIP",				"infoTip", true),
			new StyleMap("TVS_LINESATROOT",			"linesAtRoot", true),
			new StyleMap("TVS_NOTOOLTIPS",			"tooltips", true),
			new StyleMap("TVS_SHOWSELALWAYS",		"alwaysShowSelection", true),

			
			//spin
			new StyleMap("UDS_ALIGNLEFT",			"align", SpinCtrlAlignment.SC_LEFT),
			new StyleMap("UDS_ALIGNRIGHT",			"align", SpinCtrlAlignment.SC_RIGHT),
			new StyleMap("UDS_ARROWKEYS",			"arrowKeys", true),
			new StyleMap("UDS_AUTOBUDDY",			"autoBuddy", true),
			new StyleMap("UDS_NOTHOUSANDS",			"noThousands", true),
			new StyleMap("UDS_SETBUDDYINT",			"setBuddyInteger", true),
			//static
			new StyleMap("SS_OWNERDRAW",			"ownerDraw", true),
		};
	}


}
