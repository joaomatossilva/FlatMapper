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
using System.Linq.Expressions;
using System.Reflection;

namespace FlatMapper
{
    public abstract class FieldSettingsBase<T>
    {
        public int Length { get; set; }
        public char PaddingChar { get; set; }
        public bool IsNullable { get; set; }
        public string NullValue { get; set; }
        public bool PadLeft { get; set; }
        public IFormatProvider FormatProvider { get; set; }
        public IFieldValueConverter FieldValueConverter { get; set; }
        public PropertyInfo PropertyInfo { get; protected set; }
        public Func<T, object> GetHandler { get; protected set; }
        public Action<T, object> SetHandler { get; protected set; }

    }

    public class FieldSettings<T, TMember> : FieldSettingsBase<T>, IFieldSettings<T, TMember>
    {
        public FieldSettings(Expression<Func<T, TMember>> expression)
        {
            var memberExpression = GetMemberExpression(expression);
            PropertyInfo = (PropertyInfo) memberExpression.Member;
            GetHandler = DynamicMethodCompiler.CreateGetHandler<T>(PropertyInfo);
            SetHandler = DynamicMethodCompiler.CreateSetHandler<T>(PropertyInfo);
        }

        private MemberExpression GetMemberExpression(Expression<Func<T, TMember>> expression)
        {
            MemberExpression memberExpression = null;
            if (expression.Body.NodeType == ExpressionType.Convert)
            {
                var body = (UnaryExpression)expression.Body;
                memberExpression = body.Operand as MemberExpression;
            }
            else if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = expression.Body as MemberExpression;
            }
            if (memberExpression == null)
            {
                throw new ArgumentException("Not a member access", "expression");
            }
            return memberExpression;
        }

        protected FieldSettings()
        {
            
        }

        public IFieldSettings<T, TMember> WithLength(int length)
        {
            this.Length = length;
            return this;
        }

        public IFieldSettings<T, TMember> WithLeftPadding(char paddingChar)
        {
            this.PaddingChar = paddingChar;
            this.PadLeft = true;
            return this;
        }

        public IFieldSettings<T, TMember> WithRightPadding(char paddingChar)
        {
            this.PaddingChar = paddingChar;
            this.PadLeft = false;
            return this;
        }

        public IFieldSettings<T, TMember> AllowNull(string nullValue)
        {
            this.IsNullable = true;
            this.NullValue = nullValue;
            return this;
        }

        public IFieldSettings<T, TMember> UseValueConverter<TValueConverter>() where TValueConverter : FieldValueConverter<TMember>, new()
        {
            this.FieldValueConverter = new TValueConverter();
            return this;
        }

        public IFieldSettings<T, TMember> WithFormat(IFormatProvider formatProvider)
        {
            this.FormatProvider = formatProvider;
            return this;
        }
    }
}
