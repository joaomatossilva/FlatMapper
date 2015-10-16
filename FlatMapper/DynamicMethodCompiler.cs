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
                CallingConventions.Standard, typeof(object), null, type, true);

            ILGenerator generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Newobj, constructorInfo);
            generator.Emit(OpCodes.Ret);
            var handler =  (Func<object>)dynamicMethod.CreateDelegate(typeof(Func<object>));

            //TODO: Fix Il code to return a Func<T>
            return () => (T) handler();
        }

        /// <summary>
        /// This creates a funcion that will return the property value
        /// same as return type.Property;
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        internal static Func<object,object> CreateGetHandler(Type type, PropertyInfo propertyInfo)
        {
            MethodInfo getMethodInfo = propertyInfo.GetGetMethod(true);
            DynamicMethod dynamicGet = CreateGetDynamicMethod(type);
            ILGenerator getGenerator = dynamicGet.GetILGenerator();

            getGenerator.Emit(OpCodes.Ldarg_0);
            getGenerator.Emit(OpCodes.Call, getMethodInfo);
            BoxIfNeeded(getMethodInfo.ReturnType, getGenerator);
            getGenerator.Emit(OpCodes.Ret);

            return (Func<object, object>)dynamicGet.CreateDelegate(typeof(Func<object, object>));
        }

        /// <summary>
        /// This creates a function that will set the property value
        /// same as type.Property = value;
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        internal static Action<object, object> CreateSetHandler(Type type, PropertyInfo propertyInfo)
        {
            MethodInfo setMethodInfo = propertyInfo.GetSetMethod(true);
            DynamicMethod dynamicSet = CreateSetDynamicMethod(type);
            ILGenerator setGenerator = dynamicSet.GetILGenerator();

            setGenerator.Emit(OpCodes.Ldarg_0);
            setGenerator.Emit(OpCodes.Ldarg_1);
            UnboxIfNeeded(setMethodInfo.GetParameters()[0].ParameterType, setGenerator);
            setGenerator.Emit(OpCodes.Call, setMethodInfo);
            setGenerator.Emit(OpCodes.Ret);

            return (Action<object, object>)dynamicSet.CreateDelegate(typeof(Action<object, object>));
        }

        private static DynamicMethod CreateGetDynamicMethod(Type type)
        {
            return new DynamicMethod("DynamicGet", typeof(object),
                  new Type[] { typeof(object) }, type, true);
        }

        private static DynamicMethod CreateSetDynamicMethod(Type type)
        {
            return new DynamicMethod("DynamicSet", typeof(void),
                  new Type[] { typeof(object), typeof(object) }, type, true);
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
