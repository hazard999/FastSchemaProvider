using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace FastSchemaProvider
{
    public class SchemaSerializer
    {
        public Stream Serialize(SchemaProvider provider)
        {
            var Serializer = new ServiceStack.Text.TypeSerializer<SchemaProvider>();

            StreamWriter writer = new StreamWriter(new MemoryStream());

            Serializer.SerializeToWriter(provider, writer);
            
            writer.Flush();
            writer.BaseStream.Position = 0;

            return writer.BaseStream;
        }

        public void WriteToDisk(SchemaProvider provider, string FileName)
        {
            var Serializer = new ServiceStack.Text.Jsv.JsvSerializer<SchemaProvider>();

            StreamWriter writer = new StreamWriter(new FileStream(FileName, FileMode.Create));

            Serializer.SerializeToWriter(provider, writer);
            writer.Flush();
            writer.Close();
        }

        public SchemaProvider ReadFromDisk(string FileName)
        {
            var Serializer = new ServiceStack.Text.Jsv.JsvSerializer<SchemaProvider>();

            using (StreamReader reader = new StreamReader(new FileStream(FileName, FileMode.Open)))
            {
                return Serializer.DeserializeFromString(reader.ReadToEnd());
            }            
        }
    }
}
