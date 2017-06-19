using System;

namespace mqtt_listener
{
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.DependencyInjection;

  class Program
  {
    static void Main(string[] args)
    {
      IConfigurationRoot config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false)
        .Build();
      var cfg = config.Get<MqttConfig>();
      
      var sc = new ServiceCollection();
      sc
        .AddLogging()
        .AddTransient<IObserver<System.Net.Mqtt.MqttApplicationMessage>,TopicObserver>()
        .AddTransient<WebPublisher>
        .AddTransient<XamMqtt>()
        .AddSingleton<MqttConfig>(cfg)
        ;

      var sp = sc.BuildServiceProvider();
      ILoggerFactory logfactory = sp.GetService<ILoggerFactory>();
            
      logfactory
        .AddConsole();
      
      Console.WriteLine("subscribing!");

      var xm = sp.GetService<XamMqtt>();
      xm.subscribe();
    }
  }
}