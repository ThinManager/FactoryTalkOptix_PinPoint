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
using FTOptix.OPCUAClient;

public class TMLogixPinPoint : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }
 [ExportMethod]
public void banana() {
        IUANode alarmVar = InformationModel.Get(InformationModel.Get(((DataGrid)Owner.Get("AlarmGrid/AlarmsDataGrid")).SelectedItem).Get<IUAVariable>("InputNode").Value);
        Log.Info(alarmVar.BrowseName);
        var path = GetParentName(alarmVar.NodeId, alarmVar.BrowseName);
        Log.Info(path);
        
        var splitPath = path.Split('/');
        //var scope = splitPath[splitPath.Length - 2];
        var scope = findScope(splitPath);
        //var tagRoot = splitPath[splitPath.Length - 2];
        var tag = splitPath[splitPath.Length - 1];
        //var tag = splitPath[splitPath.Length - 2]+"."+splitPath[splitPath.Length - 1];

        PinPoint(scope, tag);
        
    }

private string findScope(string[] splitPath){
        string scope = "";
        foreach (string item in splitPath){
            if (item.Contains(':')){
                string[] findProgram = item.Split(':');
                scope = findProgram[findProgram.Length - 1];
                break;
            }
        }
        return scope;
    }

private string GetParentName(NodeId inputObj, string outPath = "") {
        var myVar = InformationModel.Get(inputObj);
        IUANode myVarOwner = myVar.Owner ?? null;
        if (myVarOwner != null) {
            outPath = GetParentName(myVarOwner.NodeId, myVarOwner.BrowseName + "/" + outPath);
        }
        return outPath;
    }

        [ExportMethod]
public async void PinPoint(string scope, string tag)
    {
        var eventArgs = new List<EventPostRequest> { };
        eventArgs.Add(new EventPostRequest("FTOptixPinPoint", "1"));
        eventArgs.Add(new EventPostRequest("EventType", "PinPoint"));
        eventArgs.Add(new EventPostRequest("Type", "2"));
        eventArgs.Add(new EventPostRequest("Tag", $"{tag}"));
        eventArgs.Add(new EventPostRequest($"Scope", $"{scope}"));
        //eventArgs.Add(new EventPostRequest("Project", "C:\\Demo\\2022Demo\\Oven_and_BoilerDemo_L84.acd"));

        //Create a cancellation token for the PostAsyncEvent Http method
        CancellationTokenSource ct = new CancellationTokenSource();

        //post the event
        await PostAsyncEvent(eventArgs);
    }

[ExportMethod]
public async void eventButton()
    {
        var eventArgs = new List<EventPostRequest> { };
        eventArgs.Add(new EventPostRequest("FTOptixPinPoint", "1"));
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
            request_.Headers.Add("x-api-key", "kR9Sl1rp.4QwX6ew//wLQn3SKzNaW4Jc9rCi5UWFiNhk=");

            var url_ = "https://172.29.56.43:8443/api/events/post";
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
