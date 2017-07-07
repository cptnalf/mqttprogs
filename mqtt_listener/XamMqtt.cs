using System;
using System.Collections.Generic;
using System.Text;

namespace mqtt_listener
{
  using Nito.AsyncEx;
  using System.Net.Mqtt;
  using Microsoft.Extensions.Logging;

  public class XamMqtt
  {
    private MqttConfig _config;
    private ILogger<XamMqtt> _logger;
    private IObserver<MqttApplicationMessage> _obs;

    public XamMqtt(MqttConfig config, ILogger<XamMqtt> log, IObserver<MqttApplicationMessage> obs) 
    { 
      _config = config; 
      _logger = log;
      _obs = obs;
    }

    public async void subscribe()
    {
      var cli = await
        MqttClient.CreateAsync(_config.server)
        .ConfigureAwait(false)
        ;
      var disp = cli.MessageStream.SubscribeSafe(_obs);
      
      var clientid = "mqtt2web";
      await cli.ConnectAsync(new MqttClientCredentials(clientid)).ConfigureAwait(false);

      await cli.SubscribeAsync("clients/#", MqttQualityOfService.AtLeastOnce).ConfigureAwait(false);

      foreach(var s in _config.subscriptions)
        { await cli.SubscribeAsync(s.channel, MqttQualityOfService.AtLeastOnce).ConfigureAwait(false); }

     /*
     cli.PublishAsync(new MqttApplicationMessage("house/serverroom/temp", Encoding.UTF8.GetBytes("89.9")), MqttQualityOfService.AtLeastOnce);
      */

     Console.WriteLine("Type 'q' to quit");
      string text = null;
      do {
        text = Console.ReadLine();
      } while(text != "q");

      await cli.DisconnectAsync();
      cli.Dispose();
      disp.Dispose();
      cli = null;
      disp = null;
    }
  }
}
