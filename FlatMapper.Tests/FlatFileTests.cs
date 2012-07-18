using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace FlatMapper.Tests
{
	public class TestObject : IEquatable<TestObject> {
		public int Id { get; set; }

		public string Description { get; set; }

		public int? NullableInt { get; set; }

		public int GetHashCode(TestObject obj) {
			var idHash = Id.GetHashCode();
			var descriptionHash = Object.ReferenceEquals(Description, null) ? 0 : Description.GetHashCode();
			var nullableIntHash = !NullableInt.HasValue ? 0 : NullableInt.Value.GetHashCode();
			return idHash ^ descriptionHash ^ nullableIntHash;
		}

		public bool Equals(TestObject other) {
			if (ReferenceEquals(other, null)) {
				return false;
			}

			if (ReferenceEquals(other, this)) {
				return true;
			}

			return Equals(Id, other.Id) && Equals(Description, other.Description) && Equals(NullableInt, other.NullableInt);
		}
	}

	[TestFixture]
    public class FlatFileTests {

		private Layout<TestObject> layout;

		private IList<TestObject> objects;
			
		[SetUp]
		public void init_layout() {
			layout = new Layout<TestObject>.FixedLengthLayout()
					.WithMember(o => o.Id, set => set.WithLenght(5).WithLeftPadding('0'))
					.WithMember(o => o.Description, set => set.WithLenght(25).WithRightPadding(' '))
					.WithMember(o => o.NullableInt, set => set.WithLenght(5).AllowNull("=Null").WithLeftPadding('0'));

			objects = new List<TestObject>();
			for (int i = 1; i <= 10; i++) {
				objects.Add(new TestObject { Id = i, Description = "Description " + i, NullableInt = i % 5 == 0 ? null: (int?)3 });
			}
		}

		[Test]
		public void can_write_read_stream() {
			using (var memory = new MemoryStream()) {
				using(var flatFile = new FlatFile<TestObject>(layout, memory, HandleEntryReadError)) {
					flatFile.Write(objects);
					
					memory.Seek(0, SeekOrigin.Begin);

					var objectsAfterRead = flatFile.Read().ToList();

					Assert.IsTrue(objects.SequenceEqual(objectsAfterRead));
				}
			}
		}

		private bool HandleEntryReadError(string s, Exception exception) {
			return true;
		}
    }
}
