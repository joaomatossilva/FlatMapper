using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace FlatMapper.Tests
{
    [TestFixture]
    public class FixedLengthFileTests
    {

        private Layout<TestObject> layout;

        private IList<TestObject> objects;

        [SetUp]
        public void init_layout()
        {
            layout = new Layout<TestObject>.FixedLengthLayout()
                    .HeaderLines(2)
                    .WithMember(o => o.Id, set => set.WithLength(5).WithLeftPadding('0'))
                    .WithMember(o => o.Description, set => set.WithLength(25).WithRightPadding(' '))
                    .WithMember(o => o.NullableInt, set => set.WithLength(5).AllowNull("=Null").WithLeftPadding('0'));

            objects = new List<TestObject>();
            for (int i = 1; i <= 10; i++)
            {
                objects.Add(new TestObject { Id = i, Description = "Description " + i, NullableInt = i % 5 == 0 ? null : (int?)3 });
            }
        }

        [Test]
        public void can_write_read_stream()
        {
            using (var memory = new MemoryStream())
            {
                var flatFile = new FlatFile<TestObject>(layout, memory, HandleEntryReadError);
                flatFile.Write(objects);

                memory.Seek(0, SeekOrigin.Begin);

                var objectsAfterRead = flatFile.Read().ToList();

                Assert.IsTrue(objects.SequenceEqual(objectsAfterRead));
            }
        }

        private bool HandleEntryReadError(string s, Exception exception)
        {
            return true;
        }
    }
}
