using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlatMapper
{
    public class FieldValueConverter<TMember> : IFieldValueConverter
    {
        protected Type TargetType { get; private set; }

        public FieldValueConverter()
        {
            TargetType = typeof(TMember);
            if (TargetType.IsGenericType && TargetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                TargetType = Nullable.GetUnderlyingType(TargetType);
            }
        }

        public virtual object FromString(string value)
        {
            if (TargetType.IsEnum)
            {
                return Enum.Parse(TargetType, value);
            }
            return Convert.ChangeType(value, TargetType);
        }

        public virtual string FromValue(object value)
        {
            return value.ToString();
        }
    }
}
