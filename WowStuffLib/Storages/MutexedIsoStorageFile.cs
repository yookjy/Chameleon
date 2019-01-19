using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ChameleonLib.Storages
{
    public static class MutexedIsoStorageFile
    {
        public static T Read<T>(string fileName, string mutexName) where T : new()
        {
            var model = new T();
            using (var mutexFile = new Mutex(false, mutexName))
            {
                mutexFile.WaitOne();

                try
                {
                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    using (var stream = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read, store))
                    using (var reader = new StreamReader(stream))
                        if (!reader.EndOfStream)
                        {
                            var serializer = new XmlSerializer(typeof(T));
                            model = (T)serializer.Deserialize(reader);
                            //model = JsonSerializer.DeserializeFromReader<T>(reader);
                        }
                }
                finally
                {
                    mutexFile.ReleaseMutex();
                }
            }
            return model;
        }

        public static void Write<T>(T data, string fileName, string mutexName)
        {
            using (var mutexFile = new Mutex(false, mutexName))
            {
                mutexFile.WaitOne();

                try
                {
                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    using (var stream = new IsolatedStorageFileStream(fileName, FileMode.Create, FileAccess.Write, store))
                    {
                        var serializer = new XmlSerializer(typeof(T));
                        serializer.Serialize(stream, data);
                        //JsonSerializer.SerializeToStream<T>(data, stream);
                    }
                }
                finally
                {
                    mutexFile.ReleaseMutex();
                }
            }
        }
    }
}
