#region License

// 
// Copyright (c) 2011-2012, João Matos Silva <kappy@acydburne.com.pt>
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
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FlatMapper
{
    public abstract partial class Layout<T> where T : new()
    {
        public class DelimitedLayout : Layout<T>
        {

            private string Quotes { get; set; }

            private string innerDelimiter = ",";

            private bool multiLine = false;

            public new DelimitedLayout HeaderLines(int count)
            {
                return (DelimitedLayout)base.HeaderLines(count);
            }

            public new DelimitedLayout WithMember<TMember>(Expression<Func<T, TMember>> expression, Action<IFieldSettings<T, TMember>> settings)
            {
                return (DelimitedLayout)base.WithMember(expression, settings);
            }

            public DelimitedLayout WithQuote(string quotes)
            {
                Quotes = quotes;
                return this;
            }

            public DelimitedLayout WithDelimiter(string delimiter)
            {
                Delimiter = delimiter;
                return this;
            }

            public DelimitedLayout WithMultiLine(bool multiLine)
            {
                this.multiLine = multiLine;
                return this;
            }

            private string Delimiter
            {
                get { return innerDelimiter; }
                set
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        throw new ArgumentException("Delimiter cannot be null or empty", "value");
                    }
                    innerDelimiter = value;
                }
            }

            public override T ParseLine(string line)
            {
                var entry = ItemCreateInstanceHandler();
                int linePosition = 0;
                var compositeDelimiter = !string.IsNullOrEmpty(Quotes) ? Quotes + Delimiter + Quotes : Delimiter;
                int delimiterSize = compositeDelimiter.Length;
                foreach (var field in this.Fields)
                {
                    int nextDelimiterIndex = -1;
                    if (line.Length > linePosition + delimiterSize)
                    {
                        nextDelimiterIndex = line.IndexOf(compositeDelimiter, linePosition, StringComparison.InvariantCultureIgnoreCase);
                    }
                    int fieldLength;
                    if (nextDelimiterIndex > -1)
                    {
                        fieldLength = nextDelimiterIndex - linePosition;
                    }
                    else
                    {
                        fieldLength = line.Length - linePosition;
                    }
                    string fieldValueFromLine = line.Substring(linePosition, fieldLength);
                    var convertedFieldValue = GetFieldValueFromString(field, fieldValueFromLine);
                    field.SetHandler(entry, convertedFieldValue);
                    linePosition += fieldLength + (nextDelimiterIndex > -1 ? delimiterSize : 0);
                }
                return entry;
            }

            public override string BuildLine(T entry)
            {
                var sb = new StringBuilder(100); //TODO: Use better logic to guess a line size istead of hardcoded
                bool firstField = true;
                foreach (var fieldSettingse in this.Fields)
                {
                    if (!firstField)
                    {
                        sb.Append(innerDelimiter);
                    }
                    var fieldValue = fieldSettingse.GetHandler(entry);
                    sb.Append(GetStringValueFromField(fieldSettingse, fieldValue));
                    firstField = false;
                }
                return sb.ToString();
            }

            public override string BuildHeaderLine()
            {
                string line = this.Fields.Aggregate(string.Empty,
                    (current, field) => current + (current.Length > 0 ? innerDelimiter : "") + field.PropertyInfo.Name);
                return line;
            }

            protected override object GetFieldValueFromString(FieldSettingsBase<T> fieldSettings, string memberValue)
            {
                if (!string.IsNullOrEmpty(Quotes))
                {
                    memberValue = memberValue.Replace(Quotes, String.Empty);
                }
                return base.GetFieldValueFromString(fieldSettings, memberValue);
            }

            protected override string GetStringValueFromField(FieldSettingsBase<T> field, object fieldValue)
            {
                var stringValue = base.GetStringValueFromField(field, fieldValue);
                if (!string.IsNullOrEmpty(Quotes))
                {
                    stringValue = string.Format("{0}{1}{0}", Quotes, stringValue);
                }
                return stringValue;
            }

            public override string ReadLine(StreamReader streamReader)
            {
                if(!multiLine)
                {
                    return base.ReadLine(streamReader);
                }
                
                return ReadLinesUntilQuote(streamReader);
            }

            //Credits to Nuno Santos for original algorithm behind this method
            private string ReadLinesUntilQuote(StreamReader reader)
            {
                var stringBuilder = new StringBuilder();
                string line;
                do
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }
                    if (!line.EndsWith(Quotes))
                    {
                        stringBuilder.AppendLine(line);
                    }
                    else
                    {
                        stringBuilder.Append(line);
                        break;
                    }
                } while (line != null);
                var completeLine = stringBuilder.ToString();
                return string.IsNullOrEmpty(completeLine) ? null : completeLine;
            }
        }
    }
}
