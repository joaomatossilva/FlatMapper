using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FlatMapper {
	public class FlatFile<T> : IDisposable where T : new() {
		private readonly Layout<T> layout;

		private readonly Stream innerStream;

		private readonly Func<string, Exception, bool> handleEntryReadError;

		public FlatFile(Layout<T> layout, Stream innerStream, Func<string, Exception, bool> handleEntryReadError) {
			this.layout = layout;
			this.innerStream = innerStream;
			this.handleEntryReadError = handleEntryReadError;
		}

		public IEnumerable<T> Read() {
			var reader = new StreamReader(this.innerStream);
			string line;
			while ((line = reader.ReadLine()) != null) {
				bool ignoreEntry = false;
				var entry = new T();
				int linePosition = 0;
				try {
					foreach (var field in layout.Fields) {
						string fieldValueFromLine = line.Substring(linePosition, field.Lenght);
						var convertedFieldValue = GetFieldValueFromString(field, fieldValueFromLine);
						field.PropertyInfo.SetValue(entry, convertedFieldValue, null);
						linePosition += field.Lenght;
					}
				} catch (Exception ex) {
					if (!handleEntryReadError(line, ex)) {
						throw;
					}
					ignoreEntry = true;
				}
				if (!ignoreEntry) {
					yield return entry;
				}
			}
		}

		public void Write(IEnumerable<T> entries) {
			TextWriter writer = new StreamWriter(this.innerStream);
			foreach (var entry in entries) {
				string line = layout.Fields.Aggregate(
					string.Empty,
					(current, field) => current + GetStringValueFromField(field, field.PropertyInfo.GetValue(entry, null)));
				writer.WriteLine(line);
				writer.Flush();
			}
		}

		private object GetFieldValueFromString(FieldSettings fieldSettings, string memberValue) {
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

		private string GetStringValueFromField(FieldSettings field, object fieldValue) {
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

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing) {
			if (disposing) {
				if (this.innerStream != null) {
					this.innerStream.Dispose();
				}
			}
		}

		~FlatFile() {
			Dispose(false);
		}
	}
}
