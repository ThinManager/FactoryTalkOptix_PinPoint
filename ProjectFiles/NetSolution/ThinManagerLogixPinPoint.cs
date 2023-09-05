#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.NativeUI;
using FTOptix.UI;
using FTOptix.WebUI;
using FTOptix.Alarm;
using FTOptix.Recipe;
using FTOptix.DataLogger;
using FTOptix.EventLogger;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.OPCUAClient;
using FTOptix.OPCUAServer;
using FTOptix.RAEtherNetIP;
using FTOptix.CoreBase;
using FTOptix.CommunicationDriver;
using FTOptix.Core;
#endregion

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using System.Threading;

public class ThinManagerLogixPinPoint : BaseNetLogic
{
    //new Http client object to use for the API
    private static HttpClient webClient = new HttpClient();

    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        var tag = Project.Current.GetVariable("Alarms/T1001.Tank1.Level_L/InputValue");
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void PinPoint()
    {
        // Project.Current.GetVariable("BoilerDemo/Alarms/T1001.Tank1.Level_L/InputValue");
        // Project.Current.GetVariable("BoilerDemo/Alarms/T1001.Tank1.Level_L/InputValue");
    }

    [ExportMethod]
    public async void eventButton()
    {
        var eventArgs = new List<EventPostRequest> { };
        eventArgs.Add(new EventPostRequest("TerminalPXE", "1"));
        eventArgs.Add(new EventPostRequest("EventType", "PinPoint"));
        eventArgs.Add(new EventPostRequest("Type", "3"));
        eventArgs.Add(new EventPostRequest("Tag", "Mixer_DoorAlarm"));
        eventArgs.Add(new EventPostRequest("Scope", "Mixer"));
        //eventArgs.Add(new EventPostRequest("Project", "C:\\Demo\\2022Demo\\Oven_and_BoilerDemo_L84.acd"));

        //Create a cancellation token for the PostAsyncEvent Http method
        CancellationTokenSource ct = new CancellationTokenSource();

        //post the event
        await PostAsyncEvent(eventArgs);
    }

    [ExportMethod]
    public void testMethod(){
        var tag = Project.Current.GetVariable("Alarms/T1001.Tank1.Level_L/InputValue");
    }


    //Post a ThinManager Event using REST API
    public virtual async System.Threading.Tasks.Task PostAsyncEvent(System.Collections.Generic.IEnumerable<EventPostRequest> body)
    {

        /*Create a handler for the http client that will ignore certificate issues, unsecure! If you don't want to trust all
        certificates, be sure to follow the proper steps to add the certificate to your
        certificate store on your machine */
        var handler = new HttpClientHandler();
        handler.ClientCertificateOptions = ClientCertificateOption.Manual;
        handler.ServerCertificateCustomValidationCallback =
            (httpRequestMessage, cert, cetChain, policyErrors) =>
        {
            return true;
        };

        var client_ = new HttpClient(handler);

        using (var request_ = new System.Net.Http.HttpRequestMessage())
        {
            var json_ = System.Text.Json.JsonSerializer.Serialize(body);
            var content_ = new System.Net.Http.StringContent(json_);
            content_.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
            request_.Content = content_;
            request_.Method = new System.Net.Http.HttpMethod("POST");
            request_.Headers.Add("x-api-key", "i63tkg/f.LlIq8uxTuGhD4FalC2Z/cD7y2i7JbyUCn/E=");

            var url_ = "https://10.6.10.51:8443/api/events/post";
            request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

            try
            {
                var response_ = await client_.SendAsync(request_);
            }
            catch (System.Exception)
            {
                string error = "Failed to send the event to the ThinManager server";
                //throw;
            }
        }
    }
}


public class EventPostRequest
{
    private string _name;
    public string Name => _name;

    private string _value;
    public string Value => _value;

    public EventPostRequest(string eventName, string eventValue)
    {
        _name = eventName;
        _value = eventValue;
    }
}
