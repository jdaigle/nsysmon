using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using RazorEngine;
using RazorEngine.Compilation;

namespace NSysmon.Collector.Api
{
    public class RazorFormatter : MediaTypeFormatter
    {
        public RazorFormatter()
        {
            // make sure we load assemblies we plan to reference in the Views
            typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly.ToString();
            typeof(System.Web.HttpUtility).Assembly.ToString();
            CompilerServiceBuilder.SetCompilerServiceFactory(new DefaultCompilerServiceFactory());
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xhtml+xml"));
        }

        public override Task WriteToStreamAsync(
                                                Type type,
                                                object value,
                                                Stream stream,
                                                HttpContent content,
                                                TransportContext transportContext)
        {
            var task = Task.Factory.StartNew(() =>
                {
                    var template = ReadViewForTypeFromAssemblyManifest(type);
                    Razor.Compile(template, type, type.Name);
                    var razor = Razor.Run(type.Name, value);

                    var buf = System.Text.Encoding.Default.GetBytes(razor);

                    stream.Write(buf, 0, buf.Length);

                    stream.Flush();
                });

            return task;
        }

        public string ReadViewForTypeFromAssemblyManifest(Type type)
        {
            var assembly = type.Assembly;
            var resourceName = string.Format("NSysmon.Collector.Api.{0}.cshtml", type.Name.Replace("ViewModel", ""));
            using (var resStream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(resStream))
            {
                return reader.ReadToEnd();
            }
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override bool CanWriteType(Type type)
        {
            return typeof(RazorViewModelBase).IsAssignableFrom(type);
        }
    }
}
