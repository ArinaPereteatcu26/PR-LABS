using System.Text;

namespace Network.Services
{
    public class SerializationLogic
    {
        public string SerializeToJson<T>(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "Object to serialize cannot be null.");
            }

            var jsonBuilder = new StringBuilder(); // construct JSON string
            jsonBuilder.Append("{");

            var properties = obj.GetType().GetProperties(); // inspect object at runtime
            for (int i = 0; i < properties.Length; i++)
            {
                var prop = properties[i];
                var name = prop.Name;
                var value = prop.GetValue(obj);

                // Handle the case where value might be null
                jsonBuilder.Append($"\"{name}\": \"{value?.ToString() ?? "null"}\""); // Use null-coalescing operator

                if (i < properties.Length - 1)
                {
                    jsonBuilder.Append(",");
                }
            }

            jsonBuilder.Append("}");

            return jsonBuilder.ToString();
        }

        public string SerializeToXML<T>(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "Object to serialize cannot be null.");
            }

            var xmlBuilder = new StringBuilder();
            var typeName = obj.GetType().Name;

            xmlBuilder.Append($"<{typeName}>");

            var properties = obj.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var name = prop.Name;
                var value = prop.GetValue(obj);

                // Handle null value for XML serialization
                xmlBuilder.Append($"<{name}>{value?.ToString() ?? "null"}</{name}>"); // Use null-coalescing operator
            }

            xmlBuilder.Append($"</{typeName}>");

            return xmlBuilder.ToString();
        }


        //serialize object of type T into JSON array string
        public string SerializeListToJson<T>(List<T> objList)
        {
            if (objList == null)
            {
                throw new ArgumentNullException(nameof(objList), "The list to serialize cannot be null.");
            }

            var jsonBuilder = new StringBuilder();
            jsonBuilder.Append("[");

            for (int i = 0; i < objList.Count; i++)
            {
                jsonBuilder.Append(SerializeToJson(objList[i]));

                if (i < objList.Count - 1)
                {
                    jsonBuilder.Append(", ");
                }
            }
            jsonBuilder.Append("]");

            return jsonBuilder.ToString();
        }

        public string SerializeListToXML<T>(List<T> objList)
        {
            if (objList == null)
            {
                throw new ArgumentNullException(nameof(objList), "The list to serialize cannot be null.");
            }

            var xmlBuilder = new StringBuilder();
            var typeName = typeof(T).Name + "List";
            xmlBuilder.Append($"<{typeName}>");

            foreach (var obj in objList)
            {
                xmlBuilder.Append(SerializeToXML(obj));
            }

            xmlBuilder.Append($"</{typeName}>");
            return xmlBuilder.ToString();
        }
    }
}
