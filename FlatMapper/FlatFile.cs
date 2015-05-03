using System;
using System.Collections.Generic;
using System.IO;

namespace FlatMapper
{
    public class FlatFile<T> where T : new()
    {
        private readonly Layout<T> layout;

        private readonly Stream innerStream;

        private readonly Func<string, Exception, bool> handleEntryReadError;

        public FlatFile(Layout<T> layout, Stream innerStream, Func<string, Exception, bool> handleEntryReadError)
        {
            this.layout = layout;
            this.innerStream = innerStream;
            this.handleEntryReadError = handleEntryReadError;
        }

        public FlatFile(Layout<T> layout, Stream innerStream)
            : this(layout, innerStream, DefaultThrowExceptionOnReadError)
        {
        }

        private static bool DefaultThrowExceptionOnReadError(string line, Exception exception)
        {
            throw new Exception(string.Format("Error reading line '{0}'", line), exception);
        }

        public IEnumerable<T> Read()
        {
            //we're not disposng the StreamReader because it will dispose the inner stream
            var reader = new StreamReader(this.innerStream);
            SkipHeaders(reader);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                bool ignoreEntry = false;
                T entry = default(T);
                try
                {
                    entry = layout.ParseLine(line);
                }
                catch (Exception ex)
                {
                    if (!handleEntryReadError(line, ex))
                    {
                        throw;
                    }
                    ignoreEntry = true;
                }
                if (!ignoreEntry)
                {
                    yield return entry;
                }
            }
        }

        private void SkipHeaders(StreamReader reader)
        {
            for (var i = 0; i < layout.HeaderLinesCount && !reader.EndOfStream; i++)
            {
                reader.ReadLine();
            }
        }

        public void Write(IEnumerable<T> entries)
        {
            //we're not disposng the StramWriter because it will dispose the inner stream
            var writer = new StreamWriter(this.innerStream);
            WriteHeaders(writer);
            foreach (var entry in entries)
            {
                var line = layout.BuildLine(entry);
                writer.WriteLine(line);
            }
            writer.Flush();
        }

        private void WriteHeaders(StreamWriter writer)
        {
            for (var i = 0; i < layout.HeaderLinesCount; i++)
            {
                var line = layout.BuildHeaderLine();
                writer.WriteLine(line);
            }
        }
    }
}
