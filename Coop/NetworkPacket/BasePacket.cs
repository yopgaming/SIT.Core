using Newtonsoft.Json;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace SIT.Core.Coop.NetworkPacket
{
    public abstract class BasePacket : ISITPacket
    {
        [JsonProperty(PropertyName = "serverId")]
        public string ServerId { get; set; } = CoopGameComponent.GetServerId();

        [JsonIgnore]
        private string _t;

        [JsonProperty(PropertyName = "t")]
        public string TimeSerializedBetter
        {
            get
            {
                if (string.IsNullOrEmpty(_t))
                    _t = DateTime.Now.Ticks.ToString("G");

                return _t;
            }
            set
            {
                _t = value;
            }
        }

        [JsonProperty(PropertyName = "m")]
        public virtual string Method { get; set; } = null;

        public BasePacket()
        {
            ServerId = CoopGameComponent.GetServerId();
        }

        public virtual string Serialize()
        {
            using BinaryWriter binaryWriter = new(new MemoryStream());
            var allProps = ReflectionHelpers.GetAllPropertiesForObject(this);
            binaryWriter.WriteNonPrefixedString("SIT"); // 3
            binaryWriter.WriteNonPrefixedString(ServerId); // 24
            binaryWriter.WriteNonPrefixedString(Method); // Unknown
            binaryWriter.WriteNonPrefixedString("?");
            foreach (var prop in allProps
                .Where(x => x.Name != "ServerId" && x.Name != "Method")
                .OrderByDescending(x => x.Name == "AccountId")
                )
            {
                binaryWriter.WriteNonPrefixedString(prop.GetValue(this).ToString());
                binaryWriter.WriteNonPrefixedString(",");
            }
            return Encoding.UTF8.GetString(((MemoryStream)binaryWriter.BaseStream).ToArray());
        }

        public virtual ISITPacket Deserialize(byte[] bytes)
        {
            return this;
        }

    }

    public interface ISITPacket
    {
        public string ServerId { get; set; }
        public string TimeSerializedBetter { get; set; }
        public string Method { get; set; }

    }

    public static class SerializerExtensions
    {
        public static void WriteNonPrefixedString(this BinaryWriter binaryWriter, string value)
        {
            binaryWriter.Write(Encoding.UTF8.GetBytes(value));
        }

        public static T DeserializePacketSIT<T>(this T obj, string serializedPacket)
        {
            var separatedPacket = serializedPacket.Split(',');
            var allProps = ReflectionHelpers.GetAllPropertiesForObject(obj);
            var index = 0;

            foreach (var prop in allProps
              .Where(x => x.Name != "ServerId" && x.Name != "Method")
              .OrderByDescending(x => x.Name == "AccountId")
              )
            {
                //PatchConstants.Logger.LogInfo(prop.Name);
                //PatchConstants.Logger.LogInfo(prop.PropertyType.Name);
                //PatchConstants.Logger.LogInfo(separatedPacket[index]);
                switch (prop.PropertyType.Name)
                {
                    case "Float":
                        prop.SetValue(obj, float.Parse(separatedPacket[index].ToString()));
                        break;
                    case "Single":
                        prop.SetValue(obj, Single.Parse(separatedPacket[index].ToString()));
                        break;
                    case "Boolean":
                        prop.SetValue(obj, Boolean.Parse(separatedPacket[index].ToString()));
                        break;
                    case "String":
                        prop.SetValue(obj, separatedPacket[index]);
                        break;
                    case "Integer":
                    case "Int":
                    case "Int32":
                        prop.SetValue(obj, int.Parse(separatedPacket[index].ToString()));
                        break;
                    default:
                        PatchConstants.Logger.LogError($"{prop.Name} of type {prop.PropertyType.Name} could not be parsed by SIT Deserializer!");
                        break;
                }
                index++;
            }
            return obj;
        }
    }
}
