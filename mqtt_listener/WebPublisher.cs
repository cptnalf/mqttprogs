using System;
using System.Collections.Generic;
using System.Text;

namespace mqtt_listener
{
  using System.Net.Http;
  using Microsoft.Extensions.Logging;

  /// <summary>
  /// takes messages and publishes them to a webservice used to publish metrics to prometheus.
  /// </summary>
  public class WebPublisher
  {
    private MqttConfig _config;
    private ILogger<WebPublisher> _logger;

    public WebPublisher(MqttConfig cfg, ILogger<WebPublisher> log) { _config = cfg; _logger = log; }

    /// <summary>
    /// publish values to the webservice
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="value"></param>
    /// <param name="unit"></param>
    public async void publish(string channel, decimal value, ChannelUnit unit)
    {
      var payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { channel=channel, value=value, units=unit.ToString() });
      var client = new HttpClient();
      client.Timeout = TimeSpan.FromSeconds(10.0);

      var url = _config.publish.url;
      /* @TODO lookup channel url if it's configured */

      var t = await client.PutAsync(url, new StringContent(payload, Encoding.UTF8));
      if (!t.IsSuccessStatusCode)
        { _logger.LogError("Error putting value: {0} => {1}", _config.publish.url, payload); }
    }
  }
}
