using System;
using System.Reflection;
using System.Reflection.Emit;

namespace FlatMapper
{
    //This code is based on Herbrandson artice on Code Project - Dynamic Code Generation vs Reflection
    //http://www.codeproject.com/KB/cs/Dynamic_Code_Generation.aspx

    public static class DynamicMethodCompiler
    {
        /// <summary>
        /// This creates a function that will call the parameterless constructor of type T
        /// same as return new Type();
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static Func<T> CreateInstantiateObjectHandler<T>()
        {
            Type type = typeof (T);
            ConstructorInfo constructorInfo = type.GetConstructor(BindingFlags.Public |
                   BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null);

            if (constructorInfo == null)
            {
                throw new ApplicationException(string.Format("The type {0} must declare an empty constructor(the constructor may be private, internal, protected, protected internal, or public).", type));
            }

            DynamicMethod dynamicMethod = new DynamicMethod("InstantiateObject", MethodAttributes.Static | MethodAttributes.Public, 
                CallingConventions.Standard, typeof(T), null, type, true);

            ILGenerator generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Newobj, constructorInfo);
            generator.Emit(OpCodes.Ret);

            return (Func<T>)dynamicMethod.CreateDelegate(typeof(Func<T>));
        }

        /// <summary>
        /// This creates a funcion that will return the property value
        /// same as return type.Property;
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        internal static Func<T, object> CreateGetHandler<T>(PropertyInfo propertyInfo)
        {
            var type = typeof (T);
            MethodInfo getMethodInfo = propertyInfo.GetGetMethod(true);
            DynamicMethod dynamicGet = CreateGetDynamicMethod<T>();
            ILGenerator getGenerator = dynamicGet.GetILGenerator();

            getGenerator.Emit(OpCodes.Ldarg_0);
            getGenerator.Emit(OpCodes.Call, getMethodInfo);
            BoxIfNeeded(getMethodInfo.ReturnType, getGenerator);
            getGenerator.Emit(OpCodes.Ret);

            return (Func<T, object>)dynamicGet.CreateDelegate(typeof(Func<T, object>));
        }

        /// <summary>
        /// This creates a function that will set the property value
        /// same as type.Property = value;
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        internal static Action<T, object> CreateSetHandler<T>(PropertyInfo propertyInfo)
        {
            var type = typeof(T);
            MethodInfo setMethodInfo = propertyInfo.GetSetMethod(true);
            DynamicMethod dynamicSet = CreateSetDynamicMethod<T>();
            ILGenerator setGenerator = dynamicSet.GetILGenerator();

            setGenerator.Emit(OpCodes.Ldarg_0);
            setGenerator.Emit(OpCodes.Ldarg_1);
            UnboxIfNeeded(setMethodInfo.GetParameters()[0].ParameterType, setGenerator);
            setGenerator.Emit(OpCodes.Call, setMethodInfo);
            setGenerator.Emit(OpCodes.Ret);

            return (Action<T, object>)dynamicSet.CreateDelegate(typeof(Action<T, object>));
        }

        private static DynamicMethod CreateGetDynamicMethod<T>()
        {
            return new DynamicMethod("DynamicGet", typeof(object),
                  new Type[] { typeof(T) }, typeof(T), true);
        }

        private static DynamicMethod CreateSetDynamicMethod<T>()
        {
            return new DynamicMethod("DynamicSet", typeof(void),
                  new Type[] { typeof(T), typeof(object) }, typeof(T), true);
        }

        private static void BoxIfNeeded(Type type, ILGenerator generator)
        {
            if (type.IsValueType)
            {
                generator.Emit(OpCodes.Box, type);
            }
        }

        private static void UnboxIfNeeded(Type type, ILGenerator generator)
        {
            if (type.IsValueType)
            {
                generator.Emit(OpCodes.Unbox_Any, type);
            }
        }
    }
}
