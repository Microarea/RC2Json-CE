using RC2Json.Lexan;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace RC2Json
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	/// ================================================================================
	internal class ParserBuffer : IDisposable
	{
		private bool disposed = false;	// Track whether Dispose has been called.
		private const int	EOF = -1;
		private bool ioError = false;

		protected Parser	parser;			// parent parser che utilizza il presente buffer
		protected int		bufferLen;		// the length of the current buffer read from file or string
		protected int		start;			// point to lexeme first character
		protected int		finish;			// point to lexem end character
		protected long		lineNumber;		// current line of the file (or string) scanned
		protected bool		doAudit;		// abilita l'auditing di quanto parsato
        protected long      filePos = 0;		// current line of the file (or string) scanned

		protected StreamReader	inputFile;		// file da compilare
		protected StringReader	inputString;	// stringa da compilare

		public StringBuilder	auditString;	// accumula i token parsati se auditing è abilitato
		public char[]			buffer;			// buffer per lo scan dei singoli caratteri

		//------------------------------------------------------------------------------
		public ParserBuffer (Parser	parser)
		{
			this.parser	= parser;
			Init();
		}		

		//------------------------------------------------------------------------------
		public StringBuilder AuditString
		{
			get
			{
				if (auditString == null)
					auditString = new StringBuilder();
				return auditString;
			}
				
		}

		//------------------------------------------------------------------------------
		private void Init()
		{
			inputFile	= null;
			inputString	= null;
			buffer		= null;
			bufferLen	= -2;
			start		= -1;
			finish		= -1;
			lineNumber	= 0;
			doAudit = false;
			auditString = null;
		}		

		//------------------------------------------------------------------------------
		public bool OpenString(string parseString)
		{           
			Debug.Assert(inputString == null);
			Debug.Assert(inputFile == null);

			try
			{
				disposed = false;

				// formato della stringa "xxx\nyyy\n\0"
				inputString = new StringReader(parseString);

				// Incrementa ricorsivamente la quantità di byte perchè potrebbero esserci inclusioni multiple
				parser.totalBytes += parseString.Length;
			}
			catch (IOException e)
			{
				if (inputString != null)
					inputString.Close(); 

				ioError = true;
				parser.SetError(e.ToString());
				return false;
			}

			return true;
		}

		//------------------------------------------------------------------------------
		public bool OpenFile(string parseFilename)
		{       
			Debug.Assert(inputString == null);
			Debug.Assert(inputFile == null);

			try
			{
				disposed = false;

				FileInfo fi = new FileInfo(parseFilename);
				if (!fi.Exists)
				{
					parser.SetError(LexanStrings.InvalidFileName);
					return false;
				}
				FileStream fs = new FileStream(parseFilename, FileMode.Open, FileAccess.Read, FileShare.Read);
				inputFile = new StreamReader(fs, System.Text.Encoding.GetEncoding(0));

				// Incrementa ricorsivamente la quantità di byte perchè potrebbero esserci inclusioni multiple
				parser.totalBytes += fi.Length;
			}
			catch (IOException e)
			{
				if (inputFile != null) inputFile.Close();
				ioError = true;
				parser.SetError(e.ToString());
				return false;
			}

			return true;
		}

		//------------------------------------------------------------------------------
		~ParserBuffer()	
		{
			Dispose(false);
		}

		//------------------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//------------------------------------------------------------------------------
		protected virtual void Dispose(bool disposing)
		{
			if(!disposed)
			{
				// se arrivo dal distruttore non so l'ordine di distruzione
				if(disposing)
				{
					if (inputFile != null)		inputFile.Close();
					if (inputString != null)	inputString.Close();

					Init();
				}

			}
			disposed = true;         
		}

		//------------------------------------------------------------------------------
		public void Close()
		{
			Dispose(true);
		}

		//------------------------------------------------------------------------------
		internal long	ScannedLines	{ get { return lineNumber - 1; }}
		internal bool	Eob				{ get { return finish >= bufferLen - 1; }}
		internal bool	Eof				{ get { return bufferLen == EOF; }}
		internal int	LexemeSize		{ get { return finish - start; }}
		internal int	LexemeStart		{ get { return start; }}
		internal long	CurrentLine		{ get { return lineNumber; }}
		internal int	CurrentPos		{ get { return start; }}
		internal bool	DoAudit			{ get { return doAudit; } set { doAudit = value; }}

        internal long CurrentFilePos { get { return filePos; } }

		internal void	Rewind			() { start = finish; }
        internal char GetNextChar() { finish++; filePos++;  return buffer[finish]; }

		//------------------------------------------------------------------------------
		internal string GetAuditString (bool reset)	
		{ 
			string s = AuditString.ToString();
			if (reset)
				AuditString.Length = 0;

			return s;
		}

		//------------------------------------------------------------------------------
		internal string	Lexeme
		{
			get 
			{ 
				if (buffer != null && start >= 0 && (finish - start) > 0)
					return new string(buffer, start, finish - start); 

				return "";
			}
		}

		//------------------------------------------------------------------------------
		internal string	Comment
		{
			get 
			{ 
				// elimina il cr-lf finale aggiunto durante la lettura del buffer
				int length = buffer.Length - start - 2;
				if (buffer != null && start >= 0 && length > 0)
					return new string(buffer, start, length); 
				
				return "";
			}
		}

		//------------------------------------------------------------------------------
		internal void SkipBuffer() 
		{ 
			// condizione di eob
			finish = bufferLen - 1; 
		}
	
		//------------------------------------------------------------------------------
		internal bool LoadBuffer()
		{
			// se c'è un errore di IO, imposto Eof a true per evitare loop del parser
			if (ioError) 
			{
				bufferLen = EOF;
				return false;
			}

			// control end of file exception
			if (Eof) return false;

			// reset lexeme pointers
			finish = -1;
			start = -1;

			// count processed lines.
			lineNumber++;
		
			if (Eof) return true;
			try
			{
				string readBuffer;
				readBuffer = (inputFile != null) ? inputFile.ReadLine() : inputString.ReadLine();

				// eof condition
				if (readBuffer == null)
				{
					bufferLen = EOF;
					return true;
				}

				// devo aggiungere il fine buffer per il lexan (cr-lf)
				readBuffer  += "\r\n";
				bufferLen = readBuffer.Length;
				parser.parsedBytes += bufferLen;
				buffer = new char[bufferLen]; 
				readBuffer.CopyTo(0, buffer, 0, bufferLen);

				// aggiunge cr-lf che vengono mangiati dall'analisi
				if (this.DoAudit)
					this.AuditString.Append("\r\n");
			}
			catch (IOException e)
			{
				ioError = true;
				parser.SetError(e.ToString());
				return false;
			}

			return true;
		}
	}
}
