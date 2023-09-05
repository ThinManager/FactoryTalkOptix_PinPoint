#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.Alarm;
using FTOptix.NetLogic;
using FTOptix.NativeUI;
using FTOptix.UI;
using FTOptix.WebUI;
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
using System.Linq;
using FTOptix.RAEtherNetIP;
#endregion

public class CreateEmptyVersion : BaseNetLogic
{
    [ExportMethod]
    public void PrepareEmptyVersion()
    {
        // Insert code to be executed by the method
        Log.Info("Starting to delete files...");

        // -------------------------------------------------------------------------------------
        // Deleting elements from MainPage

        // Deleting custom widgets from MainPage
        DeleteObject(Project.Current.Get("UI/Pages/MainPage/BoilerWidget1"));
        DeleteObject(Project.Current.Get("UI/Pages/MainPage/TankWidget1"));
        DeleteObject(Project.Current.Get("UI/Pages/MainPage/TankWidget2"));
        // Deleting aliases value from MainPage
        Project.Current.Get("UI/Pages/MainPage").SetAlias("MainBoilerAlias", NodeId.Empty);
        Project.Current.Get("UI/Pages/MainPage").SetAlias("MainTank1Alias", NodeId.Empty);
        Project.Current.Get("UI/Pages/MainPage").SetAlias("MainTank2Alias", NodeId.Empty);
        // Deleting kinds value from MainPage
        Project.Current.Get<Alias>("UI/Pages/MainPage/MainBoilerAlias").Kind = NodeId.Empty;
        Project.Current.Get<Alias>("UI/Pages/MainPage/MainTank1Alias").Kind = NodeId.Empty;
        Project.Current.Get<Alias>("UI/Pages/MainPage/MainTank2Alias").Kind = NodeId.Empty;

        // -------------------------------------------------------------------------------------
        // Deleting all EthernetIP Related Stuff

        // Delete all EthernetIp Tags
        DeleteChildrens(Project.Current.Get("CommDrivers/EthernetIPDriver/LogixStation/Tags"));
        // Delete all EthernetIp Types
        DeleteChildrens(Project.Current.Get("CommDrivers/EthernetIPDriver/LogixStation/Types"));


        // -------------------------------------------------------------------------------------
        // Deleting all OPC/UA Related Stuff

        // Creating backup of OPC/UA client settings
        var myBkClient = InformationModel.Make<OPCUAClient>("OPCUAClient");
        // Copy OPC/UA parameters
        var originalClient = Project.Current.Get<OPCUAClient>("OPC-UA/OPCUAClient");
        foreach (var childVariable in originalClient.Children.OfType<IUAVariable>())
            myBkClient.GetOrCreateVariable(childVariable.BrowseName).SetValueNoPermissions(childVariable.Value.Value);
        // Deleting OPC/UA client (and all its children)
        Project.Current.Get<OPCUAClient>("OPC-UA/OPCUAClient").Delete();
        // Creating new empty OPC/UA client with backed-up settings
        Project.Current.Get("OPC-UA").Add(myBkClient);
    }

    void DeleteChildrens(IUANode parentNode) {
        if (parentNode == null) {
            Log.Warning("Node: " + parentNode.BrowseName + " is null");
        } else {
            Log.Info("Deleting elements in: " + parentNode.BrowseName);
            foreach (var myChildren in parentNode.Children) {
                Log.Debug("Deleting: " + myChildren.BrowseName);
                myChildren.Delete();
            }
        }
    }

    void DeleteObject(IUANode objectNode) {
        if (objectNode == null) {
            Log.Warning("Requested node is null");
        } else {
            Log.Debug("Deleting: " + objectNode.BrowseName);
            objectNode.Delete();
        }
    }
}
