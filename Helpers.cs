using MovieCatalog.DataTypes;
using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace MovieCatalog
{
    class Helpers
    {
        public static void TryCatch(Action CodeBlockToWrap, Action<Exception> OnException=null)
        {
            try
            {
                CodeBlockToWrap();
            }
            catch(Exception exp)
            {
                if(OnException.IsNotNull())
                    OnException(exp);
            }
        }
    }

    public static class Extentions
    {
        public static bool IsNull(this object obj)
        {
            return obj == null;
        }

        public static bool IsNotNull(this object obj)
        {
            return obj != null;
        }

        public static bool IsNotNullOrWhiteSpace(this string data)
        {
            return !string.IsNullOrWhiteSpace(data);
        }

        public static string UseFormat(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        public static bool IsEqualIgnoreCase(this string data, string arg)
        {
            return data.Equals(arg, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsEqual(this string data, string arg)
        {
            return data.Equals(arg);
        }

        public static bool IsEqual(this Type data, object arg)
        {
            return data.Equals(arg);
        }

        public static string SerializeObjectToXML<T>(this T item)
        {
            var xs = new XmlSerializer(typeof(T));
            using (var stringWriter = new StringWriter())
            {
                xs.Serialize(stringWriter, item);
                return stringWriter.ToString();
            }
        }

        public static T DeserializeFromXml<T>(this string xml)
        {
            T result;
            XmlSerializer ser = new XmlSerializer(typeof(T));
            using (TextReader tr = new StringReader(xml))
            {
                result = (T)ser.Deserialize(tr);
            }
            return result;
        }
    }

    public class SettingsHelper
    {
        const string SETTINGS_FILE_NAME = "settings.xml";

        public static void SaveSettings(Settings settings)
        {
            var filePath = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, SETTINGS_FILE_NAME);
            File.WriteAllText(filePath, settings.SerializeObjectToXML());
        }

        public static Settings LoadSettings()
        {
            var filePath = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, SETTINGS_FILE_NAME);
            var contents = File.ReadAllText(filePath);

            return contents.DeserializeFromXml<Settings>();
        }
    }
}
