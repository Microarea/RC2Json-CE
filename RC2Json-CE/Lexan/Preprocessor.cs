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

using RC2Json.Lexan;
using System;
using System.IO;

namespace RC2Json
{

	/// <summary>
	/// Summary description for Preprocessor.
	/// </summary>
	/// ================================================================================
	internal class Preprocessor
	{
		protected Parser ownedParser;
	
		//------------------------------------------------------------------------------
		public Preprocessor(Parser parser) { ownedParser = parser;}

		/// <summary>
		/// abilita sempre il consumo del buffer
		/// </summary>
		//------------------------------------------------------------------------------
		public void ParseDefine(Parser parser)
		{
			string	defineName;
			
			parser.SkipToken();	// skip DEFINE
			parser.ParseID(out defineName);// determina nome della define

			// ammette solo define numeriche intere e non ID o altro
			Token token = parser.LookAhead();
			switch (token)
			{
				case Token.BYTE	:
				case Token.SHORT	:
				case Token.INT		:
				case Token.LONG	:
				case Token.FLOAT	:
				case Token.DOUBLE	:
				{
					double aValue;
					parser.ParseDouble(out aValue); 
					ownedParser.AddDefine(defineName, aValue);
					break;
				}
				case Token.EOF	:
				case Token.NOTOKEN	:
					// empty define use 1 as default value
					ownedParser.AddDefine(defineName, 1L);
					break;

				default:
				{
					// define value not allowed
					ownedParser.SetError(string.Format(LexanStrings.InvalidDefine, defineName));
					break;
				}
			}
		}

		//------------------------------------------------------------------------------
		public void ParseUndef(Parser parser)
		{
			parser.SkipToken();	// skip UNDEF

			// determina il nome della define ed ignora il resto della riga
			string defineName;
			if (parser.ParseID(out defineName))
				ownedParser.RemoveDefine(defineName);
		}

		/// <summary>
		/// il nome del file deve essere incluso come "path\filename.ext" in modo da poterlo ricercare
		/// nella directory path oppure (se non presente) nella directory dove è presente il file che lo 
		/// include.
		/// <summary>
		//------------------------------------------------------------------------------
		public void ParseInclude(Parser parser)
		{
			parser.SkipToken();	// skip INCLUDE

			string includeFile;
			if (!parser.ParseString(out includeFile) || (includeFile.Length == 0)) 
			{
				ownedParser.SetError(LexanStrings.InvalidPreprocessorFile);
				return;
			}

			try
			{
				if (!Path.IsPathRooted(includeFile))
					includeFile = Path.Combine(ownedParser.parserState.workingFolder, includeFile);
			}
			catch(Exception e) 
			{
				ownedParser.SetError(LexanStrings.InvalidFileName + Environment.NewLine + e.Message);
				return;
			}

			//vado in ricorsione per parsare il file incluso per prima cosa salvo lo stato attuale del parser 
			ownedParser.PushState();

			// adesso ownedParser  è il nuovo e non quello che ho messo nello stack.
			// apro il file e lo parso (in caso di errore viene chiuso in automatico dalla PopState
			if (!ownedParser.Open(includeFile))
			{
				ownedParser.PopState();
				ownedParser.SetError(string.Format(LexanStrings.CannotInclude, includeFile));
			}
		}

		/// <summary>
		/// gestisce solo DEFINE,UNDEF,INCLUDE. La gestione delle inclusione è accettata solo
		/// se si stà parsando un file e non una stringa. 
		/// <summary>
		//------------------------------------------------------------------------------
		public bool ParseBuffer(char[] buffer)
		{
			bool skipBuffer = true;
			string strBuffer = new string(buffer, 1, buffer.Length - 1);
			Parser	parser = new Parser(Parser.SourceType.FromString); 
			
			// passo le user keywords dell'owner parser al  nested parser
			foreach (object aKey in ownedParser.UserKeywords.Keys)
				parser.UserKeywords.Add (aKey, ownedParser.UserKeywords[aKey]);
			
			parser.Open(strBuffer);
			parser.PreprocessorDisabled = true;

			Token token = parser.LookAhead();
			switch (token)
			{
				case Token.DEFINE:		ParseDefine(parser);	break;
				case Token.UNDEF:		ParseUndef(parser);		break;
				case Token.INCLUDE:	
					if (ownedParser.Source == Parser.SourceType.FromFile) 
						ParseInclude(parser);	
					else
						skipBuffer = ownedParser.PreprocessInclude;
					break;

				default :
					ownedParser.SetError(LexanStrings.InvalidPreprocessorDirective);
					break;
			}
			parser.Close();
			return skipBuffer;
		}
	}

}
