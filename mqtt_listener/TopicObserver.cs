using System;
using System.Collections.Generic;
using System.Text;

namespace mqtt_listener
{
  using System.Net.Mqtt;
  using Microsoft.Extensions.Logging;
  using System.Text.RegularExpressions;

  internal class TopicObserver : IObserver<MqttApplicationMessage>
  {
    static Regex _ClientTopic = new Regex("clients[/](.+)", RegexOptions.Compiled);
    static Regex _TempTopic = new Regex("(.+)/temp", RegexOptions.Compiled);

    private ILogger<TopicObserver> _logger;

    public TopicObserver(ILogger<TopicObserver> l) { _logger = l; }

    public void OnCompleted()
    {
      Console.WriteLine("done!");
    }

    public void OnError(Exception error)
    {
      Console.WriteLine(error.ToString());
    }

    public void OnNext(MqttApplicationMessage value)
    {
      var m = _ClientTopic.Match(value.Topic);
      if (m.Success)
        {
          var cli = m.Captures[0].Value;
          bool online = false;
          if (value.Payload != null && value.Payload.Length > 0)
            { online = value.Payload[0] > 0; }
          _logger.LogInformation("presence: {0} online? {1}", cli, online );
        }
      else
        {
          m = _TempTopic.Match(value.Topic);

          if (m.Success)
            {
              var name = m.Captures[0].Value;
              if (value.Payload == null || value.Payload.Length < 1) { return; }
              ChannelValueType type = (ChannelValueType)value.Payload[0];
              ChannelUnit units = (ChannelUnit)value.Payload[1];

              string msg = Encoding.UTF8.GetString(value.Payload);
              _logger.LogInformation("{0} - {1}", value.Topic, msg);
            }
        }
    }
  }
}
