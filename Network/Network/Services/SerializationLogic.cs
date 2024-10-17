using System.Text;

namespace Network.Services
{
    public class SerializationLogic
    {
        public string SerializeToJson<T>(T obj)
        {
            var jsonBuilder = new StringBuilder(); //construct JSON string
            jsonBuilder.Append("{");

            var propreties = obj.GetType().GetProperties();//inspect object at runtime
            for (int i = 0; i < propreties.Length; i++)
            {
                var prop = propreties[i];
                var name = prop.Name;
                var value = prop.GetValue(obj);

                jsonBuilder.Append($"\"{name}\": \"{value}\"");//key-value pair

                if (i < propreties.Length - 1)
                {
                    jsonBuilder.Append(",");
                }
            }

            jsonBuilder.Append("}");

            return jsonBuilder.ToString();
        }

        public string SerializeToXML<T>(T obj)
        {
            var xmlBuilder = new StringBuilder();
            var typeName = obj.GetType().Name;

            xmlBuilder.Append($"<{typeName}>");

            var propreties = obj.GetType().GetProperties();
            foreach (var prop in propreties)
            {
                var name = prop.Name;
                var value = prop.GetValue(obj);

                xmlBuilder.Append($"<{name}>{value}</{name}>");
            }
            xmlBuilder.Append($"</{typeName}>");

            return xmlBuilder.ToString();
        }

        //serialize object of type T into JSON array string
        public string SerializeListToJson<T>(List<T> objList)
        {
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
    

