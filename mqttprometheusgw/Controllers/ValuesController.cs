using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace mqttprometheusgw.Controllers
{
  public class MetricMsg
  {
    public enum ChannelUnit
    {
      none
      ,deg_c
      ,deg_f
      ,psi
      ,volts
    }

    public string channel {get;set;}
    public decimal value {get;set;}
    public string units {get;set;}
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
    public IActionResult Save([FromBody] MetricMsg message)
    {
      MetricMsg.ChannelUnit unit;
      if (!Enum.TryParse(message.units, out unit)) { return this.StatusCode(400, message.units); }
      if (string.IsNullOrWhiteSpace(message.channel)) { return StatusCode(400, "No channel"); }

      var parts = message.channel.Split('/');
      var key = string.Format("{0}-{1}", parts[parts.Length -1], unit);
      
      var entry = _cache.CreateEntry(key);
      if (null == _keys.FirstOrDefault(x => string.Compare(x, key) == 0)) { _keys.Add(key); }

      entry.AbsoluteExpirationRelativeToNow = new TimeSpan(0,1,30);
      switch(unit)
        {
          case (MetricMsg.ChannelUnit.deg_c):
          case (MetricMsg.ChannelUnit.deg_f):
            {
              var itt = new IoTTemp { src=parts[parts.Length -1], };
              entry.Value = itt;

              if (unit == MetricMsg.ChannelUnit.deg_c)
                {
                  itt.value = message.value;
                  itt.valuef = (itt.value * 9.00M/5.00M) + 32.0M;
                }
              else
                {
                  itt.valuef = message.value;
                  itt.value = (itt.valuef - 32.00M)* 5.00M/9.00M;
                }

              break;
            }

          default:
            {
              return StatusCode(400, string.Format("Units not mapped: {0}",message.units));
            }
        }
      return Ok();
    }
  }
}
