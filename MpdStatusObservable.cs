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
		private const string IdlePlayer = "idle player";
		private const string CurrentSongCommand = "currentsong";

		private readonly IObservable<MpdStatus> _inner;

		public MpdStatusObservable ()
		{
			this._inner = Observable
				.Using(
					() => new TcpClient(),
					client => Observable
						.FromAsync(() => Task.Factory.FromAsync(client.BeginConnect, client.EndConnect, "localhost" /*"AudioPi.fritz.box"*/, 6600, null))
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
											100);
									}
									else if (commandResponse.Command == StatusCommand)
									{
										string state = null;
										if (commandResponse.ResponseFields.TryGetValue("state", out state))
										{
											return new MpdStatus(
												currentStatus.SongInfo,
												state == "play" 
													? PlayerState.Play
													: state == "pause"
														? PlayerState.Pause
														: PlayerState.Stop,
												100);
										}
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

