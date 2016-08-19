using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlatMapper.Tests
{
    public enum Gender
    {
        Male = 1,
        Female = 2
    }

    public class TestObject : IEquatable<TestObject>
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public int? NullableInt { get; set; }

        public Gender? NullableEnum { get; set; }

        public DateTime Date { get; set; }

        public int GetHashCode(TestObject obj)
        {
            var idHash = Id.GetHashCode();
            var descriptionHash = Object.ReferenceEquals(Description, null) ? 0 : Description.GetHashCode();
            var nullableIntHash = !NullableInt.HasValue ? 0 : NullableInt.Value.GetHashCode();
            var nullableEnum = !NullableEnum.HasValue ? 0 : NullableEnum.Value.GetHashCode();
            return idHash ^ descriptionHash ^ nullableIntHash ^ nullableEnum ^ Date.GetHashCode();
        }

        public bool Equals(TestObject other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (ReferenceEquals(other, this))
            {
                return true;
            }

            return Equals(Id, other.Id) && Equals(Description, other.Description) && Equals(NullableInt, other.NullableInt);
        }
    }
}
