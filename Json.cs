using System;
using System.Web.Script.Serialization;

namespace DailyEvents
{
  static public class Json
  {
    static public string Serialize(dynamic dictionary)
    {
      return Serializer().Serialize(dictionary);
    }

    static public dynamic Deserialize(string json)
    {
      return Serializer().Deserialize<dynamic>(json);
    }
    
    static public dynamic Deserialize<type>(string json)
    {
      return Serializer().Deserialize<type>(json);
    }

    static public JavaScriptSerializer Serializer()
    {
      return new JavaScriptSerializer();
    }
  }
}
