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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using RC2Json.Lexan;

namespace RC2Json
{
	/// ================================================================================
	public class Comment : ArrayList
	{
		public enum LineTerminator { CrLf, Cr, Lf, Zero, None };
		internal int tabSize = 1;
		internal string[] terminatorString = { "\r\n", "\r", "\n", "\0", "" };

		//------------------------------------------------------------------------------
		public int TabSize { get { return tabSize; } }

		//------------------------------------------------------------------------------
		public string All(bool noLeadingSlash, LineTerminator terminator)
		{
			string s = "";
			foreach (string cmm in this)
			{
				s += noLeadingSlash ? cmm.Remove(0, 2) : cmm;
				s += terminatorString[(int)terminator];
			}
			return s;
		}

		//------------------------------------------------------------------------------
		internal void AddComment(string src)
		{
			string cmmDelimiter = "//";
			if (src.IndexOf(cmmDelimiter, 0) >= 0)
				Add(src);
			else
				Add(cmmDelimiter + src);
		}

		/// <summary>
		/// Ricarica un oggetto Comment a paartire da una stringa aggiungendo il simbolo 
		/// di commento a inizio riga se non lo trova. Genera tante righe quanti sono i
		/// separatori di riga nella stringa di origine, ignorando l'ultimo
		/// </summary>
		//------------------------------------------------------------------------------
		public void Reload(string src, LineTerminator terminator)
		{
			Clear();

			int startIndex = 0;
			int found = 0;
			string search = terminatorString[(int)terminator];

			while (found >= 0 && startIndex < src.Length)
			{
				found = src.IndexOf(search, startIndex);
				if (found >= 0)
				{
					AddComment(src.Substring(startIndex, found - startIndex));
					startIndex = found + search.Length;
				}
			}
			if (startIndex < src.Length)
				AddComment(src.Substring(startIndex, src.Length - startIndex));
		}
	}


	/// <summary>
	/// Summary description for State.
	/// </summary>
	/// ================================================================================
	internal class ParserState
	{
		public ParserBuffer parserBuffer;
		public Fsm.State currentState = Fsm.State.START;
		public string currentLexeme;
		public bool currentUndefined = true;
		public Fsm.State fsmState = Fsm.State.START;		// Finite State Machine current state
		public double lastNumber = 0.0;
		public bool lastNumberIsDouble = false;
		public bool preprocessorDisabled = false;
		public string workingFolder; //used to find the include file to parse

		private Diagnostic diagnostic = new Diagnostic();
		private string filename;

		public Diagnostic Diagnostic { get { return diagnostic; } }
		public string Filename { get { return filename; } set { filename = value; } }


		//------------------------------------------------------------------------------
		public ParserState(Parser parser)
		{
			parserBuffer = new ParserBuffer(parser);
		}

		//------------------------------------------------------------------------------
		public void Close()
		{
			parserBuffer.Close();

			// Ho rilasciato le cose pesanti e quindi posso inibire il finalize
			GC.SuppressFinalize(this);
		}

		//------------------------------------------------------------------------------
		~ParserState()
		{
			Close();
		}

		//------------------------------------------------------------------------------
		public void SetError(string explain)
		{
			ExtendedInfo info = new ExtendedInfo();

			info.Add("Row", parserBuffer.CurrentLine);
			info.Add("Column", parserBuffer.CurrentPos + 1);
			info.Add("Filename", filename);

			diagnostic.Set(DiagnosticType.Error, explain, info);
		}

		//------------------------------------------------------------------------------
		public void SetError(string explain, long line, int column)
		{
			ExtendedInfo info = new ExtendedInfo();

			info.Add("Row", line);
			info.Add("Column", column);
			info.Add("Filename", filename);

			diagnostic.Set(DiagnosticType.Error, explain, info);
		}

		//------------------------------------------------------------------------------
		public void SetWarning(string explain)
		{
			ExtendedInfo info = new ExtendedInfo();

			info.Add("Row", parserBuffer.CurrentLine);
			info.Add("Column", parserBuffer.CurrentPos + 1);
			info.Add("Filename", filename);

			diagnostic.Set(DiagnosticType.Warning, explain, info);
		}

		//------------------------------------------------------------------------------
		public void ClearError()
		{
			diagnostic.Clear(DiagnosticType.Error);
		}
	}

	/// <summary>
	/// Parser compatibile a quello di TaskBuilder
	/// Eventuali sintassi che il parser dovrebbe riconoscere per essere compatibile a TaskBuilder
	/// Date value syntax		::= '{' d "<YYYYMMDD>" '}' or '{' d"<DD/MM/YYYY>" '}'
	/// 	/// DateTime value syntax	::= '{' dt"<DD/MM/YYYY HH:MM:SS>" '}'
	/// Time value syntax		::= '{' t "<HH:MM:SS>" '}'
	/// ElapsedTime value syntax	::= '{' et"<DDDDD:HH:MM:SS>" '}'
	/// Guid ::= '{E58EA4AA-0C98-4A3C-B41B-6D212087177C}'
	/// Enum value syntax ::= '{' "<TagEnumName>":"<ItemEnumName>" '}'
	///
	/// Dichiarazione di enumerativi in Woorm:
	///			
	///	ENUM["tagName"]	nomeVariabile;
	///			
	/// </summary>
	//==================================================================================
	public class Parser : IDisposable
	{
		protected internal long totalBytes;
		protected internal long parsedBytes;
		protected internal long scannedLines;
		protected internal SourceType sourceType;
		protected internal Token currentToken = Token.NOTOKEN;

		private static readonly CultureInfo dateFormatProvider = new System.Globalization.CultureInfo("it-IT", true);
		private Stack stateStack = new Stack();
		internal TkSymbolTable userKeywords = new TkSymbolTable();			// Symbol table definita dallo user
		internal TkSymbolTable defines = new TkSymbolTable();
		internal ParserState parserState;
		internal Comment comment = new Comment();
		internal bool preprocessInclude = true;

		public enum SourceType { FromFile, FromString };

		public event EventHandler Start;
		public event EventHandler Progress;
		public event EventHandler Stop;
		public static DateTime NullTbDateTime = new DateTime(1799, 12, 31, 0, 0, 0);


		//------------------------------------------------------------------------------
		public Parser(SourceType src)
		{
			parserState = new ParserState(this);
			sourceType = src;
		}
		//------------------------------------------------------------------------------
		public override string ToString()
		{
			return CurrentLexeme;
		}
		//------------------------------------------------------------------------------
		public bool Open(string src)
		{
			bool ok = true;
			if (Source == SourceType.FromString)
				ok = parserState.parserBuffer.OpenString(src);
			else
			{
				parserState.Filename = src;

				//gli include file devono stare nella stessa dir del file
				parserState.workingFolder = Path.GetDirectoryName(parserState.Filename);
				ok = parserState.parserBuffer.OpenFile(src);
			}

			// informa al gestore esterno di eventi l'inizio del parse
			if (ok && Start != null) Start(this, new EventArgs());
			return ok;
		}

		//------------------------------------------------------------------------------
		public void Close()
		{
			// informa al gestore esterno di eventi la fine del parse
			if (Stop != null) Stop(this, new EventArgs());

			parserState.Close();

			// Ho rilasciato le cose pesanti e quindi posso inibire il finalize
			GC.SuppressFinalize(this);
		}

		//------------------------------------------------------------------------------
		~Parser() { Close(); }

		//------------------------------------------------------------------------------
		internal int LexemeSize { get { return parserState.parserBuffer.LexemeSize; } }
		internal bool Eob { get { return parserState.parserBuffer.Eob; } }

		public bool PreprocessorDisabled { get { return parserState.preprocessorDisabled; } set { parserState.preprocessorDisabled = value; } }
		public TkSymbolTable UserKeywords { get { return userKeywords; } }
		public Diagnostic Diagnostic { get { return parserState.Diagnostic; } }
		public bool Error { get { return parserState.Diagnostic.Error; } }

		public long TotalBytes { get { return totalBytes; } }
		public long ParsedBytes { get { return parsedBytes; } }
		public long ScannedLines { get { return scannedLines; } }
		public SourceType Source { get { return sourceType; } }


		public bool Eof { get { return parserState.parserBuffer.Eof && stateStack.Count == 0; } }

		public long CurrentLine { get { return parserState.parserBuffer.CurrentLine; } }
		public int CurrentPos { get { return parserState.parserBuffer.CurrentPos; } }
		public long CurrentFilePos { get { return parserState.parserBuffer.CurrentFilePos; } }

		public string Filename { get { return parserState.Filename; } }
		public Comment Comment { get { return comment; } }
		public bool PreprocessInclude { get { return preprocessInclude; } set { preprocessInclude = value; } }

		// audit trailing strings function....
		// quando si attiva l'auditing viene conservato tutto ciò che si parsa che può essere ritornato come stringa
		//------------------------------------------------------------------------------
		public bool DoAudit { get { return parserState.parserBuffer.DoAudit; } set { parserState.parserBuffer.DoAudit = value; } }
		public string GetAuditString(bool reset = true) { return parserState.parserBuffer.GetAuditString(reset); }
		public void SetAuditString(string sa) { parserState.parserBuffer.AuditString.Append(sa); }
		public string AuditCurrentLine 
		{
			get
			{
				string s = GetAuditString(false); 
				s = s.Substring(s.LastIndexOf("\n")+1);
				return s; 
			}
		}
		// Utility per gestire i numeri con il corretto Parse...
		//------------------------------------------------------------------------------
		public bool NextTokenIsByte { get { Token token = LookAhead(); return token == Token.BYTE; } }
		public bool NextTokenIsShort { get { Token token = LookAhead(); return token == Token.BYTE || token == Token.SHORT; } }
		public bool NextTokenIsInt { get { Token token = LookAhead(); return token == Token.BYTE || token == Token.SHORT || token == Token.INT; } }
		public bool NextTokenIsLong { get { Token token = LookAhead(); return token == Token.BYTE || token == Token.SHORT || token == Token.INT || token == Token.LONG; } }
		public bool NextTokenIsFloat { get { Token token = LookAhead(); return token == Token.BYTE || token == Token.SHORT || token == Token.INT || token == Token.LONG || token == Token.FLOAT; } }
		public bool NextTokenIsDouble { get { Token token = LookAhead(); return token == Token.BYTE || token == Token.SHORT || token == Token.INT || token == Token.LONG || token == Token.FLOAT || token == Token.DOUBLE; } }

		//------------------------------------------------------------------------------
		public string GetTokenDescription(Token t)
		{
			foreach (DictionaryEntry entry in UserKeywords)
			{ if ((Token)entry.Value == t) return entry.Key.ToString(); }

			string s = Language.GetTokenString(t);
			if (s != Language.UnknownToken)
				return s;

			return t.ToString();
		}

		//------------------------------------------------------------------------------
		internal void ConcatAuditString()
		{
			if (!parserState.parserBuffer.DoAudit)
				return;

			parserState.parserBuffer.AuditString.Append(parserState.currentLexeme + " ");
		}

		//------------------------------------------------------------------------------
		internal Token GetBracketsToken()
		{
			string lexeme = parserState.currentLexeme;
			Token token = Language.GetBracketsToken(lexeme);
			if (token == Token.NOTOKEN)
				parserState.SetError(string.Format(LexanStrings.BracketNotFound, lexeme));

			return token;
		}

		//------------------------------------------------------------------------------
		internal Token GetOperatorsToken()
		{
			string lexeme = parserState.currentLexeme;
			Token token = Language.GetOperatorsToken(lexeme);
			//if (token == Token.NOTOKEN)
			//	parserState.SetError(string.Format(LexanStrings.OperatorNotFound, lexeme));

			return token;
		}

		//------------------------------------------------------------------------------
		internal Token GetNumericToken()
		{
			string lexeme = parserState.currentLexeme;
			parserState.lastNumberIsDouble = false;
			for (int i = 0; i < lexeme.Length; i++)
				if (!char.IsDigit(lexeme[i]))
				{
					parserState.lastNumberIsDouble = true;
					break;
				}

			try
			{
				parserState.lastNumber = double.Parse(lexeme, NumberFormatInfo.InvariantInfo);
			}

			catch (System.FormatException e)
			{
				SetError(lexeme + " " + e.Message);
				parserState.lastNumber = 0.0;
				return Token.NOTOKEN;
			}

			return ConvertNumericToken(parserState.lastNumber, parserState.lastNumberIsDouble);
		}

		//------------------------------------------------------------------------------
		internal Token GetHexNumericToken()
		{
			string hexString = parserState.currentLexeme;
			long hexValue = 0;

			char[] x = "xX".ToCharArray(0, 2);
			int i = hexString.IndexOfAny(x);
			if (i >= 0)
			{
				if (i >= hexString.Length - 1) hexValue = 0L;
				else
				{
					string hex = hexString.Substring(i + 1, hexString.Length - i - 1);
					hexValue = long.Parse(hex, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo);
				}
			}
			else
				hexValue = long.Parse(hexString, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo);


			parserState.lastNumber = (double)hexValue;
			parserState.lastNumberIsDouble = false;

			return ConvertNumericToken(parserState.lastNumber, parserState.lastNumberIsDouble);
		}

		//------------------------------------------------------------------------------
		internal Token GetIdToken()
		{
			string lexeme = parserState.currentLexeme;
			// Normal ID processing (no DEFINE)		
			Token token;

			// user defined keyword (if any) can also override standard
			token = Language.GetTokenFromMap(userKeywords, lexeme);
			if (token != Token.NOTOKEN)
				return token;

			// standard language keyword
			token = Language.GetKeywordsToken(lexeme);
			if (token != Token.NOTOKEN)
				return token;

			// le define sono solo numeriche intere
			if (!parserState.preprocessorDisabled)
			{
				double aValue;
				if (GetDefine(lexeme, out aValue))
				{
					parserState.lastNumber = aValue;
					parserState.lastNumberIsDouble = false;

					return ConvertNumericToken(parserState.lastNumber, parserState.lastNumberIsDouble);
				}
			}

			return Token.ID;
		}

		//------------------------------------------------------------------------------
		internal Token ConvertNumericToken(double aValue, bool lastNumberIsDouble)
		{
			if (lastNumberIsDouble)
			{
				if (aValue <= 9999999.0F) return Token.FLOAT;
				return Token.DOUBLE;
			}

			if (aValue <= byte.MaxValue) return Token.BYTE;
			else if (aValue <= short.MaxValue) return Token.SHORT;
			else if (aValue <= int.MaxValue) return Token.INT;
			else if (aValue <= long.MaxValue) return Token.LONG;
			else if (aValue <= 9999999.0F) return Token.FLOAT;

			return Token.DOUBLE;
		}

		//------------------------------------------------------------------------------
		internal void ProcessComment()
		{
			parserState.fsmState = Fsm.State.START;

			comment.tabSize = parserState.parserBuffer.LexemeStart + 1;
			comment.Add(parserState.parserBuffer.Comment);
			LoadBuffer();
		}

		//------------------------------------------------------------------------------
		internal bool GetNewLexeme()
		{
			if (!parserState.currentUndefined)
				return false;

			Fsm.CharClass character;
			Fsm.State fsmState = Fsm.State.START;
			Fsm.Action action = Fsm.Action.LAST;
			char i;

			do
			{
				comment.Clear(); // pulisce il commento prima della ricerca del prossimo token
				parserState.parserBuffer.Rewind();
				do
				{
					// carica il buffer se è vuoto e non sono in fondo al file
					while (!parserState.parserBuffer.Eof && Eob) LoadBuffer();
					if (!parserState.parserBuffer.Eof)
					{
						fsmState = parserState.fsmState;			// starting state
						i = parserState.parserBuffer.GetNextChar();	// advance the lexeme end character in the buffer

						character = Fsm.CharClassTable.Class(i);									// determinate class character
						parserState.fsmState = Fsm.TransitionTable[(int)fsmState, (int)character];	// determinate the new state of FSM
						action = Fsm.EmitTable[(int)fsmState, (int)character];						// action corresponding to new state

						// commento sino a fine riga ignora il resto della riga e ricarica il buffer
						if (fsmState == Fsm.State.CMTEOL)
							ProcessComment();

						// skip the lexeme advancing the starting pointer
						if (action == Fsm.Action.SKIP)
							parserState.parserBuffer.Rewind();
					}
				}
				while ((action != Fsm.Action.LAST) && !parserState.parserBuffer.Eof);
			}
			while (((fsmState == Fsm.State.CMNT) || (fsmState == Fsm.State.ENDCMT) || (fsmState == Fsm.State.LSTCMT)) && !Eof);

			parserState.currentUndefined = false;
			if (parserState.parserBuffer.Eof)
			{
				// se ho ancora qualcosa in stack vuol dire che sono in un file incluso e quindi
				// non devo dare eof ma devo tornare al file includente skippando il token di fine file
				if (stateStack.Count > 0)
				{
					PopState();
					GetNewLexeme();
				}
				else
				{
					parserState.currentState = Fsm.State.ENDF;
					parserState.currentLexeme = "";
				}
			}
			else
			{
				parserState.currentState = fsmState;
				parserState.currentLexeme = parserState.parserBuffer.Lexeme;
			}
			return true;
		}

		//------------------------------------------------------------------------------
		internal void PopState()
		{
			Close(); // chiude il corrente parser perche' ha finito il suo lavoro
			parserState = (ParserState)stateStack.Pop();
			Debug.Assert(parserState != null);
		}

		//------------------------------------------------------------------------------
		internal void PushState()
		{
			stateStack.Push(parserState);
			parserState = new ParserState(this);
		}

		//------------------------------------------------------------------------------
		internal void SetWorkingFolder(string szWorkingFolder)
		{ parserState.workingFolder = szWorkingFolder; }

		//------------------------------------------------------------------------------
		internal string GetCurrentStringToken()
		{
			string strCurrentToken = parserState.currentLexeme;
			return strCurrentToken;
		}

		//------------------------------------------------------------------------------
		internal void AddDefine(string key, double aValue)
		{
			// bisogna controllare che non collida
			if (defines.Contains(key))
				defines.Remove(key);

			defines.Add(key, aValue);
		}

		//------------------------------------------------------------------------------
		internal void RemoveDefine(string key)
		{
			defines.Remove(key);
		}

		//------------------------------------------------------------------------------
		public bool GetDefine(string strKey, out double aValue)
		{
			aValue = 0;
			Object o = defines[strKey];
			if (o != null) aValue = (double)o;

			return o != null;
		}

		//------------------------------------------------------------------------------
		internal void LoadBuffer()
		{
			// informa al gestore esterno che ha letto una riga
			if (Progress != null) Progress(this, new EventArgs());

			parserState.currentUndefined = true;
			if (
					parserState.parserBuffer.LoadBuffer() &&
					!parserState.parserBuffer.Eof &&
					!PreprocessorDisabled &&
					parserState.parserBuffer.buffer.Length > 1 &&
					parserState.parserBuffer.buffer[0] == '#'
				)
			{
				Preprocessor preprocessor = new Preprocessor(this);

				// lascia al parser corrente l'onere di gestire la direttiva
				// di #include se il comportamento e'stato abilitato da chi
				// istanzia il parser originario.
				if (preprocessor.ParseBuffer(parserState.parserBuffer.buffer))
					parserState.parserBuffer.SkipBuffer();
			}
		}

		//------------------------------------------------------------------------------
		internal bool BadNumer(Token expectedToken, Token foundToken)
		{
			parserState.currentUndefined = true;
			parserState.SetError(string.Format(LexanStrings.BadTokenFound, expectedToken, foundToken));

			return false;
		}

		//------------------------------------------------------------------------------
		internal short GetSign()
		{
			short sign = 1;
			if (LookAhead(Token.MINUS))
			{
				SkipToken();
				sign = -1;
			}
			else if (LookAhead(Token.PLUS))
				SkipToken();

			return sign;
		}

		//------------------------------------------------------------------------------
		public void ClearErrors() { parserState.ClearError(); }

		//------------------------------------------------------------------------------
		public void SetError(string explain)
		{
			parserState.SetError(explain);
		}

		//------------------------------------------------------------------------------
		public void SetError(string explain, long line, int column)
		{
			parserState.SetError(explain, line, column);
		}

		//------------------------------------------------------------------------------
		public void SetWarning(string explain)
		{
			parserState.SetWarning(explain);
		}

		//------------------------------------------------------------------------------
		public string CurrentLexeme { get { return parserState.currentLexeme; } }

		//------------------------------------------------------------------------------
		public bool LookAhead(Token aToken)
		{
			return LookAhead() == aToken;
		}

		//------------------------------------------------------------------------------
		public Token LookAhead()
		{
			if (!GetNewLexeme())
				return currentToken;

			/* CMNT, EXP, STRING, SSTRING: no terminal state */
			switch (parserState.currentState)
			{
				case Fsm.State.START: currentToken = Token.NOTOKEN; break;
				case Fsm.State.ID: currentToken = GetIdToken(); break;
				case Fsm.State.DECSEP: currentToken = GetIdToken(); break;
				case Fsm.State.ZERONUM:
				case Fsm.State.NUM: currentToken = GetNumericToken(); break;
				case Fsm.State.HEXNUM: currentToken = GetHexNumericToken(); break;
				case Fsm.State.OPER: currentToken = GetOperatorsToken(); break;
				case Fsm.State.ENDSTR: currentToken = Token.TEXTSTRING; break;
				case Fsm.State.ENDSSTR: currentToken = Token.TEXTSTRING; break;
				case Fsm.State.SEP: currentToken = Token.SEP; break;
				case Fsm.State.ENDCMT: currentToken = Token.CMT; break;
				case Fsm.State.BRACK: currentToken = GetBracketsToken(); break;
				case Fsm.State.BEGCMT: currentToken = GetOperatorsToken(); break;
				case Fsm.State.ENDF: currentToken = Token.EOF; break;
				default:
					{
						currentToken = Token.NOTOKEN;
						throw (new Exception("Parser : FSM error. " + parserState.currentState.ToString() + " Bad State!"));
					}
			} // switch

			return currentToken;
		}

		//------------------------------------------------------------------------------
		public Token SkipToken()
		{
			Token token = LookAhead();
			parserState.currentUndefined = true;

			ConcatAuditString();
			return token;
		}

		//------------------------------------------------------------------------------
		public bool SkipLine()
		{
			LoadBuffer();

			return true;
		}

		//--------------------------------------------------------------------------------
		public bool SkipBlock(Token startToken, Token endToken)
		{
			if (!ParseTag(startToken)) return false;
			int openBlock = 1;
			while (openBlock > 0)
			{
				if (LookAhead() == endToken)
					openBlock--;

				if (LookAhead() == startToken)
					openBlock++;

				SkipToken();

				if (LookAhead() == Token.EOF)
					return false;
			}
			return true;
		}
		//--------------------------------------------------------------------------------
		public bool SkipBlock()
		{
			return SkipBlock(Token.BEGIN, Token.END);
		}

		//--------------------------------------------------------------------------------
		public bool SkipToToken(Token token)
		{
			return SkipToToken(token, false, false);
		}

		//--------------------------------------------------------------------------------
		public bool SkipToToken(Token token, bool skipToken, bool skipInnerBlock)
		{
			return SkipToToken(new Token[] { token }, skipToken, skipInnerBlock);
		}

		//--------------------------------------------------------------------------------
		public bool SkipToToken(Token[] tokens, bool skipToken, bool skipInnerBlock)
		{
			return SkipToToken(tokens, skipToken, skipInnerBlock, false);
		}

		//--------------------------------------------------------------------------------
		public bool SkipToToken(Token[] tokens, bool skipToken, bool skipInnerBlock, bool skipInnerRound)
		{
			while (!LookAhead(Token.EOF) && !Error)
			{
				if (skipInnerBlock && LookAhead(Token.BEGIN))
				{
					if (!SkipBlock()) return false;
				}
				else if (skipInnerRound && LookAhead(Token.ROUNDOPEN))
				{
					if (!SkipBlock(Token.ROUNDOPEN, Token.ROUNDCLOSE)) return false;
				}
				else
				{
					if (LookAhead(tokens))
					{
						if (skipToken) SkipToken();
						return true;
					}

					SkipToken();
				}
			}
			return !Error;
		}

		//---------------------------------------------------------------------------------------------------
		public bool LookAhead(Token[] tokens)
		{
			foreach (Token t in tokens)
				if (LookAhead() == t)
					return true;
			return false;
		}

		//------------------------------------------------------------------------------
		public bool ParseComment(out Comment aComment)
		{
			aComment = comment;
			return true;
		}

		//------------------------------------------------------------------------------
		public bool ParseTag(Token aToken)
		{
			Token token;

			token = LookAhead();
			parserState.currentUndefined = true;

			ConcatAuditString();

			if (token != aToken)
			{
				parserState.SetError(string.Format(LexanStrings.TokenNotFound, GetTokenDescription(aToken), GetTokenDescription(token)));
				return false;
			}

			return true;
		}

		//------------------------------------------------------------------------------
		public bool Parsed(Token tk)
		{
			if (LookAhead() != tk)
				return false;

			parserState.currentUndefined = true;
			ConcatAuditString();

			return true;
		}

		//------------------------------------------------------------------------------
		public bool ParseID(out string aString)
		{
			aString = "";
			if (!ParseTag(Token.ID)) return false;

			aString = parserState.currentLexeme;
			return true;
		}

		//------------------------------------------------------------------------------
		public bool ParseBool(out bool aBool)
		{
			aBool = false;
			if (Parsed(Token.TRUE))
			{
				aBool = true;
				return true;
			}

			if (Parsed(Token.FALSE))
			{
				aBool = false;
				return true;
			}

			// cosi` da il messaggio "Aspettato Boolean incontrato ...."
			return ParseTag(Token.BOOL);
		}

		//------------------------------------------------------------------------------
		public bool ParseByte(out byte aByte)
		{
			aByte = 0;
			if (!NextTokenIsByte)
				return BadNumer(Token.BYTE, LookAhead());

			SkipToken();
			aByte = (byte)parserState.lastNumber;
			return true;
		}

		//------------------------------------------------------------------------------
		public bool ParseShort(out short aWord)
		{
			aWord = 0;
			if (!NextTokenIsShort)
				return BadNumer(Token.SHORT, LookAhead());

			SkipToken();
			aWord = (short)parserState.lastNumber;
			return true;
		}

		//------------------------------------------------------------------------------
		public bool ParseUShort(out ushort aWord)
		{
			aWord = 0;
			if (!NextTokenIsShort)
				return BadNumer(Token.SHORT, LookAhead());

			SkipToken();
			aWord = (ushort)parserState.lastNumber;
			return true;
		}

		//------------------------------------------------------------------------------
		public bool ParseInt(out int aInt)
		{
			aInt = 0;
			if (!NextTokenIsInt)
				return BadNumer(Token.INT, LookAhead());

			SkipToken();
			aInt = (int)parserState.lastNumber;
			return true;
		}

		//------------------------------------------------------------------------------
		public bool ParseLong(out long aLong)
		{
			aLong = 0;
			if (!NextTokenIsLong)
				return BadNumer(Token.LONG, LookAhead());

			SkipToken();
			aLong = (long)parserState.lastNumber;
			return true;
		}

		//------------------------------------------------------------------------------
		public bool ParseFloat(out float aFloat)
		{
			aFloat = 0.0F;
			if (!NextTokenIsFloat)
				return BadNumer(Token.FLOAT, LookAhead());

			SkipToken();
			aFloat = (float)parserState.lastNumber;
			return true;
		}

		//------------------------------------------------------------------------------
		public bool ParseDouble(out double aDouble)
		{
			aDouble = 0.0;
			if (!NextTokenIsDouble)
				return BadNumer(Token.DOUBLE, LookAhead());

			SkipToken();
			aDouble = parserState.lastNumber;
			return true;
		}

		//------------------------------------------------------------------------------
		public bool ParseFlatString(out string aString)
		{
			aString = "";
			if (!ParseTag(Token.TEXTSTRING)) return false;
			aString = parserState.currentLexeme;
			if (aString.Length < 2)
				return false;
			aString = aString.Substring(1, aString.Length - 2);

			return true;
		}

		//------------------------------------------------------------------------------
		public bool ParseString(out string str)
		{
			str = "";
			if (!ParseFlatString(out str))
				return false;

			// trasforma tutte le sequenze di "" in "
			int pos = 0;
			while (pos >= 0)
			{
				pos = str.IndexOf("\"\"", pos);
				if (pos >= 0)
					str = str.Remove(pos++, 1);
			}
			// trasforma tutte le sequenze di '' in '
			pos = 0;
			while (pos >= 0)
			{
				pos = str.IndexOf("\'\'", pos);
				if (pos >= 0)
					str = str.Remove(pos++, 1);
			}

			return true;
		}

		//------------------------------------------------------------------------------
		public bool ParseSignedShort(out short aVal)
		{
			aVal = 0; short sign = GetSign();
			bool ok = ParseShort(out aVal);
			aVal *= sign;
			return ok;
		}

		//------------------------------------------------------------------------------
		public bool ParseSignedInt(out int aVal)
		{
			aVal = 0; short sign = GetSign();
			bool ok = ParseInt(out aVal);
			aVal *= sign;
			return ok;
		}

		//------------------------------------------------------------------------------
		public bool ParseSignedLong(out long aVal)
		{
			aVal = 0L; short sign = GetSign();
			bool ok = ParseLong(out aVal);
			aVal *= sign;
			return ok;
		}

		//------------------------------------------------------------------------------
		public bool ParseSignedFloat(out float aVal)
		{
			aVal = 0.0F; short sign = GetSign();
			bool ok = ParseFloat(out aVal);
			aVal *= sign;
			return ok;
		}

		//------------------------------------------------------------------------------
		public bool ParseSignedDouble(out double aVal)
		{
			aVal = 0.0; short sign = GetSign();
			bool ok = ParseDouble(out aVal);
			aVal *= sign;
			return ok;
		}

		//------------------------------------------------------------------------------
		public bool ParseAlias(out ushort id)
		{
			id = 0;
			return ParseTag(Token.ALIAS) && ParseUShort(out id);
		}

		//------------------------------------------------------------------------------
		public bool ParseItem(out string itemName)
		{
			itemName = "";
			return
				ParseOpen() &&
				ParseID(out itemName) &&
				ParseClose();
		}

		//------------------------------------------------------------------------------
		public bool ParseSubscr(out int aVal)
		{
			aVal = 0;
			return
				ParseSquareOpen() &&
				ParseInt(out aVal) &&
				ParseSquareClose();
		}

		//------------------------------------------------------------------------------
		public bool ParseSubscr(out int aVal1, out int aVal2)
		{
			aVal1 = 0;
			aVal2 = 0;
			bool ok =
				ParseSquareOpen() &&
				ParseInt(out aVal1);

			if (ok && Parsed(Token.COMMA))
				ok = ParseInt(out aVal2);
			else
				aVal2 = 0;

			ok = ok && ParseSquareClose();

			return ok;
		}

		//------------------------------------------------------------------------------
		public bool ParseGUID(out Guid aGuid)
		{
			// dummy GUID
			string strGUID = "0000000a-000b-000c-0001-020304050607";
			bool ok =
				ParseTag(Token.UUID) &&
				ParseString(out strGUID);

			aGuid = new Guid(strGUID);
			return ok;
		}

		//------------------------------------------------------------------------------
		public bool ParseColor(Token token, out Color aColor)
		{
			int nRed = 0;
			int nGreen = 0;
			int nBlue = 0;

			bool ok1 = token == Token.NULL ? true : ParseTag(token);

			bool ok = ok1 &&
				//ParseTag	(token)		&&
				ParseOpen() &&
				ParseInt(out nRed) &&
				ParseComma() &&
				ParseInt(out nGreen) &&
				ParseComma() &&
				ParseInt(out nBlue) &&
				ParseClose();

			aColor = Color.FromArgb(nRed, nGreen, nBlue);
			return ok;
		}

		//------------------------------------------------------------------------------
		public bool ParseBegin() { return ParseTag(Token.BEGIN); }
		public bool ParseEnd() { return ParseTag(Token.END); }
		public bool ParseComma() { return ParseTag(Token.COMMA); }
		public bool ParseColon() { return ParseTag(Token.COLON); }
		public bool ParseSep() { return ParseTag(Token.SEP); }
		public bool ParseBraceOpen() { return ParseTag(Token.BRACEOPEN); }
		public bool ParseBraceClose() { return ParseTag(Token.BRACECLOSE); }
		public bool ParseSquareOpen() { return ParseTag(Token.SQUAREOPEN); }
		public bool ParseSquareClose() { return ParseTag(Token.SQUARECLOSE); }
		public bool ParseOpen() { return ParseTag(Token.ROUNDOPEN); }
		public bool ParseClose() { return ParseTag(Token.ROUNDCLOSE); }

		//------------------------------------------------------------------------------
		public bool ParseCEdit(out string text)
		{
			bool bBeginFound = Parsed(Token.BEGIN);

			if (!ParseString(out text))
				return false;
			Parsed(Token.PLUS);
			while (LookAhead(Token.TEXTSTRING) && !Diagnostic.Error && !Eof)
			{
				string strBuffer;
				ParseString(out strBuffer);
				text += "\r\n" + strBuffer;
				Parsed(Token.PLUS);
			}

			return !(bBeginFound && !ParseEnd());
		}

		//------------------------------------------------------------------------------
		public enum ComplexDataType { Unknown, DataEnum, Date, DateTime, Time, TimeSpan };

		//------------------------------------------------------------------------------
		public ComplexDataType ComplexData()
		{
			if (LookAhead(Token.ID))
			{
				string tag = parserState.currentLexeme.ToLower(CultureInfo.InvariantCulture);
				switch (tag)
				{
					case "d": return ComplexDataType.Date;
					case "dt": return ComplexDataType.DateTime;
					case "t": return ComplexDataType.Time;
					case "et": return ComplexDataType.TimeSpan;
				}
				return ComplexDataType.Unknown;
			}

			if (LookAhead
						(
							new Token[] 
							{
								Token.TEXTSTRING,
								Token.INT, 
								Token.SBYTE,
								Token.BYTE,
								Token.SHORT,
								Token.USHORT,
								Token.INT,
								Token.UINT,
								Token.LONG
							}
						)
				)
				return ComplexDataType.DataEnum;

			return ComplexDataType.Unknown;
		}

		//------------------------------------------------------------------------------
		public bool ParseDateTime(out DateTime dt)
		{
			string strDate = "";
			dt = DateTime.Today;

			SkipToken();
			if (!ParseString(out strDate))
				return false;

			// per compatibilità con TB c++ deve considerare la stringa vuota come
			// la data nulla di TB (vedi dataobj.cpp)
			if (strDate == "")
			{
				dt = NullTbDateTime;
				return true;
			}

			//prima provo la sintassi senza trattini (20093112) (si veda an. 15065)
			if (DateTime.TryParseExact(strDate, "yyyymmdd",
						dateFormatProvider,
						System.Globalization.DateTimeStyles.AllowWhiteSpaces,
						out dt
					))
				return true;

			if (DateTime.TryParse
					(
						strDate,
						dateFormatProvider,
						System.Globalization.DateTimeStyles.AllowWhiteSpaces,
						out dt
					))
				return true;

			SetError(string.Format(LexanStrings.InvalidDateTimeFormat, strDate));
			return false;
		}

		// ElapsedTime value syntax	::= et"<DDDDD:HH:MM:SS>"
		//------------------------------------------------------------------------------
		public bool ParseTimeSpan(out TimeSpan ts)
		{
			string strTimeSpan = "";
			ts = TimeSpan.MinValue;

			SkipToken();
			if (!ParseString(out strTimeSpan))
				return false;

			if (strTimeSpan == "")
				return true;

			// la sintassi gestita da parse e' DDDD.HH:MM:SS
			int pos = strTimeSpan.IndexOf(':');
			if (pos > 0)
				strTimeSpan =
					strTimeSpan.Substring(0, pos) + '.' +
					strTimeSpan.Substring(pos + 1, strTimeSpan.Length - (pos + 1));

			ts = TimeSpan.Parse(strTimeSpan);
			return true;
		}

		// 
		/// <summary>
		/// Elabora una string come tipo dato DataEnum con la sintassi '{"TagEnumName":"ItemEnumName"}'
		/// </summary>
		/// <param name="tagName">Nome del Tag	(es: "Colore")</param>
		/// <param name="itemName">Nome dell'Item (es: "Rosso")</param>
		/// <returns></returns>
		//------------------------------------------------------------------------------
		public bool ParseDataEnum(out string tagName, out string itemName)
		{
			tagName = "";
			itemName = "";
			bool ok =
				ParseString(out tagName) &&
				ParseTag(Token.COLON) &&
				ParseString(out itemName);

			return ok;
		}
		// 
		/// <summary>
		/// Elabora una string come tipo dato DataEnum con la sintassi '{nn:mm}'
		/// </summary>
		/// <param name="tagValue">Valore del Tag</param>
		/// <param name="itemValue">Valore dell'Item</param>
		/// <returns></returns>
		//------------------------------------------------------------------------------
		public bool ParseDataEnum(out int tagValue, out int itemValue)
		{
			tagValue = 0;
			itemValue = 0;
			bool ok =
				ParseInt(out tagValue) &&
				ParseTag(Token.COLON) &&
				ParseInt(out itemValue);

			return ok;
		}

		#region IDisposable Members

		public void Dispose()
		{
			Close();
		}

		#endregion
	}
}
