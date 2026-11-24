using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ValorantResFix
{
    public class Shelper
    {
        // Custom binder to allow deserializing ScalabilityGroups across different assembly versions
        private sealed class VersionTolerantBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                if (typeName != null && typeName.EndsWith("ScalabilityGroups"))
                {
                    return typeof(ScalabilityGroups);
                }
                return Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
            }
        }

        public static int SerializeObj(object obj, string fpath)
        {
            try
            {
                using (FileStream fs = new FileStream(fpath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fs, obj);
                    return 1;
                }
            }
            catch
            {
                return 0;
            }
        }

        public static object DeserializeObj(string fpath)
        {
            if (!File.Exists(fpath)) return null;

            try
            {
                using (FileStream fs = new FileStream(fpath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Binder = new VersionTolerantBinder();
                    return bf.Deserialize(fs);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
