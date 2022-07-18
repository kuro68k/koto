using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace koto
{
	public enum ArgType
	{
		Int,
		Double,
		String,
		Flag,
		Counter
	}


	public class CmdArgument
	{
		private dynamic _value;
		private ArgType _type;
		private Action<dynamic> _assign;
		private string _help;
		private string _short_description;
		private bool _matched;
		private bool _required;

		public dynamic Value { get { return _value; } set { _value = value; } }
		public ArgType Type { get { return _type; } }
		public string Help { get { return _help; } set { _help = value; } }
		public string ShortDescription { get { return _short_description; } set { _short_description = value; } }
		public bool WasMatched { get { return _matched; } set { _matched = value; } }
		public bool IsRequired { get { return _required; } }

		public CmdArgument(ArgType type,
						   string help = "",
						   string short_description = "",
						   bool required = false,
						   Action<dynamic>? assign = default(Action<dynamic>))
		{
			_type = type;
			_value = NewArgType(type);
			_assign = assign ?? throw new ArgumentNullException(nameof(assign));
			_help = help;
			_short_description = short_description;
			_required = required;
			_matched = false;
		}

		// generate a new dynamic object with type suitable for ArgType
		private dynamic NewArgType(ArgType type)
		{
			switch (type)
			{
				case ArgType.Int:
				case ArgType.Counter:
					return new int();
				case ArgType.Double:
					return new double();
				case ArgType.String:
					return String.Empty;
				case ArgType.Flag:
					return new bool();
				default:
					throw new Exception("Unknown ArgType " + type.ToString());
			}
		}

		// parse a string and set the argument's value
		public bool ParseValue(string v)
		{
			switch (_type)
			{
				case ArgType.Int:
					{
						int i;
						//bool res = int.TryParse(v, out i);
						bool res = TextUtils.ParseInt(v, out i);
						if (res)
						{
							_value = i;
							break;
						}
						return false;
					}

				case ArgType.Double:
					{
						double d;
						bool res = double.TryParse(v, out d);
						if (res)
						{
							_value = d;
							break;
						}
						else
							return false;
					}

				case ArgType.String:
					_value = v;
					break;

				case ArgType.Flag:
					_value = true;
					break;

				case ArgType.Counter:
					_value++;
					break;
				default:
					return false;
			}

			if (_assign != null)
				_assign(_value);
			return true;
		}
	}



	public class CmdOptionArgument : CmdArgument
	{
		private string[] _option_names;

		public string Name { get { if (_option_names != null) return _option_names[0]; else return "(anon)"; } }
		public string AllOptionNames { get { return string.Join(",", _option_names); } }
		public string[] OptionNameList { get { return _option_names; } }

		public CmdOptionArgument(ArgType type, string option_names = "", string help = "", string short_description = "", bool required = false, Action<dynamic>? assign = default(Action<dynamic>)) :
								base(type, help, short_description, required, assign)
		{
			if (string.IsNullOrEmpty(option_names))
				throw new ArgumentException("Options must be named", "option_names");
			if (assign is null)
				throw new ArgumentNullException(nameof(assign));
			_option_names = option_names.Split(',');
		}

		// match the argument's name(s) to a string
		public bool MatchesName(string name)
		{
			foreach (string n in _option_names)
			{
				if (n == name)
					return true;
			}
			return false;
		}
	}



	public class CmdArgs : List<CmdArgument>
	{
		private List<CmdArgument> _argList;
		private List<CmdOptionArgument> _optionList;
		private int _helpColumnWidth = 30;
		public int HelpColumnWidth { get { return _helpColumnWidth; } set { _helpColumnWidth = value; } }
		public bool PosixMode = false;

		public CmdArgs()
		{
			_argList = new List<CmdArgument>();
			_optionList = new List<CmdOptionArgument>();
		}

		public CmdArgs(List<CmdArgument> argList)
		{
			_argList = argList;
			_optionList = new List<CmdOptionArgument>();
		}

		public CmdArgs(List<CmdOptionArgument> optionList, List<CmdArgument> argList)
		{
			_optionList = optionList;
			_argList = argList;
		}

		public new void Add(CmdArgument ca)
		{
			_argList.Add(ca);
		}

		private List<string> SplitEqualsArguments(List<string> cmdList)
		{
			for (int i = 0; i < cmdList.Count; i++)
			{
				if (cmdList[i].Contains('='))
				{
					int loc = cmdList[i].IndexOf('=');
					if ((loc == 0) || (cmdList[i].Length < 3))
						continue;
					string s = cmdList[i].Substring(loc + 1);
					cmdList[i] = cmdList[i].Substring(0, loc);
					cmdList.Insert(i + 1, s);
				}
			}
			return cmdList;
		}

		// parse option arguments, return a string array containing remaining arguments
		public string[] Parse(string[] args)
		{
			List<string> cmdList = new List<string>(args);
			cmdList = SplitEqualsArguments(cmdList);
			List<string> remainder = new List<string>();

			// named option arguments
			while (cmdList.Count > 0)
			{
				string cmd = cmdList[0];
				string original_cmd = cmd;
				//Console.WriteLine(cmd + "\t (" + String.Join(",", cmdList) + ")");
				cmdList.RemoveAt(0);

				// options
				if ((cmd.StartsWith("-")) || (cmd.StartsWith("/")))
				{
					cmd = cmd.Substring(1);
					bool match = false;
					foreach (CmdOptionArgument ca in _optionList)
					{
						if (ca.MatchesName(cmd))
						{
							match = true;

							if (ca.WasMatched && (ca.Type != ArgType.Counter))
								throw new ArgumentException("Duplicate argument.", ca.AllOptionNames);

							ca.WasMatched = true;
							if ((ca.Type == ArgType.Flag) || (ca.Type == ArgType.Counter))  // no parameters
							{
								if (!ca.ParseValue(""))
									throw new ArgumentException("Parsing error.", ca.AllOptionNames);
							}
							else
							{
								if (cmdList.Count < 1)
									throw new ArgumentException("Missing parameter.", ca.AllOptionNames);
								string param = cmdList[0];
								cmdList.RemoveAt(0);
								if (!ca.ParseValue(param))
									throw new ArgumentException("Bad parameter \"" + param + "\".", ca.AllOptionNames);
							}
							break;  // foreach
						}
					}

					if (!match)
						throw new ArgumentException("Unknown argument", original_cmd);
				}
				else    // not an option
					remainder.Add(cmd);
			}

			// normal non-option arguments
			int i = 0;
			foreach (CmdArgument ca in _argList)
			{
				if (remainder.Count > i)
				{
					ca.ParseValue(remainder[i++]);
					ca.WasMatched = true;
				}
			}

			foreach (CmdOptionArgument ca in _optionList)
			{
				if (!ca.WasMatched && ca.IsRequired)
					throw new ArgumentException("Argument is required.", ca.Name);
			}
			foreach (CmdArgument ca in _argList)
			{
				if (!ca.WasMatched && ca.IsRequired)
					throw new ArgumentException("Argument is required.", ca.ShortDescription);
			}

			return remainder.ToArray();
		}

		// returns true if parsed option arguments OK
		public bool TryParse(string[] args, out string[]? remainder)
		{
			remainder = null;

			try
			{
				remainder = Parse(args);
				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return false;
			}
		}

		public bool TryParse(string[] args)
		{
			string[]? remainder;
			return TryParse(args, out remainder);
		}

		// parse non-option arguments (remainders)
		public void ParseRemainders(string[] remainders, out string? p1)
		{
			p1 = null;
			if ((remainders != null) && (remainders.Length > 0))
				p1 = remainders[0];
		}

		public void ParseRemainders(string[] remainders, out string? p1, out string? p2)
		{
			p1 = null;
			p2 = null;
			if (remainders != null)
			{
				ParseRemainders(remainders, out p1);
				if (remainders.Length > 1)
					p2 = remainders[1];
			}
		}

		public void ParseRemainders(string[] remainders, out string? p1, out string? p2, out string? p3)
		{
			p1 = null;
			p2 = null;
			p3 = null;
			if (remainders != null)
			{
				ParseRemainders(remainders, out p1, out p2);
				if (remainders.Length > 2)
					p3 = remainders[2];
			}
		}

		public void ParseRemainders(string[] remainders, out string? p1, out string? p2, out string? p3, out string? p4)
		{
			p1 = null;
			p2 = null;
			p3 = null;
			p4 = null;
			if (remainders != null)
			{
				ParseRemainders(remainders, out p1, out p2, out p3);
				if (remainders.Length > 3)
					p4 = remainders[3];
			}
		}

		// dump arguments and values to console
		public void PrintArgs()
		{
			foreach (CmdArgument ca in _argList)
			{
				Console.WriteLine(ca.ShortDescription + "\t" + ca.Value.ToString());
			}
			foreach (CmdOptionArgument ca in _optionList)
			{
				Console.WriteLine(ca.Name + "\t" + ca.Value.ToString());
			}
		}

		// print help
		public void PrintHelp()
		{
			string name = System.AppDomain.CurrentDomain.FriendlyName;
			if (name.EndsWith(".exe"))
				name = name.Substring(0, name.Length - 4);
			PrintHelp(name);
		}

		public void PrintHelp(string appName)
		{
			if (PosixMode)
			{
				PrintHelpPOSIX(appName);
				return;
			}

			// headline
			Console.Write(appName);
			if (_optionList != null)
				Console.Write(" [options]");
			foreach (CmdArgument ca in _argList)
			{
				Console.Write(" ");
				if (!ca.IsRequired)
					Console.Write("<");
				Console.Write(ca.ShortDescription);
				if (!ca.IsRequired)
					Console.Write(">");
			}
			Console.WriteLine();

			// options
			if (_optionList == null)
				return;

			Console.WriteLine();
			Console.WriteLine("Options:");
			foreach (CmdOptionArgument ca in _optionList)
			{
				string left = "  -";
				left += ca.AllOptionNames;
				if (!string.IsNullOrEmpty(ca.ShortDescription))
					//left += " <" + ca.ShortDescription + ">";
					left += "=" + ca.ShortDescription;
				left = left.PadRight(_helpColumnWidth);
				Console.Write(left + " ");
				Console.WriteLine(ca.Help);
			}
		}

		public void PrintHelpPOSIX(string appName)
		{
			Console.Write(appName);

			foreach (var opt in _optionList)
			{
				Console.Write(" [-" + opt.Name);
				if ((opt.Type != ArgType.Flag) && (opt.Type != ArgType.Counter))
					Console.Write(" " + opt.ShortDescription);
				Console.Write("]");
			}
			foreach (var arg in _argList)
				Console.Write(" [" + arg.ShortDescription + "]");
		}

		// split command line arguments into a list of strings
		public static string[]? SplitArgs(string args)
		{
			if (string.IsNullOrEmpty(args))
				return null;

			List<string> a = new List<string>();
			StringBuilder sb = new StringBuilder();
			int idx = 0;
			bool inQuotedString = false;

			while (idx < args.Length)
			{
				char c = args[idx++];

				if (c == '"')
				{
					inQuotedString = !inQuotedString;
					continue;
				}
				if (inQuotedString)
				{
					sb.Append(c);
					continue;
				}

				if ((c == ' ') || (c == '='))
				{
					if (sb.Length > 0)
						a.Add(sb.ToString());
					sb.Clear();
				}
				else
					sb.Append(c);
			}
			if (sb.Length > 0)
				a.Add(sb.ToString());

			return a.ToArray();
		}
	}
}
