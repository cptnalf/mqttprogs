using System.Collections.Generic;

namespace mqtt_listener
{
  public enum ChannelValueType
  {
    none
    ,t_int
    ,t_float
  }
  public enum ChannelUnit
  {
    none
    ,deg_c
    ,deg_f
    ,psi
    ,volts
  }

  public class ChannelPub
  {
    public string name {get;set;}
    public ChannelValueType type {get;set;}
    public ChannelUnit units {get;set;}
  }

  public class PublishConfig
  {
    public string url {get;set;}

    public List<ChannelPub> channels {get;set;}
  }

  public class ChannelSub
  {
    public string channel {get;set;}
    public byte qos {get;set;}
  }
  
  public class MqttConfig
  {
    public string server {get;set;}
    public string username {get;set;}
    public string password {get;set;}
    public string clientID {get;set;}

    public PublishConfig publish {get;set; }
    
    public List<ChannelSub> subscriptions {get;set;}

    public MqttConfig() { subscriptions = new List<ChannelSub>(); }
  }
}
