using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Strive.Network.Messages;

namespace Strive.Network.Messaging
{
    public static class CustomFormatter
    {
        public static readonly MessageTypeMap MessageTypeMap = new MessageTypeMap();

        public static byte[] Serialize(Object obj)
        {
            var buffer = new MemoryStream();

            // unique type identifier
            Type t = obj.GetType();
            byte[] encodedInt;
            try
            {
                encodedInt = BitConverter.GetBytes((Int16)MessageTypeMap.IdFromMessageType[t]);
            }
            catch (Exception)
            {
                throw new Exception("Message " + t + " has not been added to MessageTypeMap");
            }
            // reserve space for the message length field, we will fill it later,
            // and fill out the unique type identifier
            buffer.Write(encodedInt, 0, encodedInt.Length);
            buffer.Write(encodedInt, 0, encodedInt.Length);

            // encode the object
            Encode(obj, buffer, t);

            // now fill in the length field, the first field in the message.
            encodedInt = BitConverter.GetBytes((Int16)buffer.Length);
            buffer.Position = 0;
            buffer.Write(encodedInt, 0, encodedInt.Length);
            return buffer.ToArray();
        }

        public static void Encode(Object obj, MemoryStream buffer, Type t)
        {
            // if the object is a basic type, encode and return
            if (EncodeBasicType(obj, buffer, t)) return;

            foreach (FieldInfo fi in t.GetFields())
            {
                if (fi.IsStatic)
                    continue;
                Object field = fi.GetValue(obj);
                if (field == null)
                    throw new Exception("Cannot serialize object with null fields");

                // We encode to a specific type,
                // this prevents encoding derived types,
                // which would break the message protocol.
                // derived types will be encoded as messages of the
                // type they were assigned in the message_id lookup.
                Encode(field, buffer, fi.FieldType);
            }

            foreach (PropertyInfo pi in t.GetProperties())
            {
                if (!pi.CanWrite)
                    continue;
                Object p = pi.GetValue(obj, null);
                if (p == null)
                    throw new Exception("Cannot serialize object with null properties");
                Encode(p, buffer, pi.PropertyType);
            }
        }

        /// Must pass in type t of obj even though obj is dynamic because obj might be null
        public static bool EncodeBasicType(dynamic obj, MemoryStream buffer, Type t)
        {
            if (t == typeof(string))
            {
                byte[] encodedString = Encoding.Unicode.GetBytes(obj);
                byte[] encodedInt = BitConverter.GetBytes(encodedString.Length);
                buffer.Write(encodedInt, 0, encodedInt.Length);
                buffer.Write(encodedString, 0, encodedString.Length);
            }
            else if (t.IsArray)
            {
                byte[] encodedLength = BitConverter.GetBytes(obj.Length);
                buffer.Write(encodedLength, 0, encodedLength.Length);
                // recursively encode the objects of the array
                foreach (var o in ((Array)obj))
                    Encode(o, buffer, o.GetType());
            }
            else if (t.IsEnum)
            {
                byte[] encodedInt = BitConverter.GetBytes((Int16)obj);
                buffer.Write(encodedInt, 0, encodedInt.Length);
            }
            else if (t == typeof(decimal))
            {
                byte[] encodedDecimal = GetBytes(obj);
                buffer.Write(encodedDecimal, 0, encodedDecimal.Length);
            }
            else if (t == typeof(object))
            {
                return false;
            }
            else if (t.IsPrimitive)
            {
                byte[] encoded = BitConverter.GetBytes(obj);
                buffer.Write(encoded, 0, encoded.Length);
            }
            else
            {
                return false;
            }
            return true;
        }

        public static Object Deserialize(byte[] buffer)
        {
            int offset = MessageTypeMap.MessageLengthLength;
            Type t = MessageTypeMap.MessageTypeFromId[
                (MessageTypeMap.EnumMessageId)BitConverter.ToInt16(buffer, offset)];
            offset += sizeof(Int16);

            return Decode(t, buffer, ref offset);
        }

        public static Object Decode(Type t, byte[] buffer, ref int offset)
        {
            // if its a basic type, return it
            Object obj = DecodeBasicType(t, buffer, ref offset);
            if (obj != null) return obj;

            obj = FormatterServices.GetUninitializedObject(t);

            foreach (FieldInfo fi in t.GetFields().Where(x => !x.IsStatic))
                fi.SetValue(obj, Decode(fi.FieldType, buffer, ref offset));
            foreach (PropertyInfo pi in t.GetProperties().Where(x => x.CanWrite))
                pi.SetValue(obj, Decode(pi.PropertyType, buffer, ref offset), null);

            return obj;
        }

        public static Object DecodeBasicType(Type t, byte[] buffer, ref int offset)
        {
            Object result = null;
            if (t == typeof(string))
            {
                Int32 stringLength = BitConverter.ToInt32(buffer, offset);
                offset += sizeof(Int32);
                result = Encoding.Unicode.GetString(buffer, offset, stringLength);
                offset += stringLength;
            }
            else if (t.IsArray)
            {
                Int32 length = BitConverter.ToInt32(buffer, offset);
                offset += sizeof(Int32);
                var decodedArray = new ArrayList();
                for (int j = 0; j < length; j++)
                {
                    decodedArray.Add(Decode(t.GetElementType(), buffer, ref offset));
                }
                result = decodedArray.ToArray(t.GetElementType());
            }
            else if (t.IsEnum)
            {
                result = Enum.ToObject(t, BitConverter.ToInt16(buffer, offset));
                offset += sizeof(Int16);
            }
            else if (t == typeof(Byte) || t == typeof(SByte) || t == typeof(Char))
            {
                result = BitConverter.ToChar(buffer, offset);
                offset += sizeof(Byte);
            }
            else if (t == typeof(Int16))
            {
                result = BitConverter.ToInt16(buffer, offset);
                offset += sizeof(Int16);
            }
            else if (t == typeof(UInt16))
            {
                result = BitConverter.ToUInt16(buffer, offset);
                offset += sizeof(UInt16);
            }
            else if (t == typeof(Int32))
            {
                result = BitConverter.ToInt32(buffer, offset);
                offset += sizeof(Int32);
            }
            else if (t == typeof(UInt32))
            {
                result = BitConverter.ToUInt32(buffer, offset);
                offset += sizeof(UInt32);
            }
            else if (t == typeof(Int64))
            {
                result = BitConverter.ToInt64(buffer, offset);
                offset += sizeof(Int64);
            }
            else if (t == typeof(UInt64))
            {
                result = BitConverter.ToUInt64(buffer, offset);
                offset += sizeof(UInt64);
            }
            else if (t == typeof(Single))
            {
                result = BitConverter.ToSingle(buffer, offset);
                offset += sizeof(Single);
            }
            else if (t == typeof(Double))
            {
                result = BitConverter.ToDouble(buffer, offset);
                offset += sizeof(Double);
            }
            else if (t == typeof(Decimal))
            {
                result = ToDecimal(buffer, offset);
                offset += sizeof(Decimal);
            }
            else if (t == typeof(Boolean))
            {
                result = BitConverter.ToBoolean(buffer, offset);
                offset += sizeof(Boolean);
            }

            return result;
        }

        public static decimal ToDecimal(byte[] bytes, int offset)
        {
            Contract.Requires<ArgumentException>(bytes.Length == 16 + offset);
            byte[] bys = bytes.Skip(offset).Take(16).ToArray();
            var bits = new int[4];
            bits[0] = ((bys[0] | (bys[1] << 8)) | (bys[2] << 0x10)) | (bys[3] << 0x18); //lo
            bits[1] = ((bys[4] | (bys[5] << 8)) | (bys[6] << 0x10)) | (bys[7] << 0x18); //mid
            bits[2] = ((bys[8] | (bys[9] << 8)) | (bys[10] << 0x10)) | (bys[11] << 0x18); //hi
            bits[3] = ((bys[12] | (bys[13] << 8)) | (bys[14] << 0x10)) | (bys[15] << 0x18); //flags


            // If you're concerned about endianness, you can use IPAddress.HostToNetworkOrder()
            // on each of the ints.  These lines can be removed if you're not.
            bits[0] = IPAddress.HostToNetworkOrder(bits[0]);
            bits[1] = IPAddress.HostToNetworkOrder(bits[1]);
            bits[2] = IPAddress.HostToNetworkOrder(bits[2]);
            bits[3] = IPAddress.HostToNetworkOrder(bits[3]);

            Buffer.BlockCopy(bys, 0, bits, 0, 16);
            return new Decimal(bits);
        }

        public static byte[] GetBytes(decimal d)
        {
            int[] bits = decimal.GetBits(d).Select(IPAddress.NetworkToHostOrder).ToArray();
            var bytes = new byte[16];
            Buffer.BlockCopy(bits, 0, bytes, 0, 16);
            return bytes;
        }
    }
}
