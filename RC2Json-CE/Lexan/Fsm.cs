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
using System.Globalization;

namespace RC2Json
{
	/// <summary>
	/// Summary description for Fsm.
	/// </summary>
	/// ================================================================================
	internal class Fsm
	{
		public enum State
		{
			START, OPER, ID, NUM, EXP, STRING, ENDSTR,
			SSTRING, ENDSSTR, SEP, BRACK, ZERONUM, HEXNUM, DECSEP,
			CMNT, LSTCMT, ENDCMT, BEGCMT, CMTEOL, ENDF
		};
		public enum Action { LAST, SKIP, INCL};

		public enum CharClass
		{
			OPERATOR,	BRACKET, ALPHA,	DIGITS, EXPONENT, DQUOTE, SQUOTE,
			SLASH, STAR, SEPARATOR, SPACE, ZERO, HEXSYMB, HEXALPHA, DECSEP
		};
		
		
		//............. Transition state table for Finite State Machine
		public static State[,]  TransitionTable =
		{
		/*OPERATOR		BRACKET			ALPHA			DIGITS			EXPONENT		DQUOTE			SQUOTE			SLASH			STAR			SEP				SPACE			ZERO			HEXSYMB			HEXALPHA		DECSEP */

		{State.OPER,	State.BRACK,	State.ID,		State.NUM,		State.ID,		State.STRING,	State.SSTRING,	State.BEGCMT,	State.OPER,		State.SEP,		State.START,	State.ZERONUM,	State.ID,		State.ID,		State.DECSEP	}, /*START*/
		{State.OPER,	State.BRACK,	State.ID,		State.NUM,		State.ID,		State.STRING,	State.SSTRING,	State.BEGCMT,	State.OPER,		State.SEP,		State.START,	State.ZERONUM,	State.ID,		State.ID,		State.DECSEP	}, /*OPER*/
		{State.OPER,	State.BRACK,	State.ID,		State.ID,		State.ID,		State.STRING,	State.SSTRING,	State.BEGCMT,	State.OPER,		State.SEP,		State.START,	State.ID,		State.ID,		State.ID,		State.ID		}, /*ID*/
		{State.OPER,	State.BRACK,	State.ID,		State.NUM,		State.EXP,		State.STRING,	State.SSTRING,	State.BEGCMT,	State.OPER,		State.SEP,		State.START,	State.NUM,		State.ID,		State.ID,		State.NUM		}, /*NUM*/
		{State.NUM,		State.BRACK,	State.ID,		State.NUM,		State.ID,		State.STRING,	State.SSTRING,	State.BEGCMT,	State.NUM, 		State.SEP,		State.START,	State.NUM,		State.ID,		State.ID,		State.NUM		}, /*EXP*/
		{State.STRING,	State.STRING, 	State.STRING,	State.STRING,	State.STRING,	State.ENDSTR,	State.STRING, 	State.STRING,	State.STRING,	State.STRING,	State.STRING,	State.STRING,	State.STRING,	State.STRING,	State.STRING	}, /*STRING*/
		{State.OPER,	State.BRACK,  	State.ID,		State.NUM,		State.ID,		State.STRING,	State.SSTRING,	State.BEGCMT,	State.OPER,		State.SEP,		State.START,	State.ZERONUM,	State.ID,		State.ID,		State.DECSEP	}, /*ENDSTR*/
		{State.SSTRING,	State.SSTRING,	State.SSTRING,	State.SSTRING,	State.SSTRING,	State.SSTRING,	State.ENDSSTR,	State.SSTRING,	State.SSTRING,	State.SSTRING,	State.SSTRING,	State.SSTRING,	State.SSTRING,	State.SSTRING,	State.SSTRING	}, /*SSTRING*/
		{State.OPER,	State.BRACK,  	State.ID,		State.NUM,		State.ID,		State.STRING,	State.SSTRING,	State.BEGCMT,	State.OPER,		State.SEP,		State.START,	State.ZERONUM,	State.ID,		State.ID, 		State.DECSEP	}, /*ENDSSTR*/
		{State.OPER,	State.BRACK,  	State.ID,		State.NUM,		State.ID,		State.STRING,	State.SSTRING,	State.BEGCMT,	State.OPER,		State.SEP,		State.START,	State.ZERONUM,	State.ID,		State.ID, 		State.DECSEP	}, /*SEP*/
		{State.OPER,	State.BRACK,  	State.ID,		State.NUM,		State.ID,		State.STRING,	State.SSTRING,	State.BEGCMT,	State.OPER,		State.SEP,		State.START,	State.ZERONUM,	State.ID,		State.ID, 		State.DECSEP	}, /*BRACK*/
		{State.OPER,	State.BRACK,  	State.ID,		State.NUM,		State.EXP,		State.STRING,	State.SSTRING,	State.BEGCMT,	State.OPER,		State.SEP,		State.START,	State.ZERONUM,	State.HEXNUM,	State.ID, 		State.DECSEP	}, /*ZERONUM*/
		{State.OPER,	State.BRACK,  	State.ID,		State.HEXNUM,	State.HEXNUM,	State.STRING,	State.SSTRING,	State.BEGCMT,	State.OPER,		State.SEP,		State.START,	State.HEXNUM,	State.ID,		State.HEXNUM,	State.ID		}, /*HEXNUM*/
		{State.OPER,	State.BRACK,  	State.ID,		State.NUM,		State.ID,		State.STRING,	State.SSTRING,	State.BEGCMT,	State.OPER,		State.SEP,		State.START,	State.NUM,		State.ID,		State.ID,		State.ID		}, /*DECSEP*/

			/*  analize comments in C++ form */

		{State.CMNT,	State.CMNT,		State.CMNT,		State.CMNT,		State.CMNT,  	State.CMNT,  	State.CMNT,		State.CMNT,  	State.LSTCMT,	State.CMNT,  	State.CMNT,		State.CMNT,		State.CMNT,		State.CMNT,  	State.CMNT		}, /*CMNT*/
		{State.CMNT,	State.CMNT,		State.CMNT,		State.CMNT,		State.CMNT,  	State.CMNT,  	State.CMNT,   	State.ENDCMT,	State.LSTCMT,	State.CMNT,  	State.CMNT,		State.CMNT,		State.CMNT,		State.CMNT,  	State.CMNT		}, /*LSTCMT*/
		{State.OPER,	State.BRACK,	State.ID,		State.NUM, 		State.ID,    	State.STRING,	State.SSTRING,	State.BEGCMT,	State.OPER,  	State.SEP,   	State.START,	State.NUM,		State.ID,		State.ID,    	State.DECSEP	}, /*ENDCMT*/
		{State.OPER,	State.BRACK,	State.ID,		State.NUM, 		State.ID,    	State.STRING,	State.SSTRING,	State.CMTEOL,	State.CMNT,  	State.SEP,   	State.START,	State.NUM,		State.ID,		State.ID,    	State.DECSEP	}, /*BEGCMT*/
		{State.CMTEOL,	State.CMTEOL,	State.CMTEOL,	State.CMTEOL,	State.CMTEOL,	State.CMTEOL,	State.CMTEOL, 	State.CMTEOL,	State.CMTEOL,	State.CMTEOL,	State.CMTEOL,	State.CMTEOL,	State.CMTEOL,	State.CMTEOL,	State.CMTEOL	}, /*CMTEOL*/
		};

		
		//................ emit table row entry fsm, column entry
		public static Action[,] EmitTable=
		{
		/*OPERATOR		BRACKET			ALPHA			DIGITS			EXPONENT		DQUOTE			SQUOTE			SLASH			STAR			SEP				SPACE			ZERO			HEXSYMB			HEXALPHA		DECSEP*/		
		{Action.SKIP,	Action.SKIP,	Action.SKIP,	Action.SKIP,	Action.SKIP,	Action.SKIP,	Action.SKIP,	Action.SKIP,	Action.SKIP,	Action.SKIP,	Action.SKIP,	Action.SKIP,	Action.SKIP,    Action.SKIP,	Action.SKIP		}, /*START*/
		{Action.INCL,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.INCL,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST		}, /*OP*/
		{Action.LAST,	Action.LAST,	Action.INCL,	Action.INCL,	Action.INCL,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL		}, /*ID*/
		{Action.LAST,	Action.LAST,	Action.LAST,	Action.INCL,	Action.INCL,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.INCL,	Action.LAST,	Action.LAST,	Action.INCL		}, /*NUM*/
		{Action.INCL,	Action.LAST,	Action.INCL,	Action.INCL,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.INCL,	Action.LAST,	Action.LAST,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL		}, /*EXP*/
		{Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL		}, /*STRING*/
		{Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.INCL,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST		}, /*ENDSTR*/
		{Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL		}, /*SSTRING*/
		{Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.INCL,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST		}, /*ENDSSTR*/
		{Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST		}, /*SEP*/
		{Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST		}, /*BRACK*/
		{Action.LAST,	Action.LAST,	Action.LAST,	Action.INCL,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.INCL,	Action.INCL,	Action.LAST,	Action.INCL		}, /*ZERONUM*/
		{Action.LAST,	Action.LAST,	Action.LAST,	Action.INCL,	Action.INCL,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.INCL,	Action.LAST,	Action.INCL,	Action.LAST		}, /*HEXNUM*/
		{Action.LAST,	Action.LAST,	Action.INCL,	Action.INCL,	Action.INCL,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL		}, /*DECSEP*/

		/*  analize comments in C++ form */
		{Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL		}, /*CMNT*/
		{Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL		}, /*LSTCMT*/
		{Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST		}, /*ENDCMT*/
		{Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.INCL,	Action.INCL,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST,	Action.LAST		}, /*BEGCMT*/
		{Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL,	Action.INCL		}, /*CMTEOL*/
		};

		// unicode compatible character class translation
		public class CharClassTable
		{
			public static CharClass Class(char c)
			{
				if (Char.IsControl(c) || c == ' ' || c == '\\' || c == '#') return CharClass.SPACE;
				if (c == ';') return CharClass.SEPARATOR;
				if (c == '0') return CharClass.ZERO;
				if (Char.IsDigit(c)) return CharClass.DIGITS;
				if (c == '.') return CharClass.DECSEP;
				if (c == '*') return CharClass.STAR;
				if (c == '\'') return CharClass.SQUOTE;
				if (c == '"') return CharClass.DQUOTE;
				if (c == '/') return CharClass.SLASH;
				if (c == '!' || c == '$' || c == '%' || c == '&' || c == '+' || c == '-' || c == '<' || c == '=' || c == '>' || c == '?' || c == '^' || c == '~' || c == '`' || c == '|') return CharClass.OPERATOR;
				if (c == '(' || c == ')' || c == ',' || c == ':' || c == '[' || c == ']' || c == '{' || c == '}') return CharClass.BRACKET;

				char u = Char.ToUpper(c, CultureInfo.InvariantCulture);
				if (u == 'E') return CharClass.EXPONENT;
				if (u == 'X') return CharClass.HEXSYMB;
				if (u == 'A' || u == 'B' || u == 'C' || u == 'D' || u == 'E' || u == 'F') return CharClass.HEXALPHA;

				return CharClass.ALPHA;
			}
		};               
	}
}
