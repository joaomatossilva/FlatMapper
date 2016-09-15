using System;

namespace FlatMapper
{
    public class ParserErrorInfo
    {
        public string Line { get; internal set; }
        public string FieldName { get; internal set; }
        public Type FieldType { get; internal set; }
        public string ErrorMessage { get; internal set; }
    }
}
