using System;

namespace MPDisplay
{
	public struct MpdStatus
	{
		private readonly int _volume;
		private readonly SongInfo? _songInfo;
		private readonly PlayerState _state;

		public MpdStatus (SongInfo? songInfo, PlayerState state, int volume)
		{
			this._state = state;
			this._volume = volume;
			this._songInfo = songInfo;
		}

		public int Volume
		{
			get
			{
				return this._volume;
			}
		}

		public SongInfo? SongInfo
		{
			get
			{
				return this._songInfo;
			}
		}

		public PlayerState PlayerState
		{
			get
			{
				return this._state;
			}
		}
	}
}

