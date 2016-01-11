using System;

namespace FlatMapper
{
    public interface IFieldValueConverter
    {
        object FromString(string value, IFormatProvider formatProvider);
        string FromValue(object value, IFormatProvider formatProvider);
    }
}