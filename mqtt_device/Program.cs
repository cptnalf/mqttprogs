using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mqtt_device
{
  using Microsoft.Extensions.Logging;
  
  class Program
  {
    static void Main(string[] args)
    {
      ILoggerFactory logfactory = new Microsoft.Extensions.Logging.LoggerFactory();
      Random r = new Random((int)(DateTime.Now.Ticks + (long)Environment.CurrentManagedThreadId));

      IConfigurationRoot config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false)
        .Build();
      var cfg = config.Get<MqttConfig>();

      logfactory.AddConsole(true);


      switch(cfg.mode)
        {
          case(1):
          case(2):
            { break; }
          default:
          {
            Console.WriteLine("Mode (1=listen,2=publish)");
            string m = Console.ReadLine();
            cfg.mode = int.Parse(m);
            break;
          }
        };
      
      switch(cfg.mode)
        {
          case (2):
          {
            Console.WriteLine("publishing!");
            var t = _publish().Result;
            break;
          }
          case (1):
          {
            Console.WriteLine("subscribing!");
            var xm = new XamMqtt(cfg, logfactory.CreateLogger<XamMqtt>());
            xm.subscribe();
            //_subscribe(cfg);
            break;            
          }
        }

 /*
      if (mode == 1)
        {
          client.MessageStream.Subscribe(new Obs());
          var t = client.SubscribeAsync("house/serverroom/temp", System.Net.Mqtt.MqttQualityOfService.AtLeastOnce);
          t.Wait();
        }
      
      Console.WriteLine("Type 'q' to quit");
      string text = null;
      do { 
        text = Console.ReadLine(); 
        if (mode == 2 && text != "q") 
          {
            if (!client.IsConnected) 
              { 
                client.ConnectAsync(new MqttClientCredentials("blarg")).Wait();
              }
            var x = _SendMsg(r, client).Result;
            Console.WriteLine(x);
          }

      } while(text != "q");

      var t1 = client.DisconnectAsync();
      t1.Wait();
      client.Dispose();
      */
    }
    
    private static async Task<int> _publish()
    {
      Console.WriteLine("Type 'q' to quit");
      
      var client = new uPLibrary.Networking.M2Mqtt.MqttClient("grissom.klingon");
      client.Connect(Guid.NewGuid().ToString());
      Random r = new Random((int)(DateTime.Now.Ticks + (long)Environment.CurrentManagedThreadId));
      string text = null;
      do { 
        text = Console.ReadLine(); 
        if (text != "q") 
          {
            double d = r.NextDouble() * 100.00;
            var bytes = System.Text.Encoding.UTF8.GetBytes(string.Format("{{ \"temp\":{0} }}",d));

            client.Publish("house/serverroom/temp", bytes);
            
            Console.WriteLine(1);
          }

      } while(text != "q");
      
      client.Disconnect();

      return 1;
    }

    private static async Task<double> _SendMsg(Random r)
    {
      double x = r.NextDouble() * 100.0;
      var bytes = System.Text.Encoding.UTF8.GetBytes(string.Format("{{ \"temp\":{0} }}",x));
      /* "house/serverroom/temp", bytes), MqttQualityOfService.AtLeastOnce); */
      
      return x;
    }

    private static void _subscribe(MqttConfig cfg)
    {
      //System.Threading.Timer tmr;
      //"192.168.9.21"

      var client = new uPLibrary.Networking.M2Mqtt.MqttClient(cfg.server);
      client.ProtocolVersion = uPLibrary.Networking.M2Mqtt.MqttProtocolVersion.Version_3_1_1;
      client.MqttMsgSubscribed += Client_MqttMsgSubscribed;
      client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
      client.MqttMsgPublished += Client_MqttMsgPublished;

      if (!string.IsNullOrWhiteSpace(cfg.username))
        { client.Connect(Guid.NewGuid().ToString(), cfg.username, cfg.password); }
      else
        { client.Connect(Guid.NewGuid().ToString()); }

      var channels = new List<string>();
      var qoss = new List<byte>();
      {
        foreach(var s in cfg.subscriptions)
          {
            channels.Add(s.channel);
            qoss.Add(s.qos);
          }
      }
      
      /*client.Subscribe(new string[] {"house/serverroom/temp"}, new byte[] { 0});*/
      client.Subscribe(channels.ToArray(), qoss.ToArray());
      
      Console.WriteLine("Type 'q' to quit");
      string text = null;
      do {
        text = Console.ReadLine();
      } while(text != "q");

      client.Disconnect();
    }

    private static void Client_MqttMsgPublished(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishedEventArgs e)
    {
      Console.WriteLine("{0}", e.MessageId);
    }

    private static void Client_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
    {
      Console.WriteLine("{0} {1}", e.Topic, System.Text.Encoding.UTF8.GetString(e.Message));


    }

    private static void Client_MqttMsgSubscribed(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgSubscribedEventArgs e)
    {
      Console.WriteLine("{0} : {1}", sender, e.MessageId);
    }
  }
}