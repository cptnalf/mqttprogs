using System;
using System.Threading.Tasks;

namespace mqtt_device
{
  
  class Program
  {
    static void Main(string[] args)
    {
      Random r = new Random((int)(DateTime.Now.Ticks + (long)Environment.CurrentManagedThreadId));
      var mode = 1;

      {
        Console.WriteLine("Mode (1=listen,2=publish)");
        string m = Console.ReadLine();
        mode = int.Parse(m);
      }
      
      if (mode == 2)
        {
          var t = _publish().Result;
        }
      if (mode == 1)
        {
          _subscribe();
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

    private static async System.Threading.Tasks.Task<double> _SendMsg(Random r)
    {
      double x = r.NextDouble() * 100.0;
      var bytes = System.Text.Encoding.UTF8.GetBytes(string.Format("{{ \"temp\":{0} }}",x));
      /* "house/serverroom/temp", bytes), MqttQualityOfService.AtLeastOnce); */
      
      return x;
    }

    private static void _subscribe()
    {
      //System.Threading.Timer tmr;
      var client = new uPLibrary.Networking.M2Mqtt.MqttClient("192.168.9.21");
      client.ProtocolVersion = uPLibrary.Networking.M2Mqtt.MqttProtocolVersion.Version_3_1;
      client.Connect(Guid.NewGuid().ToString());

      client.Subscribe(new string[] {"house/serverroom/temp"}, new byte[] { 0});

      client.MqttMsgSubscribed += Client_MqttMsgSubscribed;
      client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
      client.MqttMsgPublished += Client_MqttMsgPublished;
      
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