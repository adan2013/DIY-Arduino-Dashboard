using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArduinoDashboardInterpreter
{
    static class Serialization
    {
        public static bool SerializeObject(ref Settings obj, string path)
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    formatter.Serialize(stream, obj);
                    stream.Close();
                }
                return true;
            }
            catch (Exception)
            {
                MessageBox.Show("Serialization error!");
                return false;
            }
        }

        public static Settings DeserializeObject(string path)
        {
            try
            {
                Settings obj;
                IFormatter formatter = new BinaryFormatter();
                using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    obj = (Settings)formatter.Deserialize(stream);
                }
                return obj;
            }
            catch (Exception)
            {
                MessageBox.Show("Deserialization error!");
                return null;
            }
        }
    }
}
