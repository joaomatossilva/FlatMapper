using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Xunit;
using Shouldly;

namespace FlatMapper.Tests
{
    public class FixedLengthFileTests
    {

        private Layout<TestObject> layout;

        private IList<TestObject> objects;

        public FixedLengthFileTests()
        {
            layout = new Layout<TestObject>.FixedLengthLayout()
                    .HeaderLines(2)
                    .WithMember(o => o.Id, set => set.WithLength(5).WithLeftPadding('0'))
                    .WithMember(o => o.Description, set => set.WithLength(25).WithRightPadding(' '))
                    .WithMember(o => o.NullableInt, set => set.WithLength(5).AllowNull("=Null").WithLeftPadding('0'))
                    .WithMember(o => o.NullableEnum, set => set.WithLength(10).AllowNull("======NULL").WithLeftPadding(' '))
                    .WithMember(o => o.Date, set => set.WithLength(19).WithFormat(new CultureInfo("pt-PT"))); //PT-pt default dates are always fixed 19 chars "13-12-2015 23:41:41"

            objects = new List<TestObject>();
            for (int i = 1; i <= 10; i++)
            {
                objects.Add(new TestObject { Id = i, Description = "Description " + i, NullableInt = i % 5 == 0 ? null : (int?)3, NullableEnum = i % 3 == 0 ? null : (Gender?)(i % 3) });
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

                Assert.True(objects.SequenceEqual(objectsAfterRead));
            }
        }

        private bool HandleEntryReadError(ParserErrorInfo errorInfo, Exception exception)
        {
            return false;
        }
    }
}
