using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FlatMapper {
	public abstract partial class Layout<T> {

		protected List<FieldSettings> InnerFields { get; private set; }

		protected Layout() {
			this.InnerFields = new List<FieldSettings>();
		}

		public Layout<T> WithMember<TMType>(Expression<Func<T, TMType>> expression, Action<IFieldSettings> settings) {
			var memberExpression = GetMemberExpression(expression);
			var fieldSettings = new FieldSettings(memberExpression.Member as PropertyInfo);
			settings(fieldSettings);
			InnerFields.Add(fieldSettings);
			return this;
		}

		private MemberExpression GetMemberExpression<TMType>(Expression<Func<T, TMType>> expression) {
			MemberExpression memberExpression = null;
			if (expression.Body.NodeType == ExpressionType.Convert) {
				var body = (UnaryExpression)expression.Body;
				memberExpression = body.Operand as MemberExpression;
			} else if (expression.Body.NodeType == ExpressionType.MemberAccess) {
				memberExpression = expression.Body as MemberExpression;
			}
			if (memberExpression == null) {
				throw new ArgumentException("Not a member access", "expression");
			}
			return memberExpression;
		}

		public IEnumerable<FieldSettings> Fields {
			get {
				return InnerFields;
			}
		}

		internal abstract T ParseLine(string line);

		internal abstract string BuildLine(T value);

		protected object GetFieldValueFromString(FieldSettings fieldSettings, string memberValue) {
			if (fieldSettings.IsNullable && memberValue.Equals(fieldSettings.NullValue)) {
				return null;
			}
			memberValue = fieldSettings.PadLeft
							? memberValue.TrimStart(new char[] { fieldSettings.PaddingChar })
							: memberValue.TrimEnd(new char[] { fieldSettings.PaddingChar });
			if (fieldSettings.PropertyInfo.PropertyType.IsGenericType
				&& fieldSettings.PropertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
				return Convert.ChangeType(memberValue, Nullable.GetUnderlyingType(fieldSettings.PropertyInfo.PropertyType));
			}
			return Convert.ChangeType(memberValue, fieldSettings.PropertyInfo.PropertyType);
		}

		protected string GetStringValueFromField(FieldSettings field, object fieldValue) {
			if (fieldValue == null) {
				return field.NullValue;
			}
			string lineValue = fieldValue.ToString();
			if (lineValue.Length < field.Lenght) {
				lineValue = field.PadLeft
								? lineValue.PadLeft(field.Lenght, field.PaddingChar)
								: lineValue.PadRight(field.Lenght, field.PaddingChar);
			}
			return lineValue;
		}

	}
}
