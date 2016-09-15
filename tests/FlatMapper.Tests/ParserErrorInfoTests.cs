using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FlatMapper.Tests
{
    public class ParserErrorInfoTests
    {
        public class TestObject
        {
            public string FirstField { get; set; }
            public int SecondField { get; set; }
        }

        [Fact]
        public void InvalidDataGivesDetailedInfo()
        {
            var layout = new Layout<TestObject>.DelimitedLayout()
                .WithDelimiter(";")
                .WithMember(o => o.FirstField)
                .WithMember(o => o.SecondField);

            var completeString = "\"teste\";\"invalidData\";";
            try
            {
                var result = layout.ParseLine(completeString);              
            }
            catch (ParserErrorException ex)
            {
                Assert.Equal(ex.ParserErrorInfo.FieldName, "SecondField");
                Assert.Equal(ex.ParserErrorInfo.Line, completeString);
                Assert.Equal(ex.ParserErrorInfo.FieldType, typeof(int));
                return;
            }
            throw new Exception("We should have thrown a ParserErrorException");
        }
    }
}
