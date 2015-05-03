using System.Reflection;

namespace FlatMapper
{
    public class FieldSettings : IFieldSettings
    {

        public int Lenght { get; set; }
        public char PaddingChar { get; set; }
        public bool IsNullable { get; set; }
        public string NullValue { get; set; }
        public bool PadLeft { get; set; }
        public PropertyInfo PropertyInfo { get; set; }


        public FieldSettings(PropertyInfo propertyInfo)
        {
            this.PropertyInfo = propertyInfo;
        }

        public IFieldSettings WithLenght(int lenght)
        {
            this.Lenght = lenght;
            return this;
        }

        public IFieldSettings WithLeftPadding(char paddingChar)
        {
            this.PaddingChar = paddingChar;
            this.PadLeft = true;
            return this;
        }

        public IFieldSettings WithRightPadding(char paddingChar)
        {
            this.PaddingChar = paddingChar;
            this.PadLeft = false;
            return this;
        }

        public IFieldSettings AllowNull(string nullValue)
        {
            this.IsNullable = true;
            this.NullValue = nullValue;
            return this;
        }
    }
}
