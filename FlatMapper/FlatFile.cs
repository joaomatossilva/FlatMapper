using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FlatMapper {
	public class FlatFile<T> : IDisposable where T : new() {
		private readonly Layout<T> layout;

		private readonly Stream innerStream;

		private readonly Func<string, Exception, bool> handleEntryReadError;

		public FlatFile(Layout<T> layout, Stream innerStream, Func<string, Exception, bool> handleEntryReadError) {
			this.layout = layout;
			this.innerStream = innerStream;
			this.handleEntryReadError = handleEntryReadError;
		}

		public IEnumerable<T> Read() {
			var reader = new StreamReader(this.innerStream);
			string line;
			while ((line = reader.ReadLine()) != null) {
				bool ignoreEntry = false;
				T entry = default(T);
				try {
					entry = layout.ParseLine(line);
				} catch (Exception ex) {
					if (!handleEntryReadError(line, ex)) {
						throw;
					}
					ignoreEntry = true;
				}
				if (!ignoreEntry) {
					yield return entry;
				}
			}
		}

		public void Write(IEnumerable<T> entries) {
			TextWriter writer = new StreamWriter(this.innerStream);
			foreach (var entry in entries) {
				var line = layout.BuildLine(entry);
				writer.WriteLine(line);
			}
			writer.Flush();
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing) {
			if (disposing) {
				if (this.innerStream != null) {
					this.innerStream.Dispose();
				}
			}
		}

		~FlatFile() {
			Dispose(false);
		}
	}
}
