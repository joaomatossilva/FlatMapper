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
