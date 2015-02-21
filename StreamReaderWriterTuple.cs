using System;
using System.IO;
using System.Text;
using System.Reactive.Linq;
using System.Linq;
using System.Reactive;

namespace MPDisplay
{
	public sealed class StreamReaderWriterTuple : IDisposable
	{
		private readonly StreamReader _reader;
		private readonly StreamWriter _writer;

		public StreamReaderWriterTuple (Stream stream)
		{
			this._reader = new StreamReader (stream, Encoding.UTF8);
			this._writer = new StreamWriter (stream, new UTF8Encoding (false));
			this._writer.AutoFlush = true;
		}

		public IObservable<CommandResponse> Send(string command)
		{
			return Observable
				.FromAsync(() => this._writer
				    .WriteAsync(command + "\n"))
				.SelectMany(_ => AsyncEnumerable
					.Repeat(Unit.Default)
					.SelectMany(x => this._reader
						.ReadLineAsync()
					    .ToAsyncEnumerable())
					.TakeWhile(line => (line != null) && (line != "OK"))
					.ToObservable())
				.ToArray()
				.Select(array => new CommandResponse(command, array));
		}

		public void Dispose()
		{
			this._reader.Dispose ();
			this._writer.Dispose ();
		}
	}
}

