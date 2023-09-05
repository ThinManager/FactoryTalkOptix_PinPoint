#region Using directives
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using System;
using UAManagedCore;
using FTOptix.RAEtherNetIP;
#endregion

public class RecipesEditorOutputMessageLogic : BaseNetLogic {
    public override void Start() {
        messageVariable = Owner.GetVariable("Message");
        if (messageVariable == null)
            throw new ArgumentNullException("Unable to find variable Message in OutputMessage label");
    }

    public override void Stop() {
        lock (lockObject) {
            task?.Dispose();
        }
    }

    [ExportMethod]
    public void SetOutputMessage(string message) {
        lock (lockObject) {
            task?.Dispose();

            messageVariable.Value = message;
            task = new DelayedTask(() => { messageVariable.Value = ""; }, 5000, LogicObject);
            task.Start();
        }
    }

    [ExportMethod]
    public void SetOutputLocalizedMessage(LocalizedText message) {
        SetOutputMessage(InformationModel.LookupTranslation(message).Text);
    }

    private DelayedTask task;
    private IUAVariable messageVariable;
    private object lockObject = new object();
}
