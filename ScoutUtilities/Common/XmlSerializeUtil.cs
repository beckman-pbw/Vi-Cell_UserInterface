// ***********************************************************************
// <copyright file="XmlSerializeUtil.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>

using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace ScoutUtilities.Common
{
    public static class XmlSerializeUtil
    {
        public static bool SerializeToXml<T>(string path, List<T> collection)
        {
            var serializer = new XmlSerializer(typeof(List<T>));
            using (var fs = new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(fs, collection);
                return true;
            }
        }

        public static List<T> DeSerializeFromXml<T>(string path)
        {
            var collections = new List<T>();
            var serializer = new XmlSerializer(typeof(List<T>));
            using (var stream = File.OpenRead(path))
            {
                var other = (List<T>)serializer.Deserialize(stream);
                collections.Clear();
                collections.AddRange(other);
            }
            return collections;
        }
    }
}
