namespace FlatMapper
{
    public interface IFieldValueConverter
    {
        object FromString(string value);
        string FromValue(object value);
    }
}