using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FlatMapper
{
    public class FieldValueConverter<TMember> : IFieldValueConverter
    {
        private static readonly Type StringType = typeof(string);
        protected Type TargetType { get; private set; }

        public FieldValueConverter()
        {
            TargetType = typeof(TMember);
#if NET35
            var isGeneric = TargetType.IsGenericType;
#else
            var isGeneric = TargetType.GetTypeInfo().IsGenericType;
#endif
            if (isGeneric && TargetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                TargetType = Nullable.GetUnderlyingType(TargetType);
            }
        }

        public virtual object FromString(string value, IFormatProvider formatProvider)
        {
#if NET35
            var isEnum = TargetType.IsEnum;
#else
            var isEnum = TargetType.GetTypeInfo().IsEnum;
#endif            
            if (isEnum)
            {
                return Enum.Parse(TargetType, value);
            }
            return Convert.ChangeType(value, TargetType, formatProvider);
        }

        public virtual string FromValue(object value, IFormatProvider formatProvider)
        {
            if (value == null)
            {
                return null;
            }
            return (string)Convert.ChangeType(value, StringType, formatProvider);
        }
    }
}
