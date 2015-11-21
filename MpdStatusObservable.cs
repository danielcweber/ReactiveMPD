using System;
using System.Net;
using System.Reactive;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace MPDisplay
{
	public sealed class MpdStatusObservable : IObservable<MpdStatus>
	{
		private const string StatusCommand = "status";
		private const string IdlePlayer = "idle player mixer";
		private const string CurrentSongCommand = "currentsong";

		private readonly IObservable<MpdStatus> _inner;

		public MpdStatusObservable (string host, int port)
		{
			this._inner = Observable
				.Using(
					() => new TcpClient(),
					client => Observable
						.FromAsync(() => Task.Factory.FromAsync(client.BeginConnect, client.EndConnect, host, port, null))
						//.Do(_ => Console.WriteLine("Connected"))
						.SelectMany(unit => Observable
				            .Using(
								() => client.GetStream(),
								stream => Observable
									.Using(
										() => new StreamReaderWriterTuple(stream),
										tuple => Observable
											.Return(Unit.Default)
											.SelectMany(_ => tuple.Send(CurrentSongCommand)
												.Concat(tuple.Send(StatusCommand))
												.Concat(tuple.Send(IdlePlayer)))
											.Repeat()
						            		.TakeWhile(_ => stream.CanRead)))
							.Scan(
								new MpdStatus(),
								(currentStatus, commandResponse) =>
								{
									if (commandResponse.Command == CurrentSongCommand)
									{
										return new MpdStatus(
											commandResponse.ResponseFields.Count > 0
												? new SongInfo(commandResponse.ResponseFields)
												: (SongInfo?)null,
											currentStatus.PlayerState,
											currentStatus.Volume);
									}
									else if (commandResponse.Command == StatusCommand)
									{
										string state = null;
										string volume = null;

										var newPlayerState = (commandResponse.ResponseFields.TryGetValue("state", out state))
											? (state == "play" 
												? PlayerState.Play
												: state == "pause"
													? PlayerState.Pause
													: PlayerState.Stop)
											: currentStatus.PlayerState;
										
										var newVolume = (commandResponse.ResponseFields.TryGetValue("volume", out volume))
											? int.Parse(volume)
											: currentStatus.Volume;
										
										return new MpdStatus(
											currentStatus.SongInfo,
											newPlayerState,
											newVolume);
									}	

									return currentStatus;
								})
							.DistinctUntilChanged()))
				.Catch<MpdStatus, Exception>(ex =>
				{
					Console.WriteLine(ex);
					return Observable
						.Empty<MpdStatus>()
						.Delay(TimeSpan.FromSeconds(10));
				})
				.Repeat();
		}

		public IDisposable Subscribe(IObserver<MpdStatus> observer)
		{
			return this._inner.Subscribe (observer);
		}
	}
}

