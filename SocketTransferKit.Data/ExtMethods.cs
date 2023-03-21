using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;
using Newtonsoft.Json;

namespace SocketTransferKit.Data
{
    public static class ExtMethods
    {
        /// <summary>
        /// 反序列化为命令
        /// </summary>
        /// <param name="data"></param>
        /// <param name="compressedData"></param>
        /// <returns></returns>
        public static ICommand ToCommand(this byte[] data, bool compressedData = false)
        {
            if (compressedData)
            {
                data = DecompressBytes(data);
            }
            ICommand rtn;
            try
            {
                rtn = DeserializeObject(data) as ICommand;
            }
            catch (Exception)
            {
                rtn = DeserializeStruct<StructCommand>(data);
            }
            return rtn;
        }

        public static byte[] CompressBytes(byte[] bytes,int compressLevel=1)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                var zipStream = new GZipOutputStream(outStream);
                zipStream.SetLevel(compressLevel);
                zipStream.Write(bytes, 0, bytes.Length);
                zipStream.Close();
                zipStream.Dispose();
                byte[] rtnBytes = outStream.ToArray();
                outStream.Close();
                return rtnBytes;
            }
        }

        public static byte[] DecompressBytes(byte[] bytes)
        {
            byte[] writeData = new byte[2048];
            using (MemoryStream inStream = new MemoryStream(bytes))
            {
                Stream zipStream = new GZipInputStream(inStream);
                MemoryStream outStream = new MemoryStream();
                while (true)
                {
                    int size = zipStream.Read(writeData, 0, writeData.Length);
                    if (size > 0)
                    {
                        outStream.Write(writeData, 0, size);
                    }
                    else
                    {
                        break;
                    }
                }
                zipStream.Close();
                zipStream.Dispose();
                var rtn = outStream.ToArray();
                outStream.Close();
                outStream.Dispose();
                inStream.Close();
                return rtn;
            }
        }

        //将命令转换为BASE64形式
        public static string ToBase64String(this ICommand command)
        {
            var b = SerializeObject(command);
            var rtn = Convert.ToBase64String(b);
            return rtn;
        }

        /// <summary>
        /// 高速序列化（可能不稳定）
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] SerializeStruct(object obj)
        {
            var buffer = new byte[Marshal.SizeOf(obj.GetType())];
            var gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var pBuffer = gcHandle.AddrOfPinnedObject();
            Marshal.StructureToPtr(obj, pBuffer, false);
            gcHandle.Free();
            return buffer;
        }

        /// <summary>
        /// 高速反序列化（可能不稳定）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T DeserializeStruct<T>(byte[] data)
        {
            var gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var obj = Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(T));
            gcHandle.Free();
            return (T)obj;
        }

        /// <summary>
        /// 序列化为二进制数据
        /// </summary>
        /// <param name="command"></param>
        /// <param name="compress"></param>
        /// <returns></returns>
        public static byte[] ToByte(this ICommand command, bool compress)
        {
            byte[] b = null;
            if (command is StructCommand)
            {
                b = SerializeStruct(command);
            }
            else if (command is Command)
            {
                b = SerializeObject(command);
            }

            if (compress)
            {
                b = CompressBytes(b);
            }
            return b;
        }

        /// <summary>
        /// 把对象序列化并返回相应的字节
        /// </summary>
        /// <param name="pObj">需要序列化的对象</param>
        /// <returns>byte[]</returns>
        public static byte[] SerializeObject(object pObj)
        {
            if (pObj == null)
                return null;
            var memory = new System.IO.MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(memory, pObj);
            memory.Position = 0;
            var read = new byte[memory.Length];
            memory.Read(read, 0, read.Length);
            memory.Close();
            memory.Dispose();
            return read;
        }


        /// <summary>
        /// 把字节反序列化成相应的对象
        /// </summary>
        /// <param name="pBytes">字节流</param>
        /// <returns>object</returns>
        public static object DeserializeObject(byte[] pBytes)
        {
            if (pBytes == null)
            {
                return null;
            }
            var memory = new MemoryStream(pBytes) { Position = 0 };
            var formatter = new BinaryFormatter();
            var newOjb = formatter.Deserialize(memory);
            memory.Close();
            memory.Dispose();
            return newOjb;
        }

        /// <summary>
        /// 将32位整数转换为字节形式
        /// </summary>
        /// <param name="int32"></param>
        /// <returns></returns>
        public static byte[] ConvertIntToByteArray(this int int32)
        {
            return BitConverter.GetBytes(int32);
        }

        /// <summary>
        /// 将字节形式转换为32位整数
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static int ConvertByteArrayToInt(this byte[] bytes)
        {
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// 将命令转换为结构化byte[]（前四字节标志命令类型，后四字节标志消息体长度，最后为命令数据）
        /// </summary>
        /// <param name="command"></param>
        /// <param name="compress"></param>
        /// <returns></returns>
        public static byte[] ToStructByteArray(this ICommand command, bool compress = false)
        {
            var bytes = command.ToByte(compress);
            var length2ByteArray = bytes.Length.ConvertIntToByteArray();
            var listBytes = bytes.ToList();
            listBytes.InsertRange(0, length2ByteArray);
            listBytes.InsertRange(0, command.CommandType.GetHashCode().ConvertIntToByteArray());

            return listBytes.ToArray();
        }

        /// <summary>
        /// 将对象序列化为JSON字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Object2Json(this object obj)
        {
            var rtn = JsonConvert.SerializeObject(obj);
            return rtn;
        }

        /// <summary>
        /// JSON转换为指定类型的实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public static T Json2Object<T>(this string jsonStr)
        {
            var rtn = JsonConvert.DeserializeObject<T>(jsonStr);
            return rtn;
        }

        /// <summary>
        /// JSON转换为弱类型的实体
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public static dynamic Json2Object(this string jsonStr)
        {
            var rtn = JsonConvert.DeserializeObject(jsonStr) as dynamic;
            return rtn;
        }

        /// <summary>
        /// 创建对象的深拷贝副本
        /// </summary>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CloneObject<T>(this T obj)
        {
            var binaryData = SerializeObject(obj);
            var newObj = (T)DeserializeObject(binaryData);
            return newObj;
        }


    }
}
