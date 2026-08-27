using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ParrotAgent.Kernel
{
    public static class Reflection
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
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T CreateInstance<T, A>(Type type, A args)
        {
            return (T)Activator.CreateInstance(type, args);
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

            var assemblies = AppDomain.CurrentDomain.GetReferenceAssemblies();
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

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray(); // 即使部分类型因依赖缺失无法加载，仍使用已成功加载的部分类型继续
            }
            catch (FileNotFoundException)
            {
                return list; // 程序集本身无法加载其类型定义时，跳过该程序集
            }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        private static HashSet<Assembly> GetReferenceAssemblies(this AppDomain domain)
        {
            var hashset = new HashSet<Assembly>();

            var assemblies = domain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                hashset.Add(assembly);
                GetReferenceAssemblies(assembly, hashset);
            }

            return hashset;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="hashset"></param>
        private static void GetReferenceAssemblies(Assembly assembly, HashSet<Assembly> hashset)
        {
            var assemblyNames = assembly.GetReferencedAssemblies();
            foreach (var assemblyName in assemblyNames)
            {
                Assembly ass;
                try
                {
                    ass = Assembly.Load(assemblyName);
                }
                catch (FileNotFoundException)
                {
                    continue;// 引用解析失败（如平台专属包/版本缺失/运行时不匹配），跳过
                }
                catch (FileLoadException)
                {
                    continue;
                }
                catch (BadImageFormatException)
                {
                    continue;// 非托管/格式不兼容的程序集，跳过
                }

                if (hashset.Add(ass))
                {
                    GetReferenceAssemblies(ass, hashset);
                }
            }
        }
    }
}
