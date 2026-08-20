using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace ParrotAgent.Kenel
{
    internal static class Reflection
    {
        /// <summary>
        /// 实例化T类型的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateInstance<T>()
        {
            return Activator.CreateInstance<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T CreateInstance<T>(Type type)
        {
            return (T)Activator.CreateInstance(type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TBase"></typeparam>
        /// <typeparam name="TAttribute"></typeparam>
        /// <returns></returns>
        public static List<Type> FindAll<TBase, TAttribute>() where TAttribute : Attribute
        {
            var list = new List<Type>();

            var assemblies = AssemblyLoadContext.Default.Assemblies; //AppDomain.CurrentDomain.GetReferanceAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = FindAll<TBase, TAttribute>(assembly);
                if (types.Count > 0) list.AddRange(types);
            }

            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <typeparam name="TBase"></typeparam>
        /// <typeparam name="TAttribute"></typeparam>
        /// <returns></returns>
        private static List<Type> FindAll<TBase, TAttribute>(Assembly assembly) where TAttribute : Attribute
        {
            var list = new List<Type>();

            var baseType = typeof(TBase);
            var attrType = typeof(TAttribute);

            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (type.IsAbstract || type.IsInterface || type.IsEnum) continue;
                // 不是从TBase继承
                if (!baseType.IsAssignableFrom(type)) continue;

                var objects = type.GetCustomAttributes(attrType, false);
                if (objects.Length == 0) continue;

                list.Add(type);
            }

            return list;
        }
    }
}
