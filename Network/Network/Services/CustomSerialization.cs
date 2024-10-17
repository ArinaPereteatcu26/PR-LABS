using System.Text;
using System.Reflection;

namespace Network.Services
{
    public class CustomSerialization
    {
        // Serialize an object or its properties
        public string Serialize<T>(T obj)
        {
            if (obj == null) return "null";

            var serialized = new StringBuilder();
            var typeName = obj.GetType().Name;
            serialized.Append($"[{typeName}]");

            //retrive public instance properties of the object using reflection

            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var name = prop.Name;
                var value = prop.GetValue(obj);

                // Handle nested objects
                //check if not null or simple type
                if (value != null && !IsSimpleType(value.GetType()))
                {
                    serialized.Append($"{name}<{Serialize(value)};");
                }
                //if simple value it returns the format
                else
                {
                    serialized.Append($"{name}<{value};");
                }
            }

            return serialized.ToString();
        }

        // Serialize a list of objects
        public string SerializeList<T>(List<T> objList)
        {
            if (objList == null || objList.Count == 0) return "[]"; //indicate empty string

            var serialized = new StringBuilder("[List]"); //serialized data is list

            foreach (var obj in objList)
            {
                serialized.Append(Serialize(obj));
                serialized.Append("|");
            }

            return serialized.ToString().TrimEnd('|');
        }

        // Deserialize an object from custom format

        public T Deserialize<T>(string serializedData) where T : new()
        {
            if (string.IsNullOrEmpty(serializedData)) return default;

            var obj = new T();
            var typeName = typeof(T).Name;

            // Skip the type declaration
            var dataStart = serializedData.IndexOf($"[{typeName}]") + typeName.Length + 2;
            var keyValuePairs = serializedData.Substring(dataStart)
                                              .Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in keyValuePairs)
            {
                var keyValue = pair.Split('<');
                if (keyValue.Length == 2)
                {
                    var propertyName = keyValue[0];
                    var propertyValue = keyValue[1];

                    //use reflection to find property and set the value
                    var propertyInfo = typeof(T).GetProperty(propertyName);
                    if (propertyInfo != null)
                    {
                        var convertedValue = Convert.ChangeType(propertyValue, propertyInfo.PropertyType);
                        propertyInfo.SetValue(obj, convertedValue);
                    }
                }
            }
            return obj;
        }

        // Deserialize a list of objects from custom format
        public List<T> DeserializeList<T>(string serializedData) where T : new()
        {
            var objList = new List<T>();
            if (string.IsNullOrEmpty(serializedData)) return objList;

            var serializedObjects = serializedData.Split('|', StringSplitOptions.RemoveEmptyEntries);

            foreach (var serializedObj in serializedObjects)
            {
                objList.Add(Deserialize<T>(serializedObj));
            }

            return objList;
        }

        // Helper method to determine if a type is simple (int, string, etc.)
        private bool IsSimpleType(Type type)
        {
            return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal);
        }
    }
}
