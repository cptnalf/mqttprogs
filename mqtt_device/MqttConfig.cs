using System;
using System.Collections.Generic;
using System.Text;

namespace mqtt_device
{
  public class ChannelSub
  {
    public string channel {get;set;}
    public byte qos {get;set;}
  }
  
  public class MqttConfig
  {
    public int mode {get;set;}
    public string server {get;set;}
    public string username {get;set;}
    public string password {get;set;}
    public string clientID {get;set;}

    public List<ChannelSub> subscriptions {get;set;}

    public MqttConfig() { subscriptions = new List<ChannelSub>(); }
  }
}
