/**********************************************
* Pantagruel
* Copyright 2015-2016 James Clark
**********************************************/
using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

namespace Pantagruel.Serializer
{
    /// <summary>
    /// Simple tools for dealing with xml-specific problems.
    /// </summary>
    public static class XmlTools
    {
        /// <summary>
        /// Removes insignificant whitespace between elements in an xmlk-formatted string.
        /// </summary>
        /// <param name="xml">The xml-formatted string from which to remove whitespace.</param>
        /// <returns>A new xml-formatted string with the whitespace removed.</returns>
        public static string RemoveWhitespace(string xml)
        {
            var reg = new Regex(@">\s*<");
            return reg.Replace(xml.Trim(), "><");
        }
    }
}
