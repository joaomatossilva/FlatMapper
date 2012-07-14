using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FlatMapper {
	public class Layout<T> {

		private readonly List<FieldSettings> fields = new List<FieldSettings>();

		public Layout<T> WithMember<TMType>(Expression<Func<T, TMType>> expression, Action<IFieldSettings> settings) {
			var memberExpression = GetMemberExpression(expression);
			var fieldSettings = new FieldSettings(memberExpression.Member as PropertyInfo);
			settings(fieldSettings);
			fields.Add(fieldSettings);
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
				return fields;
			}
		}
	}
}
