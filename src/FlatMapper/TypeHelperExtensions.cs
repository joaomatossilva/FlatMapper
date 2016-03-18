#region License

// 
// Copyright (c) 2011-2016, João Matos Silva <kappy@acydburne.com.pt>
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
using System;
using System.Reflection;

namespace FlatMapper
{
    internal static class TypeHelperExtensions
    {
        internal static MethodInfo GetSetMethodInfo(this PropertyInfo propertyInfo)
        {
#if NET35
            return propertyInfo.GetSetMethod(true);
#else
            return propertyInfo.SetMethod;
#endif
        }

        internal static MethodInfo GetGetMethodInfo(this PropertyInfo propertyInfo)
        {
#if NET35
            return propertyInfo.GetGetMethod(true);
#else
            return propertyInfo.GetMethod;
#endif
        }

        internal static bool IsValueType(this Type type)
        {
#if NET35
            return type.IsValueType;
#else
            return type.GetTypeInfo().IsValueType;
#endif
        }

        internal static bool IsGenericType(this Type type)
        {
#if NET35
            return type.IsGenericType;
#else
            return type.GetTypeInfo().IsGenericType;
#endif
        }

        internal static bool IsEnum(this Type type)
        {
#if NET35
            return type.IsEnum;
#else
            return type.GetTypeInfo().IsEnum;
#endif
        }
    }
}
