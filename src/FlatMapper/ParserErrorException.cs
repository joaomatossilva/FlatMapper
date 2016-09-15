using System;

namespace FlatMapper
{
    public class ParserErrorException : Exception
    {
        public ParserErrorException(ParserErrorInfo parserErrorInfo)
            : base(BuildErrorMessage(parserErrorInfo))
        {
            ParserErrorInfo = parserErrorInfo;
        }

        public ParserErrorException(ParserErrorInfo parserErrorInfo, Exception innerException)
            : base(BuildErrorMessage(parserErrorInfo), innerException)
        {
            ParserErrorInfo = parserErrorInfo;
        }

        public ParserErrorInfo ParserErrorInfo { get; private set; }

        private static string BuildErrorMessage(ParserErrorInfo parserErrorInfo)
        {
            return $"Error Parsing field {parserErrorInfo.FieldName}: {parserErrorInfo.ErrorMessage}";
        }
    }
}
