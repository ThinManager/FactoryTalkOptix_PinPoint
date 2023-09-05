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
using FTOptix.RAEtherNetIP;
#endregion

/*
 *  Script to reproduce when presenting this demo:
 *      Optional:
 *          - Open project, do a Save As, show it's full
 *          - Go into NetLogic use DT NetLogic and explain that you can interact with elements both at RT and DT
 *      Important:
 *          - Remember to do a Save As (until the Reset is implemented in WebIde)
 *  1. Desktop IDE
 *      - Open local project
 *      - Import PLC tags using wizard 
 *          Set proper PLC address
 *          Highlight both custom types and instances
 *      - Import OPC/UA tags
 *          Set proper OPC/UA Server address
 *          Highlight custom types, instances and methods
 *      - Local commit of the project
 *          Demonstrate offline history and "restore" feature
 *          Highlight the fact that this is completely offline
 *      - FTRA Login from Optix
 *      - Create GitHub repository
 *          Remember to use same project name
 *      - Push project to remote
 *          Show authentication request
 *          Explain that GitHub token is stored in FTRA account
 *  2. Web IDE
 *      - Open project from remote
 *          Show that no authentication is required as it is already stored in FTRA account
 *      - Add BoilerWidget, Tank1 and Tank2 instances
 *          Highlight that these widgets were custom built
 *          Optional: show TemplateLibrary
 *      - Link widgets aliases to MainPage aliases
 *          Explain that this is needed to create a common place for tags to communicate if coming
 *          from different communication drivers
 *      - Link MainPage aliases to PLC Tags
 *          Explain that this feature is very useful to use same HMI application on PLC from different
 *          manufacturer
 *      - Start Emulator and show that it works on Web IDE
 *      - Push project to remote
 *          Open GitHub page with commit history
 *          Open commit history on IDE and show the "cloud" icon
 *  3. Desktop IDE
 *      - Pull project from remote
 *      - Show WebPresentationEngine
 *          Show HTTPS capabilities
 *      - Run Emulator
 *          Show that sessions are totally independent from Native to Web
 *          Show responsiveness both on Web and Native with ScaleLayout
 *      - Open Conflicts page
 *          Add TextBox1
 *          Add Label1 with Test1 as Text property
 *          Move LED to the right side
 *      - Push project
 *      - Close project
 *  4. Web IDE
 *      - Open conflicts page
 *          Add SpinBox1
 *          Add Label1 with Test2 as Text property
 *          Delete LED1
 *      - Push
 *          Show conflict resolution page
 *          Accept resolution
 *      - Deploy to HT2200
 *      - Open FTRA and show desktop
 *  5. Desktop IDE
 *      - Connected IDE
 */

public class DemonstrationScript : BaseNetLogic
{
    [ExportMethod]
    public void Demo()
    {
        // Insert code to be executed by the method
        Log.Info("This is not a NetLogic, it only contains the speech!!");
        Log.Info("Open NetLogic to see the script");
    }
}
