using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace koto
{
	public class TextUtils
	{
		// wrap a string, splitting on commas, periods or with hyphens
		public static string WrapString(string s, int max_width)
		{
			string w = "";
			int last_split_point = -1;
			int last_newline = 0;
			int width = 0;
			bool skip_spaces = false;

			for (int i = 0; i < s.Length; i++)
			{
				if (skip_spaces && (Char.IsWhiteSpace(s[i])))
				{
					last_newline = i;
					continue;
				}
				skip_spaces = false;

				width++;
				if (width > max_width)
				{
					if (last_split_point == -1)     // no split points found
					{
						w += s.Substring(last_newline, width).Trim() + "-\n";
						last_newline = i + 1;
						width = 0;
					}
					else
					{
						w += s.Substring(last_newline, last_split_point - last_newline + 1).Trim() + "\n";
						width = i - last_split_point;
						last_newline = last_split_point + 1;
					}
					skip_spaces = true;
					last_split_point = -1;
				}

				if ((s[i] == '.') || (s[i] == ','))
					last_split_point = i;
			}

			// any remaining text
			if (last_newline != s.Length)
				w += s.Substring(last_newline);

			return w;
		}

		// compact whitespace so that multiple spaces are reduce to just one
		public static string CompactWhitespace(string s)
		{
			string comp = "";
			bool last_char_was_space = false;

			for (int i = 0; i < s.Length; i++)
			{
				if (!Char.IsWhiteSpace(s[i]))
				{
					comp += s[i];
					last_char_was_space = false;
				}
				else
				{
					if (!last_char_was_space)
						comp += ' ';
					last_char_was_space = true;
				}
			}

			return comp;
		}

		// compact whitespace and word wrap to max_width
		public static string Reformat(string s, int max_width)
		{
			return WrapString(CompactWhitespace(s), max_width);
		}

		// checks if a string is a C format number (decminal, 0b, 0x)
		public static bool IsCNumeric(string s)
		{
			s = s.ToUpper().Trim();
			if (s.StartsWith("0X"))
			{
				for (int i = 2; i < s.Length; i++)
				{
					if (!Char.IsDigit(s[i]) &&
						s[i] != 'A' && s[i] != 'B' && s[i] != 'C' && s[i] != 'D' && s[i] != 'E' && s[i] != 'F')
						return false;
				}
			}
			else if (s.StartsWith("0B"))
			{
				for (int i = 2; i < s.Length; i++)
				{
					if (s[i] != '0' && s[i] != '1')
						return false;
				}
			}
			else if (s.StartsWith("0"))
			{
				for (int i = 1; i < s.Length; i++)
				{
					if (!Char.IsNumber(s[i]) &&
						s[i] != '9')
						return false;
				}
			}
			else
			{
				for (int i = 0; i < s.Length; i++)
				{
					if (!Char.IsNumber(s[i]))
						return false;
				}
			}
			return true;
		}

		// parse a C format int (decimal, 0b, 0x)
		public static bool ParseInt(string s, out int i)
		{
			i = 0;
			int _base = 10;

			s.Trim();
			s = s.ToUpper();

			if (s.StartsWith("0B"))
			{
				_base = 2;
				s = s.Substring(2);
			}
			else if (s.StartsWith("0X"))
			{
				_base = 16;
				s = s.Substring(2);
			}

			try
			{
				i = Convert.ToInt32(s, _base);
			}
			catch
			{
				return false;
			}

			return true;
		}

		// pad a string at regular intervals e.g. 0000 0000 0000 0000
		public static string PadString(string s, string padding, int spacing)
		{
			string output = "";
			int i = 0;
			while (i < s.Length)
			{
				output += s.Substring(i, spacing);
				i += spacing;
				if (i < s.Length)
					output += padding;
			}
			return output;
		}

		public static string PadString(string s, char padding, int spacing)
		{
			return PadString(s, padding.ToString(), spacing);
		}

		// convert string to a fixed length with padding/truncation
		public static string FixedLengthString(string s, char padding, int length)
		{
			if (s.Length > length)
				return "..." + s.Substring(s.Length - length + 3);

			if (s.Length < length)
				return s.PadRight(length, padding);

			return s;
		}

		// convert a path+filename to a fixed length string with padding/truncation
		static string FixedLengthFilename(string filename, char padding, int length)
		{
			if (filename.Length == length)
				return filename;

			while (filename.Length > length)
			{
				int p = filename.IndexOf(Path.VolumeSeparatorChar);
				if (p == -1)
					p = filename.IndexOf(Path.DirectorySeparatorChar);
				if (p == -1)
					p = filename.IndexOf(Path.AltDirectorySeparatorChar);
				if ((p != -1) && (filename.Length > p + 1))
					filename = filename.Substring(p + 1);
				else
					break;
			}

			if (filename.Length > length)
				filename = "..." + filename.Substring(filename.Length - length + 3);

			if (filename.Length < length)
				filename = filename.PadRight(length, padding);

			return filename;
		}

		// convert a number to binary string
		public static string ToBinString(UInt64 u64, int digits)
		{
			if (digits > 64)
				throw new Exception("Too many digits (max 64)");

			UInt64 mask = 1;
			var sb = new StringBuilder(new string('0', digits));
			for (int i = 0; i < digits; i++)
			{
				if ((u64 & mask) != 0)
					sb[digits - i - 1] = '1';
				mask <<= 1;
			}
			return sb.ToString();
		}

		public static string ToBinString(UInt64 v)
		{
			return ToBinString((UInt64)v, 64);
		}

		public static string ToBinString(UInt32 v)
		{
			return ToBinString((UInt64)v, 32);
		}

		public static string ToBinString(UInt16 v)
		{
			return ToBinString((UInt64)v, 16);
		}

		public static string ToBinString(byte v)
		{
			return ToBinString((UInt64)v, 8);
		}

		public static string ToBinString(Int64 v)
		{
			return ToBinString((UInt64)v, 64);
		}

		public static string ToBinString(Int32 v)
		{
			return ToBinString((UInt64)v, 32);
		}

		public static string ToBinString(Int16 v)
		{
			return ToBinString((UInt64)v, 16);
		}

		public static string ToBinString(sbyte v)
		{
			return ToBinString((UInt64)v, 8);
		}
	}
}