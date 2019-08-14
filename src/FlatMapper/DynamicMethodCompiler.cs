using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace FlatMapper
{

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
            //Try to find a default parameterless constructor
            ConstructorInfo constructorInfo = type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Length == 0);

            if (constructorInfo == null)
            {
                throw new Exception(string.Format("The type {0} must declare an empty constructor(the constructor may be private, internal, protected, protected internal, or public).", type));
            }

            var contructorExp = Expression.New(constructorInfo);
            var newLambdaExp = Expression.Lambda<Func<T>>(contructorExp);
            return (Func<T>) newLambdaExp.Compile();
        }

        /// <summary>
        /// This creates a function that will return the property value
        /// same as return type.Property;
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        internal static Func<T, object> CreateGetHandler<T>(PropertyInfo propertyInfo)
        {
            var entity = Expression.Parameter(propertyInfo.DeclaringType);
            var getMethodInfo = propertyInfo.GetGetMethodInfo();

            var callGetExpression = Expression.Convert(Expression.Call(entity, getMethodInfo), typeof(object));
            var getExpression = Expression.Lambda(callGetExpression, entity);

            return (Func<T, object>)getExpression.Compile();
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
            var objParameter = Expression.Parameter(propertyInfo.DeclaringType, "object");
            var valueParameter = Expression.Parameter(typeof(object), "value");
            var propertyExp = Expression.Property(objParameter, propertyInfo);

            var assignExp = Expression.Assign(propertyExp, Expression.Convert(valueParameter, propertyInfo.PropertyType));

            return Expression.Lambda<Action<T, object>>
                (assignExp, objParameter, valueParameter).Compile();
        }
    }
}

