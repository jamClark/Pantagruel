using UnityEngine;
using System.Collections;
using System.Reflection;
using System;
using System.Text;

namespace Pantagruel.Test
{
    public static class Reflectinator
    {
        static BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Prints lots of reflection info.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        public static void Reflect(object obj, Type type)
        {
            Debug.Log("<color=green>-------------- Reflecting on: "+type.Name+"---------------</color>");
            Type objType = type;
            MemberInfo[] info = objType.GetMembers(flags);

            foreach (var mem in info)
            {
                Debug.Log(MemberType(mem) + ": " + Name(mem, obj) + " of type " + mem.ReflectedType + " with these attributes: " + Attrs(mem));
                if (mem.Name == "m_InvokeArray")
                {
                    FieldInfo f = (FieldInfo)mem;
                    object[] arr = f.GetValue(obj) as object[];
                    Debug.Log("Count: " + arr.Length);
                    foreach (var o in arr)
                    {
                        Debug.Log("Is: " + o);
                    }
                }
            }

            // check base class as well
            Type baseType = objType.BaseType;
            if (baseType != null && baseType != typeof(object))
            {
                Debug.Log("<color=blue>---------------------Reflecting base type: " + baseType.Name + "---------------------</color>");

                Reflect(obj, baseType);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mem"></param>
        /// <returns></returns>
        static string Attrs(MemberInfo mem)
        {
            StringBuilder s = new StringBuilder();

            s.Append("<color=cyan>");
            foreach(var attr in mem.GetCustomAttributes(false))
            {
                //Attribute a = attr as Attribute;
                s.Append(", [");
                s.Append(attr.ToString());
                s.Append("] ");
            }
            s.Append("</color>");
            s.Append(" ");
            return s.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mem"></param>
        /// <returns></returns>
        static string MemberType(MemberInfo mem)
        {
            if (mem.MemberType == MemberTypes.Property)
                return "<color=green>Property:</color>";
            else if (mem.MemberType == MemberTypes.Field)
                return "<color=blue>Field:</color>";
            else if (mem.MemberType == MemberTypes.Event)
                return "<color=cyan>Event:</color>";
            else return "<color=magenta>Method:</color>";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mem"></param>
        /// <returns></returns>
        static string Name(MemberInfo mem, object obj)
        {
            string paramInfos = "";
            string valueInfo = "null";

            int pub = 1;
            if (mem.MemberType == MemberTypes.Property)
            {
                //var m = (PropertyInfo)mem;
                pub = 1;
                if (obj != null)
                {
                    //object o = m.GetValue(obj, null);
                    //if (o != null) valueInfo = o.ToString();
                }
            }
            else if (mem.MemberType == MemberTypes.Field)
            {
                var m = (FieldInfo)mem;
                if (m.IsPublic) pub = 1;
                else if (m.IsPrivate) pub = -1;
                else pub = 0;
                if (obj != null)
                {
                    object o = m.GetValue(obj);
                    if (o != null) valueInfo = o.ToString();
                }
            }
            else if (mem.MemberType == MemberTypes.Method)
            {
                var m = (MethodInfo)mem;
                if (m.IsPublic) pub = 1;
                else if (m.IsPrivate) pub = -1;
                else pub = 0;

                paramInfos = "(";
                foreach (var info in m.GetParameters())
                {
                    paramInfos += info.ParameterType + " " + info.Name + ",";
                }
                paramInfos += ")";
            }

            string color;
            if (pub == -1) color = "red";
            else if (pub == 0) color = "yellow";
            else color = "black";

            if (mem.MemberType == MemberTypes.Method)
            {
                return "<color=" + color + ">" + mem.Name + paramInfos + ":</color>";

            }
            return "<color=" + color + ">" + mem.Name + ":</color> = " + valueInfo + "; ";

        }

    }
}