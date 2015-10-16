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
using System.Linq.Expressions;
using System.Reflection;

namespace FlatMapper
{
    public abstract partial class Layout<T>
    {

        protected List<FieldSettings<T>> InnerFields { get; private set; }

        internal int HeaderLinesCount { get; set; }
        protected Func<T> ItemCreateInstanceHandler { get; private set; }

        protected Layout()
        {
            this.InnerFields = new List<FieldSettings<T>>();
            this.ItemCreateInstanceHandler = DynamicMethodCompiler.CreateInstantiateObjectHandler<T>();
        }

        protected Layout<T> HeaderLines(int count)
        {
            HeaderLinesCount = count;
            return this;
        }

        protected Layout<T> WithMember(Expression<Func<T, object>> expression, Action<IFieldSettings> settings)
        {
            var fieldSettings = new FieldSettings<T>(expression);
            settings(fieldSettings);
            InnerFields.Add(fieldSettings);
            return this;
        }

        internal IEnumerable<FieldSettings<T>> Fields
        {
            get
            {
                return InnerFields;
            }
        }

        public abstract T ParseLine(string line);

        public abstract string BuildLine(T value);

        public abstract string BuildHeaderLine();

        protected virtual object GetFieldValueFromString(FieldSettings<T> fieldSettings, string memberValue)
        {
            if (fieldSettings.IsNullable && memberValue.Equals(fieldSettings.NullValue))
            {
                return null;
            }
            memberValue = fieldSettings.PadLeft
                            ? memberValue.TrimStart(new char[] { fieldSettings.PaddingChar })
                            : memberValue.TrimEnd(new char[] { fieldSettings.PaddingChar });

            //TODO: Execute here custom converters
            return Convert.ChangeType(memberValue, fieldSettings.ConvertToType);
        }

        protected virtual string GetStringValueFromField(FieldSettings<T> field, object fieldValue)
        {
            if (fieldValue == null)
            {
                return field.NullValue;
            }
            string lineValue = fieldValue.ToString();
            if (lineValue.Length < field.Lenght)
            {
                lineValue = field.PadLeft
                                ? lineValue.PadLeft(field.Lenght, field.PaddingChar)
                                : lineValue.PadRight(field.Lenght, field.PaddingChar);
            }
            return lineValue;
        }

        public virtual string ReadLine(StreamReader streamReader)
        {
            return streamReader.ReadLine();
        }
    }
}
