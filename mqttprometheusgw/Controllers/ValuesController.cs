using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace mqttprometheusgw.Controllers
{
  [Route("")]
  public class ValuesController : Controller
  {
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
       * storage: need to link many channels (devices) to one metric with many instances.
       *   mqtt channel => one device
       *   eg:
       *    house/serverroom/temp
       *    house/garage/temp
       *    
       *    turned into:
       *    esp8266_temp{serverroom}
       *    esp8266_temp{garage}
       * 
       *   
       * 
       */

      return Ok(result);
    }
  }
}
