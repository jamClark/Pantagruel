using UnityEngine;
using System.Collections;
using System.Reflection;
using System;
using UnityEngine.Events;

namespace Pantagruel.Test
{
    /// <summary>
    /// 
    /// </summary>
    public class ReflectinatorComponent : MonoBehaviour
    {
        public UnityEngine.Object Obj;
        
        void Start()
        {
           
            Reflectinator.Reflect(Obj, Obj.GetType());
            //Sprite temp = Resources.Load(loc, typeof(Sprite)) as Sprite;
            //if (temp != null) Debug.Log("Found!");
            //else Debug.Log("Missing");
            
        }
    }
}
