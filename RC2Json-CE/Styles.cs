﻿/*
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
	public enum WindowExStyles
	{
		WS_EX_DLGMODALFRAME = 0x00000001,
		WS_EX_NOPARENTNOTIFY = 0x00000004,
		WS_EX_TOPMOST = 0x00000008,
		WS_EX_ACCEPTFILES = 0x00000010,
		WS_EX_TRANSPARENT = 0x00000020,
		WS_EX_MDICHILD = 0x00000040,
		WS_EX_TOOLWINDOW = 0x00000080,
		WS_EX_WINDOWEDGE = 0x00000100,
		WS_EX_CLIENTEDGE = 0x00000200,
		WS_EX_CONTEXTHELP = 0x00000400,
		WS_EX_RIGHT = 0x00001000,
		WS_EX_LEFT = 0x00000000,
		WS_EX_RTLREADING = 0x00002000,
		WS_EX_LTRREADING = 0x00000000,
		WS_EX_LEFTSCROLLBAR = 0x00004000,
		WS_EX_RIGHTSCROLLBAR = 0x00000000,
		WS_EX_CONTROLPARENT = 0x00010000,
		WS_EX_STATICEDGE = 0x00020000,
		WS_EX_APPWINDOW = 0x00040000,
		WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE),
		WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST),
		WS_EX_LAYERED = 0x00080000,
		WS_EX_NOREDIRECTIONBITMAP = 0x00200000,
		WS_EX_NOINHERITLAYOUT = 0x00100000,
		WS_EX_NOACTIVATE = 0x08000000,
		WS_EX_LAYOUTRTL = 0x00400000,
		WS_EX_COMPOSITED = 0x02000000,

	}

	public enum WindowStyles : uint
	{
		WS_OVERLAPPED = 0x00000000,
		WS_POPUP = 0x80000000,
		WS_CHILD = 0x40000000,
		WS_MINIMIZE = 0x20000000,
		WS_VISIBLE = 0x10000000,
		WS_DISABLED = 0x08000000,
		WS_CLIPSIBLINGS = 0x04000000,
		WS_CLIPCHILDREN = 0x02000000,
		WS_MAXIMIZE = 0x01000000,
		WS_CAPTION = 0x00C00000,
		WS_BORDER = 0x00800000,
		WS_DLGFRAME = 0x00400000,
		WS_VSCROLL = 0x00200000,
		WS_HSCROLL = 0x00100000,
		WS_SYSMENU = 0x00080000,
		WS_THICKFRAME = 0x00040000,
		WS_GROUP = 0x00020000,
		WS_TABSTOP = 0x00010000,
		WS_MINIMIZEBOX = 0x00020000,
		WS_MAXIMIZEBOX = 0x00010000,
		WS_TILED = 0x00000000,
		WS_ICONIC = 0x20000000,
		WS_SIZEBOX = 0x00040000,
		WS_TILEDWINDOW = 0x00CF0000,
		WS_OVERLAPPEDWINDOW = (WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX),
		WS_POPUPWINDOW = (WS_POPUP | WS_BORDER | WS_SYSMENU),
		WS_CHILDWINDOW = (WS_CHILD),

		/*
		 * Edit Control Styles
		 */
		ES_LEFT = 0x0000,
		ES_CENTER = 0x0001,
		ES_RIGHT = 0x0002,
		ES_MULTILINE = 0x0004,
		ES_UPPERCASE = 0x0008,
		ES_LOWERCASE = 0x0010,
		ES_PASSWORD = 0x0020,
		ES_AUTOVSCROLL = 0x0040,
		ES_AUTOHSCROLL = 0x0080,
		ES_NOHIDESEL = 0x0100,
		ES_OEMCONVERT = 0x0400,
		ES_READONLY = 0x0800,
		ES_WANTRETURN = 0x1000,
		ES_NUMBER = 0x2000,


		/*
		 * Button Control Styles
		 */
		BS_PUSHBUTTON = 0x00000000,
		BS_DEFPUSHBUTTON = 0x00000001,
		BS_CHECKBOX = 0x00000002,
		BS_AUTOCHECKBOX = 0x00000003,
		BS_RADIOBUTTON = 0x00000004,
		BS_3STATE = 0x00000005,
		BS_AUTO3STATE = 0x00000006,
		BS_GROUPBOX = 0x00000007,
		BS_USERBUTTON = 0x00000008,
		BS_AUTORADIOBUTTON = 0x00000009,
		BS_PUSHBOX = 0x0000000A,
		BS_OWNERDRAW = 0x0000000B,
		BS_TYPEMASK = 0x0000000F,
		BS_LEFTTEXT = 0x00000020,
		BS_TEXT = 0x00000000,
		BS_ICON = 0x00000040,
		BS_BITMAP = 0x00000080,
		BS_LEFT = 0x00000100,
		BS_RIGHT = 0x00000200,
		BS_CENTER = 0x00000300,
		BS_TOP = 0x00000400,
		BS_BOTTOM = 0x00000800,
		BS_VCENTER = 0x00000C00,
		BS_PUSHLIKE = 0x00001000,
		BS_MULTILINE = 0x00002000,
		BS_NOTIFY = 0x00004000,
		BS_FLAT = 0x00008000,
		BS_RIGHTBUTTON = BS_LEFTTEXT,


		/*
		 * Static Control Constants
		 */
		SS_LEFT = 0x00000000,
		SS_CENTER = 0x00000001,
		SS_RIGHT = 0x00000002,
		SS_ICON = 0x00000003,
		SS_BLACKRECT = 0x00000004,
		SS_GRAYRECT = 0x00000005,
		SS_WHITERECT = 0x00000006,
		SS_BLACKFRAME = 0x00000007,
		SS_GRAYFRAME = 0x00000008,
		SS_WHITEFRAME = 0x00000009,
		SS_USERITEM = 0x0000000A,
		SS_SIMPLE = 0x0000000B,
		SS_LEFTNOWORDWRAP = 0x0000000C,
		SS_OWNERDRAW = 0x0000000D,
		SS_BITMAP = 0x0000000E,
		SS_ENHMETAFILE = 0x0000000F,
		SS_ETCHEDHORZ = 0x00000010,
		SS_ETCHEDVERT = 0x00000011,
		SS_ETCHEDFRAME = 0x00000012,
		SS_TYPEMASK = 0x0000001F,
		SS_REALSIZECONTROL = 0x00000040,
		SS_NOPREFIX = 0x00000080,
		SS_NOTIFY = 0x00000100,
		SS_CENTERIMAGE = 0x00000200,
		SS_RIGHTJUST = 0x00000400,
		SS_REALSIZEIMAGE = 0x00000800,
		SS_SUNKEN = 0x00001000,
		SS_EDITCONTROL = 0x00002000,
		SS_ENDELLIPSIS = 0x00004000,
		SS_PATHELLIPSIS = 0x00008000,
		SS_WORDELLIPSIS = 0x0000C000,
		SS_ELLIPSISMASK = 0x0000C000,


		// Dialog Styles
		DS_ABSALIGN = 0x01,
		DS_SYSMODAL = 0x02,
		DS_LOCALEDIT = 0x20,
		DS_SETFONT = 0x40,
		DS_MODALFRAME = 0x80,
		DS_NOIDLEMSG = 0x100,
		DS_SETFOREGROUND = 0x200,
		DS_3DLOOK = 0x0004,
		DS_FIXEDSYS = 0x0008,
		DS_NOFAILCREATE = 0x0010,
		DS_CONTROL = 0x0400,
		DS_CENTER = 0x0800,
		DS_CENTERMOUSE = 0x1000,
		DS_CONTEXTHELP = 0x2000,
		DS_SHELLFONT = (DS_SETFONT | DS_FIXEDSYS),

		// Listbox Styles
		LBS_NOTIFY = 0x0001,
		LBS_SORT = 0x0002,
		LBS_NOREDRAW = 0x0004,
		LBS_MULTIPLESEL = 0x0008,
		LBS_OWNERDRAWFIXED = 0x0010,
		LBS_OWNERDRAWVARIABLE = 0x0020,
		LBS_HASSTRINGS = 0x0040,
		LBS_USETABSTOPS = 0x0080,
		LBS_NOINTEGRALHEIGHT = 0x0100,
		LBS_MULTICOLUMN = 0x0200,
		LBS_WANTKEYBOARDINPUT = 0x0400,
		LBS_EXTENDEDSEL = 0x0800,
		LBS_DISABLENOSCROLL = 0x1000,
		LBS_NODATA = 0x2000,
		LBS_NOSEL = 0x4000,
		LBS_COMBOBOX = 0x8000,
		LBS_STANDARD = (LBS_NOTIFY | LBS_SORT | WS_VSCROLL | WS_BORDER),

		// combobox styles
		CBS_SIMPLE = 0x0001,
		CBS_DROPDOWN = 0x0002,
		CBS_DROPDOWNLIST = 0x0003,
		CBS_OWNERDRAWFIXED = 0x0010,
		CBS_OWNERDRAWVARIABLE = 0x0020,
		CBS_AUTOHSCROLL = 0x0040,
		CBS_OEMCONVERT = 0x0080,
		CBS_SORT = 0x0100,
		CBS_HASSTRINGS = 0x0200,
		CBS_NOINTEGRALHEIGHT = 0x0400,
		CBS_DISABLENOSCROLL = 0x0800,
		CBS_UPPERCASE = 0x2000,
		CBS_LOWERCASE = 0x4000,


		// Scroll Bar Styles
		SBS_HORZ = 0x0000,
		SBS_VERT = 0x0001,
		SBS_TOPALIGN = 0x0002,
		SBS_LEFTALIGN = 0x0002,
		SBS_BOTTOMALIGN = 0x0004,
		SBS_RIGHTALIGN = 0x0004,
		SBS_SIZEBOXTOPLEFTALIGN = 0x0002,
		SBS_SIZEBOXBOTTOMRIGHTALIGN = 0x0004,
		SBS_SIZEBOX = 0x0008,
		SBS_SIZEGRIP = 0x0010,

		//treeview styles

		TVS_HASBUTTONS = 0x0001,
		TVS_HASLINES = 0x0002,
		TVS_LINESATROOT = 0x0004,
		TVS_EDITLABELS = 0x0008,
		TVS_DISABLEDRAGDROP = 0x0010,
		TVS_SHOWSELALWAYS = 0x0020,
		TVS_RTLREADING = 0x0040,
		TVS_NOTOOLTIPS = 0x0080,
		TVS_CHECKBOXES = 0x0100,
		TVS_TRACKSELECT = 0x0200,
		TVS_SINGLEEXPAND = 0x0400,
		TVS_INFOTIP = 0x0800,
		TVS_FULLROWSELECT = 0x1000,
		TVS_NOSCROLL = 0x2000,
		TVS_NONEVENHEIGHT = 0x4000,
		TVS_NOHSCROLL = 0x8000,


		//tab control styles
		TCS_SCROLLOPPOSITE = 0x0001,
		TCS_BOTTOM = 0x0002,
		TCS_RIGHT = 0x0002,
		TCS_MULTISELECT = 0x0004,
		TCS_FLATBUTTONS = 0x0008,
		TCS_FORCEICONLEFT = 0x0010,
		TCS_FORCELABELLEFT = 0x0020,
		TCS_HOTTRACK = 0x0040,
		TCS_VERTICAL = 0x0080,
		TCS_TABS = 0x0000,
		TCS_BUTTONS = 0x0100,
		TCS_SINGLELINE = 0x0000,
		TCS_MULTILINE = 0x0200,
		TCS_RIGHTJUSTIFY = 0x0000,
		TCS_FIXEDWIDTH = 0x0400,
		TCS_RAGGEDRIGHT = 0x0800,
		TCS_FOCUSONBUTTONDOWN = 0x1000,
		TCS_OWNERDRAWFIXED = 0x2000,
		TCS_TOOLTIPS = 0x4000,
		TCS_FOCUSNEVER = 0x8000,

		//spin styles
		UDS_WRAP = 0x0001,
		UDS_SETBUDDYINT = 0x0002,
		UDS_ALIGNRIGHT = 0x0004,
		UDS_ALIGNLEFT = 0x0008,
		UDS_AUTOBUDDY = 0x0010,
		UDS_ARROWKEYS = 0x0020,
		UDS_HORZ = 0x0040,
		UDS_NOTHOUSANDS = 0x0080,
		UDS_HOTTRACK = 0x0100,

		//progress styles
		PBS_SMOOTH = 0x01,
		PBS_VERTICAL = 0x04,


		//list view styles
		LVS_ICON = 0x0000,
		LVS_REPORT = 0x0001,
		LVS_SMALLICON = 0x0002,
		LVS_LIST = 0x0003,
		LVS_TYPEMASK = 0x0003,
		LVS_SINGLESEL = 0x0004,
		LVS_SHOWSELALWAYS = 0x0008,
		LVS_SORTASCENDING = 0x0010,
		LVS_SORTDESCENDING = 0x0020,
		LVS_SHAREIMAGELISTS = 0x0040,
		LVS_NOLABELWRAP = 0x0080,
		LVS_AUTOARRANGE = 0x0100,
		LVS_EDITLABELS = 0x0200,
		LVS_OWNERDATA = 0x1000,
		LVS_NOSCROLL = 0x2000,
		LVS_TYPESTYLEMASK = 0xfc00,
		LVS_ALIGNTOP = 0x0000,
		LVS_ALIGNLEFT = 0x0800,
		LVS_ALIGNMASK = 0x0c00,
		LVS_OWNERDRAWFIXED = 0x0400,
		LVS_NOCOLUMNHEADER = 0x4000,
		LVS_NOSORTHEADER = 0x8000,

		ACS_CENTER = 0x0001,
		ACS_TRANSPARENT = 0x0002,
		ACS_AUTOPLAY = 0x0004,
		ACS_TIMER = 0x0008
	}

}
