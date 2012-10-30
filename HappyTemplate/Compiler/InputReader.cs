using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Scripting;

namespace HappyTemplate.Compiler
{
	class InputReader
	{
		private readonly SourceUnit _sourceUnit;
		readonly TextReader _reader;
		int _lineNo = 1;
		int _columnNo = 1;
		bool _eof;
		private int _index = 0;
		readonly List<Character> _queue = new List<Character>();

		public SourceLocation StartLocation { get; private set; }

		public SourceUnit SourceUnit { get { return _sourceUnit;} } 
       
		public bool Eof  { get { return _eof; }  }

		public InputReader(SourceUnit sourceUnit)
		{
			_sourceUnit = sourceUnit;
			_reader = sourceUnit.GetReader();
		}

		public void ResetStartLocation()
		{
			this.StartLocation = CurrentLocation;
		}

		public HappySourceLocation GetHappySourceLocation()
		{
			return new HappySourceLocation(_sourceUnit, this.StartLocation, this.CurrentLocation);
		}
	
		public SourceLocation CurrentLocation
		{
			get
			{
				if(_queue.Count == 0)
					return new SourceLocation(_index, _lineNo, _columnNo);

				Character c = _queue[0];
				return new SourceLocation(c.Index, c.LineNo, c.ColumnNo);
			}
		}

		public char Read()
		{
			char retval;
			if(_queue.Count == 0)
				retval = Extract().C;
			else
			{
				Character r = _queue[0];
				_queue.RemoveAt(0);
				retval = r.C;
			}

			if(this.Peek(0) == 0xFFFF)
				_eof = true;

			return retval;
		}

		private Character Extract()
		{
			int readChar = _reader.Read();

			Character retval = new Character(_index, _lineNo, _columnNo, _sourceUnit.Path, (char)readChar);

			if (retval.C == '\n')
			{
				_lineNo++;
				_columnNo = 1;
			}
			else
				_columnNo++;

			return retval;
		}

		public char Peek()
		{
			return Peek(0);
		}

		public char Peek(int ahead)
		{
			while(_queue.Count <= ahead)
				_queue.Add(Extract());

			return _queue[ahead].C;
		}

		public bool PeekIsWhite()
		{
			return Char.IsWhiteSpace(Peek());
		}

		public bool PeekIsNumber()
		{
			return Char.IsNumber(Peek());
		}
	}
}