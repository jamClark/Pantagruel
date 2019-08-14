using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System;

namespace Pantagruel.Serializer.Test
{
    /// <summary>
    /// Unit tests for checking serialization of general data.
    /// </summary>
    public class BasicTypeTester: SerializerTesterBase
    {
        /// <summary>
        /// Tests if various multidimensional arrays of ints are serialized correctly.
        /// </summary>
        [Test]
        public void TestMultiDimIntArray_Init()
        {
            int[,] sarr1 = new int[2, 4];
            var doc = XmlSerializer.Serialize(sarr1, 1, "test1");
            Assert.NotNull(doc);

            int[,] darr1 = XmlDeserializer.Deserialize(doc.InnerXml, 1) as int[,];
            Assert.NotNull(darr1);
            Assert.AreNotSame(sarr1, darr1);
            Assert.AreEqual(darr1.Rank, 2);

        }

        [Test]
        public void TestMultiDimIntArray_Lengths()
        {
            int[,,] sarr1 = new int[2, 3, 5];
            var doc = XmlSerializer.Serialize(sarr1, 1, "test1");

            int[,,] darr1 = XmlDeserializer.Deserialize(doc.InnerXml, 1) as int[,,];
            Assert.NotNull(darr1);
            Assert.AreNotSame(sarr1, darr1);
            Assert.AreEqual(3, darr1.Rank);
            Assert.AreEqual(2, darr1.GetLength(0));
            Assert.AreEqual(3, darr1.GetLength(1));
            Assert.AreEqual(5, darr1.GetLength(2));
        }

        [Test]
        public void TestMultiDimIntArray_Values()
        {
            int val = 0;
            int[,,] sarr1 = new int[2, 3, 5];
            IterateArrayWithValues(sarr1, 0, new int[sarr1.Rank], ref val);

            var doc = XmlSerializer.Serialize(sarr1, 1, "test1");
            //Debug.Log("Save: " + Application.persistentDataPath + "/test.xml");
            //doc.Save(Application.persistentDataPath + "/test.xml");

            int[,,] darr1 = XmlDeserializer.Deserialize(doc.InnerXml, 1) as int[,,];

            Assert.NotNull(darr1);
            Assert.AreNotSame(sarr1, darr1);
            Assert.AreEqual(3, darr1.Rank);
            Assert.AreEqual(2, darr1.GetLength(0));
            Assert.AreEqual(3, darr1.GetLength(1));
            Assert.AreEqual(5, darr1.GetLength(2));

            //manually test individual elements
            Assert.AreEqual(2, darr1.GetValue(0, 0, 1));
            Assert.AreEqual(6, darr1.GetValue(0, 1, 0));
            Assert.AreEqual(22, darr1.GetValue(1, 1, 1));
           
        }

        /// <summary>
        /// Helper used to assign an incrementing value to each element of a multidimensional array of any size and rank.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="rank"></param>
        /// <param name="indicies"></param>
        /// <param name="val"></param>
        void IterateArrayWithValues(Array a, int rank, int[] indicies, ref int val)
        {
            for(int i = 0; i < a.GetLength(rank); i++)
            {
                indicies[rank] = i;
                if(rank+1 < a.Rank)
                {
                    IterateArrayWithValues(a, rank + 1, indicies, ref val);
                }
                else
                {
                    val += 1;
                    a.SetValue(val, indicies);
                }
            }
        }

        //[Test]
        public void TestInPlaceDeserializationOfPrimitive()
        {
            Assert.Fail();
        }
    }
}