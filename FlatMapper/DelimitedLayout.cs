using System;
using System.Linq;
using System.Linq.Expressions;

namespace FlatMapper
{
    public abstract partial class Layout<T> where T : new()
    {
        public class DelimitedLayout : Layout<T>
        {

            private string Quotes { get; set; }

            private string innerDelimiter = ",";

            public new DelimitedLayout HeaderLines(int count)
            {
                return (DelimitedLayout)base.HeaderLines(count);
            }

            public new DelimitedLayout WithMember<TMType>(Expression<Func<T, TMType>> expression, Action<IFieldSettings> settings)
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

            internal override T ParseLine(string line)
            {
                var entry = new T();
                int linePosition = 0;
                int delimiterSize = Delimiter.Length;
                foreach (var field in this.Fields)
                {
                    int nextDelimiterIndex = -1;
                    if (line.Length > linePosition + delimiterSize)
                    {
                        nextDelimiterIndex = line.IndexOf(Delimiter, linePosition, StringComparison.InvariantCultureIgnoreCase);
                    }
                    int fieldLenght;
                    if (nextDelimiterIndex > -1)
                    {
                        fieldLenght = nextDelimiterIndex - linePosition;
                    }
                    else
                    {
                        fieldLenght = line.Length - linePosition;
                    }
                    string fieldValueFromLine = line.Substring(linePosition, fieldLenght);
                    var convertedFieldValue = GetFieldValueFromString(field, fieldValueFromLine);
                    field.PropertyInfo.SetValue(entry, convertedFieldValue, null);
                    linePosition += fieldLenght + (nextDelimiterIndex > -1 ? delimiterSize : 0);
                }
                return entry;
            }

            internal override string BuildLine(T entry)
            {
                string line = this.Fields.Aggregate(string.Empty,
                    (current, field) => current + (current.Length > 0 ? innerDelimiter : "") + GetStringValueFromField(field, field.PropertyInfo.GetValue(entry, null)));
                return line;
            }

            internal override string BuildHeaderLine()
            {
                string line = this.Fields.Aggregate(string.Empty,
                    (current, field) => current + (current.Length > 0 ? innerDelimiter : "") + field.PropertyInfo.Name);
                return line;
            }

            protected override object GetFieldValueFromString(FieldSettings fieldSettings, string memberValue)
            {
                if (!string.IsNullOrEmpty(Quotes))
                {
                    memberValue = memberValue.Replace(Quotes, String.Empty);
                }
                return base.GetFieldValueFromString(fieldSettings, memberValue);
            }

            protected override string GetStringValueFromField(FieldSettings field, object fieldValue)
            {
                var stringValue = base.GetStringValueFromField(field, fieldValue);
                if (!string.IsNullOrEmpty(Quotes))
                {
                    stringValue = string.Format("{0}{1}{0}", Quotes, stringValue);
                }
                return stringValue;
            }
        }
    }
}
