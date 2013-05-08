using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace DailyEvents
{
  public class HttpClient
  {
    private readonly string entryPoint;
    private readonly Dictionary<string, string> customHeaders;

    public HttpClient(string entryPoint) : this(entryPoint, new Dictionary<string, string>()) {}

    public HttpClient(string entryPoint, Dictionary<string, string> customHeaders)
    {
      this.entryPoint    = entryPoint;
      this.customHeaders = customHeaders;
    }
    
    public string Get(string path)
    {
      return Get(path, new Dictionary<string, string>());
    }
    
    public string Get(string path, Dictionary<string, string> parameters)
    {
      if (parameters.Count > 0)
        path += "?" + ToStringParameters(parameters);
      
      HttpWebRequest request = WebRequest.Create(entryPoint + path) as HttpWebRequest;
      string response = null;

      Debug.WriteLine(request.RequestUri);

      using (HttpWebResponse webResponse = request.GetResponse() as HttpWebResponse)
      {
        StreamReader reader = new StreamReader(webResponse.GetResponseStream());
        response = reader.ReadToEnd();
      }
      Debug.WriteLine(response);

      return response;
    }
    
    public string Post(string path)
    {
      return Post(path, new Dictionary<string, string>());
    }
    
    public string Post(string path, Dictionary<string, string> parameters)
    {
      HttpWebRequest request = WebRequest.Create(new Uri(entryPoint + path)) as HttpWebRequest;
      request.ContentType = "application/json";
      request.Method = "POST";

      byte[] formData = UTF8Encoding.UTF8.GetBytes(Json.Serialize(parameters));
      request.ContentLength = formData.Length;
      
      foreach (var header in customHeaders)
      {
        request.Headers[header.Key] = header.Value;
      }
      using (Stream post = request.GetRequestStream())
      {  
        post.Write(formData, 0, formData.Length);  
      }
      string response = null;

      Debug.WriteLine(request.RequestUri);

      using (HttpWebResponse webResponse = request.GetResponse() as HttpWebResponse)
      {  
        StreamReader reader = new StreamReader(webResponse.GetResponseStream());
        response = reader.ReadToEnd();
      }
      Debug.WriteLine(response);

      return response;
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
