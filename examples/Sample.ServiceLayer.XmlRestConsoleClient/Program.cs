using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Sample.ServiceLayer.XmlRestConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var grab = new UrlGrab();

            Console.WriteLine("Executing Single Request");
            Console.WriteLine();
            var result = grab.Get(String.Format("http://localhost/Sample.ServiceLayer.Host/Service.svc/xml/?request=GetAgeRequest&DateOfBirth={0}", "1983/04/21"));
            Console.WriteLine(result.ResponseText);
            Console.WriteLine();
            Console.WriteLine("Executing Multiple Requests");
            Console.WriteLine();
            var result2 = grab.Get(String.Format("http://localhost/Sample.ServiceLayer.Host/Service.svc/xml/?request[0]=GetAgeRequest&DateOfBirth[0]={0}&request[1]=ReverseStringRequest&StringToReverse[1]={1}", "1983/04/21", "Text to reverse"));
            Console.WriteLine(result2.ResponseText);
            Console.WriteLine();
            Console.WriteLine("Executing One Way Request");
            Console.WriteLine();
            Console.WriteLine("The following is executing a Set Cache Command setting the key as Key1 and the value as Value1. ");
            Console.WriteLine("The response is the result of a Get Cache Request for the key Key1");
            Console.WriteLine();
            grab.Get(String.Format("http://localhost/Sample.ServiceLayer.Host/Service.svc/xml/oneway?request=SetCacheCommand&CacheKey={0}&CacheValue={1}", "Key1", "Value1"));
            var result3 = grab.Get(String.Format("http://localhost/Sample.ServiceLayer.Host/Service.svc/xml/?request=GetCacheRequest&CacheKey={0}", "Key1"));
            Console.WriteLine(result3.ResponseText);
            Console.WriteLine();
            Console.ReadLine();
        }
    }

    public class UrlGrab
    {
        public UrlGrabResponse Get(string url)
        {

            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                return new UrlGrabResponse
                {
                    ResponseCode = response.StatusCode,
                    ResponseText = sr.ReadToEnd()
                };
            }

        }

        public UrlGrabResponse Post(string url, string data, string contentType)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = contentType;
            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(data);
            }
            var response = (HttpWebResponse)request.GetResponse();
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                return new UrlGrabResponse
                {
                    ResponseCode = response.StatusCode,
                    ResponseText = sr.ReadToEnd()
                };
            }
        }
    }

    public class UrlGrabResponse
    {
        public HttpStatusCode ResponseCode { get; set; }
        public String ResponseText { get; set; }
    }
}
