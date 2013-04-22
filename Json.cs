using System;
using System.Web.Script.Serialization;

namespace DailyEvents
{
  static public class Json
  {
    static public string Serialize(dynamic dictionary)
    {
      return NewSerializer().Serialize(dictionary);
    }

    static public dynamic Deserialize(string json)
    {
      return NewSerializer().Deserialize<dynamic>(json);
    }

    static private JavaScriptSerializer NewSerializer()
    {
      return new JavaScriptSerializer();
    }
  }
}
