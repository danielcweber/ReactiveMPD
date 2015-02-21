using System;
using System.Linq;
using System.Collections.Generic;

namespace MPDisplay
{
	public class CommandResponse
	{
		private readonly string _command;
		private readonly Dictionary<string, string> _responseFields;

		public CommandResponse(string command, string[] responseFields) : this(command)
		{
			var splitted = responseFields
				.Select (x => x
					.Split(new[] { ':' }, 2)
				    .Select (part => part.Trim ())
				    .ToArray())
				.Where (x => x.Length == 2);

			this._responseFields = splitted.ToDictionary (
				array => array [0],
				array => array [1]);
		}

		public CommandResponse (string command, Dictionary<string, string> responseFields) :this(command)
		{
			this._responseFields = responseFields;
		}

		private CommandResponse(string command)
		{
			this._command = command;
		}

		public string Command
		{
			get
			{
				return this._command;
			}
		}

		public Dictionary<string, string> ResponseFields
		{
			get
			{
				return this._responseFields;
			}
		}
	}
}

