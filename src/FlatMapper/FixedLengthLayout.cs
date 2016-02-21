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
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FlatMapper
{
    public abstract partial class Layout<T> where T : new()
    {
        public class FixedLengthLayout : Layout<T>
        {
            public new FixedLengthLayout HeaderLines(int count)
            {
                return (FixedLengthLayout)base.HeaderLines(count);
            }

            public FixedLengthLayout WithMember<TMember>(Expression<Func<T, TMember>> expression)
            {
                return this.WithMember(expression, set => { });
            }

            public new FixedLengthLayout WithMember<TMember>(Expression<Func<T, TMember>> expression, Action<IFieldSettings<T, TMember>> settings)
            {
                return (FixedLengthLayout)base.WithMember(expression, settings);
            }

            private int _lineSize;

            private int LineSize
            {
                get
                {
                    if (_lineSize == 0)
                    {
                        _lineSize = this.Fields.Sum(f => f.Length);
                    }
                    return _lineSize;
                }
            }

            public override T ParseLine(string line)
            {
                var entry = ItemCreateInstanceHandler();
                int linePosition = 0;
                foreach (var field in this.Fields)
                {
                    string fieldValueFromLine = line.Substring(linePosition, field.Length);
                    var convertedFieldValue = GetFieldValueFromString(field, fieldValueFromLine);
                    field.SetHandler(entry, convertedFieldValue);
                    linePosition += field.Length;
                }
                return entry;
            }

            public override string BuildLine(T entry)
            {
                var sb = new StringBuilder(LineSize);
                foreach (var fieldSettingse in this.Fields)
                {
                    var fieldValue = fieldSettingse.GetHandler(entry);
                    sb.Append(GetStringValueFromField(fieldSettingse, fieldValue));
                }
                return sb.ToString();
            }

            public override string BuildHeaderLine()
            {
                //TODO: use stringBuilder to handle Headers, Truncate header name by FieldLength if needed
                string line = this.Fields.Aggregate(string.Empty,
                    (current, field) => current + GetStringValueFromField(field, field.PropertyInfo.Name));
                return line;
            }
        }
    }
}
