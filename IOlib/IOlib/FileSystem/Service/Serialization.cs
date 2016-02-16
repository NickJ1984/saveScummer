using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using IOlib.FileSystem.Service.Extensions;

namespace IOlib.FileSystem.Service
{
    public class serializator
    {
        public static TOut deserialize<TOut>(byte[] data)
        {
            if(data.isNullOrEmpty()) throw new ArgumentException(nameof(data));

            object result;
            BinaryFormatter bFormatter = new BinaryFormatter();

            using (var ms = new MemoryStream(data))
            {
                result = bFormatter.Deserialize(ms);
            }

            return (TOut)result;
        }

        public static byte[] serialize<TIn>(TIn serializableObject)
        {
            if(serializableObject == null) throw new ArgumentNullException(nameof(serializableObject));

            byte[] result = new byte[0];
            BinaryFormatter bFormatter = new BinaryFormatter();

            using (var ms = new MemoryStream())
            {
                bFormatter.Serialize(ms, serializableObject);
                result = ms.ToArray();
            }

            return result;
        }
    }
}
