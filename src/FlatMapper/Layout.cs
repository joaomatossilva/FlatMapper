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

        protected List<FieldSettingsBase<T>> InnerFields { get; private set; }

        internal int HeaderLinesCount { get; set; }
        protected Func<T> ItemCreateInstanceHandler { get; private set; }

        protected Layout()
        {
            this.InnerFields = new List<FieldSettingsBase<T>>();
            this.ItemCreateInstanceHandler = DynamicMethodCompiler.CreateInstantiateObjectHandler<T>();
        }

        protected Layout<T> HeaderLines(int count)
        {
            HeaderLinesCount = count;
            return this;
        }

        protected Layout<T> WithMember<TMember>(Expression<Func<T, TMember>> expression, Action<IFieldSettings<T, TMember>> settings)
        {
            var fieldSettings = new FieldSettings<T, TMember>(expression);
            settings(fieldSettings);
            fieldSettings.FieldValueConverter = fieldSettings.FieldValueConverter ?? new FieldValueConverter<TMember>();
            InnerFields.Add(fieldSettings);
            return this;
        }

        internal IEnumerable<FieldSettingsBase<T>> Fields
        {
            get
            {
                return InnerFields;
            }
        }

        public abstract T ParseLine(string line);

        public abstract string BuildLine(T value);

        public abstract string BuildHeaderLine();

        protected virtual object GetFieldValueFromString(FieldSettingsBase<T> fieldSettings, string memberValue)
        {
            if (fieldSettings.IsNullable && (string.IsNullOrEmpty(memberValue) || memberValue.Equals(fieldSettings.NullValue)))
            {
                return null;
            }
            memberValue = fieldSettings.PadLeft
                            ? memberValue.TrimStart(new char[] { fieldSettings.PaddingChar })
                            : memberValue.TrimEnd(new char[] { fieldSettings.PaddingChar });
            try
            {
                return fieldSettings.FieldValueConverter.FromString(memberValue, fieldSettings.FormatProvider);
            }
            catch (Exception ex)
            {
                var errorInfo = new ParserErrorInfo
                {
                    ErrorMessage = ex.Message,
                    FieldName = fieldSettings.PropertyInfo.Name,
                    FieldValue = memberValue,
                    FieldType = fieldSettings.PropertyInfo.PropertyType
                };
                throw new ParserErrorException(errorInfo, ex);
            }
        }

        protected virtual string GetStringValueFromField(FieldSettingsBase<T> field, object fieldValue)
        {
            if (fieldValue == null)
            {
                return field.NullValue;
            }
            string lineValue = field.FieldValueConverter.FromValue(fieldValue, field.FormatProvider);
            if (lineValue.Length < field.Length)
            {
                lineValue = field.PadLeft
                                ? lineValue.PadLeft(field.Length, field.PaddingChar)
                                : lineValue.PadRight(field.Length, field.PaddingChar);
            }
            return lineValue;
        }

        public virtual string ReadLine(StreamReader streamReader)
        {
            return streamReader.ReadLine();
        }
    }
}
