using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DailyEvents
{
  [TestFixture()]
  public class JsonTest
  {
    
    [Test()]
    public void should_serialize_dictionary_to_json()
    {
      string json = Json.Serialize(new Dictionary<string, string>() {
        { "one", "1" }, { "two", "2" }, { "three", "3" }
      });
      Assert.AreEqual("{\"one\":\"1\",\"two\":\"2\",\"three\":\"3\"}", json);
    }
    
    [Test()]
    public void should_deserialize_json_to_dictionary()
    {
      string json = "{\"one\":\"1\",\"two\":\"2\",\"three\":\"3\"}";
      Dictionary<string, object> dictionary = Json.Deserialize(json);

      Assert.AreEqual("1", dictionary["one"]);
      Assert.AreEqual("2", dictionary["two"]);
      Assert.AreEqual("3", dictionary["three"]);
    }
  }
}
