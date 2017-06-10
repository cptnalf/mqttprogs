using System;
using System.Net.Mqtt;
using System.Threading.Tasks;

namespace mqtt_device
{
  public class Obs : IObserver<System.Net.Mqtt.MqttApplicationMessage>
  {
    public void OnCompleted()
    {
      Console.WriteLine("Blarg");
    }

    public void OnError(Exception error)
    {
      Console.WriteLine(error.ToString());
    }

    public void OnNext(MqttApplicationMessage value)
    {
      var text = System.Text.Encoding.UTF8.GetString(value.Payload);
      Console.WriteLine("{0} : {1}", value.Topic, text);
    }
  }

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
    
    private static async Task<IMqttClient> _publish()
    {
      var client = await System.Net.Mqtt.MqttClient.CreateAsync("127.0.0.1", new MqttConfiguration { Port=8883, KeepAliveSecs=20, WaitTimeoutSecs=10} );
      Console.WriteLine("Type 'q' to quit");
      Random r = new Random((int)(DateTime.Now.Ticks + (long)Environment.CurrentManagedThreadId));
      string text = null;
      await client.ConnectAsync(new MqttClientCredentials("blarg"));

      do { 
        text = Console.ReadLine(); 
        if (text != "q") 
          {
            if (!client.IsConnected) 
              { 
                await client.ConnectAsync(new MqttClientCredentials("blarg"));
              }
            var x = _SendMsg(r, client).Result;
            Console.WriteLine(x);
          }

      } while(text != "q");
      


      return client;
    }

    private static async System.Threading.Tasks.Task<double> _SendMsg(Random r, IMqttClient client)
    {
      double x = r.NextDouble() * 100.0;
      var bytes = System.Text.Encoding.UTF8.GetBytes(string.Format("{{ \"temp\":{0} }}",x));
      await client.PublishAsync(new MqttApplicationMessage("house/serverroom/temp", bytes), MqttQualityOfService.AtLeastOnce);
      
      return x;
    }
  }
}