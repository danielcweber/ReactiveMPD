using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MPDisplay
{
	public struct SongInfo
	{
		private readonly string _title;
		private readonly string _artist;
		private readonly string _album;

		public SongInfo (Dictionary<string, string> data)
		{
			this._title = null;

			if (data.ContainsKey ("Title"))
				this._title = data ["Title"];
			else if (data.ContainsKey ("Name"))
				this._title = data ["Name"];

			data.TryGetValue ("Album", out this._album);
			data.TryGetValue ("Artist", out this._artist);

			if ((this._title == null) && (data.ContainsKey("file")))
			{
				var segments = data["file"].Split(Path.DirectorySeparatorChar);

				if (segments.Length > 0)
				{
					this._title = Path.GetFileNameWithoutExtension(segments [segments.Length - 1]
						//.TrimStart('0', '1', '2', '3', '4', '5', '6', '7', '8', '9')
						.Trim());
				}

				if ((segments.Length > 1) && (this._album == null))
					this._album = segments [segments.Length - 2];

				if ((segments.Length > 1) && (this._artist == null))
					this._artist =  segments [segments.Length - 3];
			}
		}

		public override string ToString ()
		{
			var builder = new StringBuilder ();

			if (this._artist != null)
				builder.AppendFormat ("{0} - ", this._artist);

			builder.Append (this._title);

			return builder.ToString ();
		}
	}
}

