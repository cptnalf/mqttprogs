using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace mqttprometheusgw.Controllers
{
  public class MqttMessage
  {
    public string channel {get;set;}
    public string msg {get;set;}
  }

  public class IoTTemp
  {
    public string src {get;set;}
    public decimal value {get;set;}
    public decimal valuef {get;set;}
  }

  [Route("")]
  public class ValuesController : Controller
  {
    private IMemoryCache _cache;
    private ConcurrentBag<string> _keys;
    
    public ValuesController(IMemoryCache cache, Microsoft.Extensions.Options.IOptionsSnapshot<MqttSettings> settings)
    { _cache = cache; _keys = new ConcurrentBag<string>(); }

    // GET api/values
    [HttpGet("metrics")]
    public IActionResult Get()
    {
      string result = null;

      /*
       * expiring - values published to me need to expire at some point in time.
       *     we want mutliple requests to give new results
       *     , and we want values published to go away when the publishing sources go away
       *     (maybe link to presence?)
       */
      var temps = new List<IoTTemp>();
      IoTTemp temp = null;
      foreach(var x in _keys)
        {
          if (_cache.TryGetValue<IoTTemp>(x, out temp))
            { if (temp != null) { temps.Add(temp); } }
        }

      return new MetricsResult(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("text/plain"), temps);
    }

    [HttpPut("save")]
    public IActionResult Save([FromBody] MqttMsg message)
    {
      
      var entry = _cache.CreateEntry(key);
      if (null == _keys.FirstOrDefault(x => string.Compare(x, key) == 0)) { _keys.Add(key); }

      entry.AbsoluteExpirationRelativeToNow = new TimeSpan(0,1,30);
      var itt = new IoTTemp { instance="inst", value=tempc};
      entry.Value = itt;
      itt.valuef = (itt.value + 32.0M) * 9.00M/5.00M;
      return Ok();
    }
  }
}
