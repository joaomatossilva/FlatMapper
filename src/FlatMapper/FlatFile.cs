#region License

// 
// Copyright (c) 2011-2015, João Matos Silva <kappy@acydburne.com.pt>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 

#endregion
using System;
using System.Collections.Generic;
using System.IO;

namespace FlatMapper
{
    public class FlatFile<T> where T : new()
    {
        private readonly Layout<T> layout;

        private readonly Stream innerStream;

        private readonly Func<ParserErrorInfo, Exception, bool> handleEntryReadError;

        private readonly System.Text.Encoding encoding;

        private bool writeHeaders;

        public FlatFile(Layout<T> layout, Stream innerStream, System.Text.Encoding encoding, Func<ParserErrorInfo, Exception, bool> handleEntryReadError)
        {
            this.layout = layout;
            this.innerStream = innerStream;
            this.handleEntryReadError = handleEntryReadError;
            this.encoding = encoding;
            writeHeaders = true;
        }

        public FlatFile(Layout<T> layout, Stream innerStream, Func<ParserErrorInfo, Exception, bool> handleEntryReadError)
            : this(layout, innerStream, System.Text.Encoding.UTF8, handleEntryReadError)
        {
        }

        public FlatFile(Layout<T> layout, Stream innerStream)
            : this(layout, innerStream, DefaultOnReadError)
        {
        }

        private static bool DefaultOnReadError(ParserErrorInfo errorInfo, Exception exception)
        {
            return false;
        }

        public IEnumerable<T> Read()
        {
            //we're not disposng the StreamReader because it will dispose the inner stream
            var reader = new StreamReader(this.innerStream, encoding);
            SkipHeaders(reader);
            string line;
            while ((line = layout.ReadLine(reader)) != null)
            {
                bool ignoreEntry = false;
                T entry = default(T);
                try
                {
                    entry = layout.ParseLine(line);
                }
                catch (ParserErrorException ex)
                {
                    if (!handleEntryReadError(ex.ParserErrorInfo, ex))
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
            //we're not disposing the StreamWriter because it will dispose the inner stream
            var writer = new StreamWriter(this.innerStream, encoding);

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
            if (writeHeaders)
            {
                for (var i = 0; i < layout.HeaderLinesCount; i++)
                {
                    var line = layout.BuildHeaderLine();
                    writer.WriteLine(line);
                }
                writeHeaders = false;
            }
        }
    }
}
