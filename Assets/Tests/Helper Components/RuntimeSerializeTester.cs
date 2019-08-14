using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Xml;
using Pantagruel.Serializer;
using Pantagruel.Serializer.Surrogate;

namespace Pantagruel.Serializer.Test
{
    /// <summary>
    /// 
    /// </summary>
    public class RuntimeSerializeTester : MonoBehaviour
    {
        public GameObject Go;

        string path = "";

        void Awake()
        {
            path = Application.persistentDataPath + System.IO.Path.DirectorySeparatorChar + "player1.xml";
            
        }

        void Update()
        {
            if (Go != null && Input.GetKeyDown(KeyCode.S))
            {
                var player = Go;
                XmlDocument doc = XmlSerializer.Serialize(player, 1);

                Debug.Log("<color=blue>Saved to:</color> " + path);
                doc.Save(path);
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                Debug.Log("<color=green>Loaded From:</color> " + path);
                string text = System.IO.File.ReadAllText(path);

                //we are deserializing a gameobject here. 
                //No need to keep the reference it will return
                XmlDeserializer.Deserialize(text, 1);
            }
        }

    }
}