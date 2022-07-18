namespace koto
{
	internal class Variables
	{
		private Dictionary<string, Func<string, string>> Vars = new Dictionary<string, Func<string, string>>();

		public void Add(string name, Func<string, string> func)
		{
			if (Vars.ContainsKey(name))
				Vars.Remove(name);
			Vars.Add(name, func);
		}

		public void Clear()
		{
			Vars.Clear();
		}

		public string Get(string name)
		{
			return Get(name, string.Empty);
		}
		public string Get(string name, string fileName)
		{
			if (Vars.ContainsKey(name))
				return Vars[name](fileName);
			throw new Exception("Unknown variable \"" + name + "\"");
		}

		public bool Exists(string name)
		{
			return Vars.ContainsKey(name);
		}
	}
}
