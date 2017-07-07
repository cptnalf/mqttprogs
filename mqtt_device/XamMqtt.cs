using System;
using System.Collections.Generic;
using System.Text;

namespace mqtt_device
{
  using Nito.AsyncEx;
  using System.Net.Mqtt;
  using Microsoft.Extensions.Logging;

  internal class Obs : IObserver<MqttApplicationMessage>
  {
    private ILogger<XamMqtt> _logger;

    public Obs(ILogger<XamMqtt> l) { _logger = l; }

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
      string msg = Encoding.UTF8.GetString(value.Payload);
      Console.Write("{0} - {1}", value.Topic, msg);
      _logger.LogInformation("{0} - {1}", value.Topic, msg);
    }
  }

  public class XamMqtt
  {
    private MqttConfig _config;
    private ILogger<XamMqtt> _logger;

    public XamMqtt(MqttConfig config, ILogger<XamMqtt> log) { _config = config; _logger = log; }
    public void subscribe()
    {
      var cli = 
        (
        MqttClient.CreateAsync(_config.server)
        .ConfigureAwait(false)
        )
        .GetAwaiter()
        .GetResult();
      var disp = cli.MessageStream.SubscribeSafe(new Obs(_logger));
      
      var clientid = "mqtt2web";
      cli.ConnectAsync(new MqttClientCredentials(clientid)).ConfigureAwait(false).GetAwaiter().GetResult();

      cli.SubscribeAsync("clients/#", MqttQualityOfService.AtLeastOnce).ConfigureAwait(false).GetAwaiter().GetResult();

      foreach(var s in _config.subscriptions)
        {
          AsyncContext.Run(() => cli.SubscribeAsync(s.channel, MqttQualityOfService.AtLeastOnce));
        }

     /*
     cli.PublishAsync(new MqttApplicationMessage("house/serverroom/temp", Encoding.UTF8.GetBytes("89.9")), MqttQualityOfService.AtLeastOnce);
      */

     Console.WriteLine("Type 'q' to quit");
      string text = null;
      do {
        text = Console.ReadLine();
      } while(text != "q");

      cli.DisconnectAsync();
      cli.Dispose();
      disp.Dispose();
    }

    public async void publish(string clientid)
    {
      var r = new Random(Environment.CurrentManagedThreadId + (int)DateTime.Now.Ticks);
      var cli = await MqttClient.CreateAsync(_config.server).ConfigureAwait(false);

      await cli.ConnectAsync(new MqttClientCredentials(clientid)).ConfigureAwait(false);

      string topic = string.Format("clients/{0}", clientid);
      await cli.PublishAsync(new MqttApplicationMessage(topic, new byte[] { 1}), MqttQualityOfService.AtLeastOnce)
        .ConfigureAwait(false);

      Console.WriteLine("Type 'q' to quit (enter to publish)");
      string text = null;
      do {
        r.Next();

        if ((r.Next(10000) % 2) == 0) { topic = "house/serverroom/temp"; }
        else { topic = "house/garage/temp"; }

        float f = (float)(r.NextDouble() * 100.0d);
        byte[] bytes = new byte[8];
        bytes[0] = 2;
        bytes[1] = 2;
        { var b1 = BitConverter.GetBytes(f); for(int i=0; i<b1.Length; ++i) { bytes[2+i] = b1[i]; } }

        await cli.PublishAsync(new MqttApplicationMessage(topic, bytes), MqttQualityOfService.AtLeastOnce).ConfigureAwait(false);

        text = Console.ReadLine();
      } while(text != "q");

      await cli.DisconnectAsync();
      cli.Dispose();
      cli = null;
    }
  }
}
