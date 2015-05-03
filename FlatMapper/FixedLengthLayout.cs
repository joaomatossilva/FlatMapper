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

            public new FixedLengthLayout WithMember<TMType>(Expression<Func<T, TMType>> expression, Action<IFieldSettings> settings)
            {
                return (FixedLengthLayout)base.WithMember(expression, settings);
            }

            internal override T ParseLine(string line)
            {
                var entry = new T();
                int linePosition = 0;
                foreach (var field in this.Fields)
                {
                    string fieldValueFromLine = line.Substring(linePosition, field.Lenght);
                    var convertedFieldValue = GetFieldValueFromString(field, fieldValueFromLine);
                    field.PropertyInfo.SetValue(entry, convertedFieldValue, null);
                    linePosition += field.Lenght;
                }
                return entry;
            }

            internal override string BuildLine(T entry)
            {
                string line = this.Fields.Aggregate(string.Empty,
                    (current, field) => current + GetStringValueFromField(field, field.PropertyInfo.GetValue(entry, null)));
                return line;
            }

            internal override string BuildHeaderLine()
            {
                string line = this.Fields.Aggregate(string.Empty,
                    (current, field) => current + GetStringValueFromField(field, field.PropertyInfo.Name));
                return line;
            }
        }
    }
}
