#region License

// 
// Copyright (c) 2011-2015, João Matos Silva <kappy@acydburne.com.pt>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 

#endregion
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
