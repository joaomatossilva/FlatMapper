namespace FlatMapper
{
    public interface IFieldSettings
    {
        IFieldSettings WithLenght(int lenght);

        IFieldSettings WithLeftPadding(char paddingChar);

        IFieldSettings WithRightPadding(char paddingChar);

        IFieldSettings AllowNull(string nullValue);
    }
}
