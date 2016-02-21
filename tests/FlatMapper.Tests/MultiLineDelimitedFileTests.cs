using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Shouldly;
using Xunit;

namespace FlatMapper.Tests
{
    public class MultiLineDelimitedFileTests
    {

        private Layout<TestObject> layout;

        private IList<TestObject> objects;

        public MultiLineDelimitedFileTests()
        {
            layout = new Layout<TestObject>.DelimitedLayout()
                    .WithDelimiter(";")
                    .WithQuote("\"")
                    .HeaderLines(2)
                    .WithMultiLine(true)
                    .WithMember(o => o.Id, set => set.WithLength(5).WithLeftPadding('0'))
                    .WithMember(o => o.Description, set => set.WithLength(25).WithRightPadding(' '))
                    .WithMember(o => o.NullableInt, set => set.WithLength(5).AllowNull("=Null").WithLeftPadding('0'))
                    .WithMember(o => o.NullableEnum, set => set.WithLength(10).AllowNull("").WithLeftPadding(' '));

            objects = new List<TestObject>();
            for (int i = 1; i <= 10; i++)
            {
                objects.Add(new TestObject { Id = i, Description = "Description " + i + "\r\nmulti\r\nline", NullableInt = i % 5 == 0 ? null : (int?)3, NullableEnum = i % 3 == 0 ? null : (Gender?)(i % 3) });
            }
        }

        [Fact]
        public void can_write_read_stream()
        {
            using (var memory = new MemoryStream())
            {
                var flatFile = new FlatFile<TestObject>(layout, memory, HandleEntryReadError);
                flatFile.Write(objects);

                memory.Seek(0, SeekOrigin.Begin);

                var objectsAfterRead = flatFile.Read().ToList();

                //TODO: improve this with xunit
                true.ShouldBe(objects.SequenceEqual(objectsAfterRead));
            }
        }

        private bool HandleEntryReadError(string s, Exception exception)
        {
            return false;
        }

        [Fact]
        public void can_read_string_with_delimiter_on_text()
        {
            var testString = "this;is;the;test";
            var completeString = "\"12\";\"" + testString + "\";\"=Null\"";
            var result = layout.ParseLine(completeString);
            result.Description.ShouldBe(testString);
        }
    }
}
