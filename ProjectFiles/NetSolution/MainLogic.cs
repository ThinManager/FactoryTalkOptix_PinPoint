#region Using directives
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using UAManagedCore;
using FTOptix.RAEtherNetIP;
#endregion

public class MainLogic : BaseNetLogic {
    public override void Start() {
        var highLevel = Project.Current.GetVariable("OPC-UA/OPCUAClient/Objects/Demo/BoilerDemo/Boiler1/FillLevelSensor/FillLevel/EURange/High");
        if (highLevel != null)
            highLevel.Value = 100;
        var minLevel = Project.Current.GetVariable("OPC-UA/OPCUAClient/Objects/Demo/BoilerDemo/Boiler1/FillLevelSensor/FillLevel/EURange/Low");
        if (minLevel != null)
            minLevel.Value = 0;
        var highTemperature = Project.Current.GetVariable("OPC-UA/OPCUAClient/Objects/Demo/BoilerDemo/Boiler1/TemperatureSensor/Temperature/EURange/High");
        if (highTemperature != null)
            highTemperature.Value = 90;
        var lowTemperature = Project.Current.GetVariable("OPC-UA/OPCUAClient/Objects/Demo/BoilerDemo/Boiler1/TemperatureSensor/Temperature/EURange/Low");
        if (lowTemperature != null)
            lowTemperature.Value = -10;
    }

    public override void Stop() {
        // Insert code to be executed when the user-defined logic is stopped
    }
}
