using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace KeyboardAudioVisualizer.Legacy
{
    public static class SerializationHelper
    {
        #region Methods

        public static void SaveObjectToFile<T>(T serializableObject, string path)
        {
            if (serializableObject == null) return;

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Seek(0, SeekOrigin.Begin);
                    xmlDocument.Load(stream);
                    xmlDocument.Save(path);
                }
            }
            catch {/* Catch'em all */}
        }

        public static T LoadObjectFromFile<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return default(T);

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;

                using (StringReader sr = new StringReader(xmlString))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    using (XmlReader reader = new XmlTextReader(sr))
                        return (T)serializer.Deserialize(reader);
                }
            }
            catch {/* Catch'em all */}

            return default(T);
        }

        #endregion
    }
}
