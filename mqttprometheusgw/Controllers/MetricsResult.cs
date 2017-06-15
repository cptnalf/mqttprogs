using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace mqttprometheusgw.Controllers
{
  using Stream = System.IO.Stream;
  using StreamWriter = System.IO.StreamWriter;

  /// <summary>
  /// https://blog.stephencleary.com/2016/11/streaming-zip-on-aspnet-core.html
  /// https://github.com/StephenClearyExamples/AsyncDynamicZip/blob/core-ziparchive/Example/src/WebApplication/Controllers/FileController.cs
  /// </summary>
  public class MetricsResult : FileResult
  {
    private IReadOnlyList<IoTTemp> _temps;

    public MetricsResult(MediaTypeHeaderValue contentType, IReadOnlyList<IoTTemp> temps)
        : base(contentType?.ToString())
    {
      if (temps == null) { throw new ArgumentNullException(nameof(temps)); }
      _temps = temps;
    }

    public override Task ExecuteResultAsync(ActionContext context)
    {
      if (context == null) { throw new ArgumentNullException(nameof(context)); }
      
      var executor = new FileCallbackResultExecutor(context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>());
      return executor.ExecuteAsync(context, this);
    }

    private sealed class FileCallbackResultExecutor : FileResultExecutorBase
    {
      public FileCallbackResultExecutor(ILoggerFactory loggerFactory)
          : base(CreateLogger<FileCallbackResultExecutor>(loggerFactory))
      { }

      public async Task ExecuteAsync(ActionContext context, MetricsResult result)
      {
        SetHeadersAndLog(context, result);
        StreamWriter strm = new StreamWriter(context.HttpContext.Response.Body, System.Text.Encoding.UTF8);

        /*
         * storage: need to link many channels (devices) to one metric with many instances.
         *   mqtt channel => one device
         *   eg:
         *    house/serverroom/temp
         *    house/garage/temp
         *    
         *    turned into:
         *    esp8266_temp{instance="serverroom"}
         *    esp8266_temp{instance="garage"}
         * 
         *   
         * formatted like:
# HELP collectd_bind_dns_notify Collectd exporter: 'bind' Type: 'dns_notify' Dstype: 'api.Derive' Dsname: 'value'
# TYPE collectd_bind_dns_notify counter
collectd_bind_dns_notify{bind="...",instance="",type="rejected"} 99
         */
        
        foreach(var x in result._temps)
          {
            await strm.WriteLineAsync(string.Format("esp8266_temp{{instance=\"{0}\",units=\"C\"}} {1}", x.src, x.value));
            await strm.WriteLineAsync(string.Format("esp8266_temp{{instance=\"{0}\",units=\"F\"}} {1}", x.src, x.valuef));
          }
      }
    }
  }
}
