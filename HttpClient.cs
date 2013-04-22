using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace DailyEvents
{
  public class HttpClient
  {
    private readonly string entryPoint;

    public HttpClient(string entryPoint)
    {
      this.entryPoint = entryPoint;
    }
    
    public dynamic Get(string path)
    {
      return Get(path, new Dictionary<string, string>());
    }
    
    public dynamic Get(string path, Dictionary<string, string> parameters)
    {
      if (parameters.Count > 0)
        path += "?" + ToStringParameters(parameters);
      
      HttpWebRequest request = WebRequest.Create(entryPoint + path) as HttpWebRequest;
      string response = null;
      
      using (HttpWebResponse webResponse = request.GetResponse() as HttpWebResponse)
      {
        StreamReader reader = new StreamReader(webResponse.GetResponseStream());
        response = reader.ReadToEnd();
      }
      return Json.Deserialize(response);
    }
    
    public dynamic Post(string path)
    {
      return Post(path, new Dictionary<string, string>());
    }
    
    public dynamic Post(string path, Dictionary<string, string> parameters)
    {
      HttpWebRequest request = WebRequest.Create(new Uri(entryPoint + path)) as HttpWebRequest;
      request.ContentType = "application/x-www-form-urlencoded";
      request.Method = "POST";
      
      byte[] formData = UTF8Encoding.UTF8.GetBytes(ToStringParameters(parameters));
      request.ContentLength = formData.Length;
      
      using (Stream post = request.GetRequestStream())
      {  
        post.Write(formData, 0, formData.Length);  
      }
      string response = null;
      
      using (HttpWebResponse webResponse = request.GetResponse() as HttpWebResponse)
      {  
        StreamReader reader = new StreamReader(webResponse.GetResponseStream());
        response = reader.ReadToEnd();
      }
      return Json.Deserialize(response);
    }
    
    private string ToStringParameters(Dictionary<string, string> parameters)
    {
      StringBuilder sb = new StringBuilder();
      foreach (var param in parameters)
      {
        sb.Append(param.Key);
        sb.Append("=");
        sb.Append(HttpUtility.UrlEncode(param.Value));
        sb.Append("&");
      }
      return sb.ToString();
    }
  }
}
