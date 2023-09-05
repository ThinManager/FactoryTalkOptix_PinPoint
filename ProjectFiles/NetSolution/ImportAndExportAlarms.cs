#region Using directives
using FTOptix.Alarm;
using FTOptix.Core;
using FTOptix.CoreBase;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UAManagedCore;
using FTOptix.RAEtherNetIP;
using OpcUa = UAManagedCore.OpcUa;
#endregion

public class ImportAndExportAlarms : BaseNetLogic {
    [ExportMethod]
    public void ImportAlarms() {
        var csvPath = GetCSVFilePath();
        if (string.IsNullOrEmpty(csvPath)) {
            Log.Error("ImportExportAlarms", "No CSV file chosen, please fill the CSVPath variable");
            return;
        }

        char? fieldDelimiter = GetFieldDelimiter();
        if (fieldDelimiter == null || fieldDelimiter == '\0')
            return;

        bool wrapFields = GetWrapFields();

        if (!File.Exists(csvPath)) {
            Log.Error("ImportExportAlarms", "The file " + csvPath + " does not exist");
            return;
        }

        try {
            using (var csvReader = new CSVFileReader(csvPath) { FieldDelimiter = fieldDelimiter.Value, WrapFields = wrapFields }) {
                if (csvReader.EndOfFile()) {
                    Log.Error("ImportExportAlarms", "The file " + csvPath + " is empty");
                    return;
                }

                while (!csvReader.EndOfFile()) {
                    var alarmFields = csvReader.ReadLine();

                    if (alarmFields.Count < minimumFieldsNumber) {
                        Log.Warning("ImportExportAlarms", "Wrong alarm CSV line, some parameters are missing");
                        continue;
                    }

                    try {
                        var alarm = ImportAlarmConfiguration(alarmFields);
                        AddAlarmToModel(Owner, alarm);
                    } catch (Exception ex) {
                        Log.Info("ImportExportAlarms", "Unable to initialize alarm " + alarmFields[alarmBrowseNameField] + ": " + ex.Message);
                    }

                }
            }

            Log.Info("ImportExportAlarms", "Alarms successfully imported");
        } catch (Exception ex) {
            Log.Error("ImportExportAlarms", "Unable to import alarms: " + ex.ToString());
        }
    }

    [ExportMethod]
    public void ExportAlarms() {
        var csvPath = GetCSVFilePath();
        if (string.IsNullOrEmpty(csvPath)) {
            Log.Error("ImportExportAlarms", "No CSV file chosen, please fill the CSVPath variable");
            return;
        }

        char? fieldDelimiter = GetFieldDelimiter();
        if (fieldDelimiter == null || fieldDelimiter == '\0')
            return;

        bool wrapFields = GetWrapFields();

        try {
            using (var csvWriter = new CSVFileWriter(csvPath) { FieldDelimiter = fieldDelimiter.Value, WrapFields = wrapFields }) {
                foreach (var alarm in GetAlarmList()) {
                    var alarmFields = CollectAlarmConfiguration(alarm);
                    csvWriter.WriteLine(alarmFields.ToArray());
                }
            }

            Log.Info("ImportExportAlarms", "Alarms successfully exported to " + csvPath);
        } catch (Exception ex) {
            Log.Error("ImportExportAlarms", "Unable to export alarms: " + ex);
        }
    }

    private void AddAlarmToModel(IUANode parent, AlarmController alarm) {
        if (parent.Get<AlarmController>(alarm.BrowseName) != null) {
            Log.Warning("ImportExportAlarms", "Alarm " + alarm.BrowseName + " already exists");
            return;
        }

        parent.Add(alarm);
    }

    private AlarmController ImportAlarmConfiguration(List<string> alarmFields) {
        var browseName = alarmFields[alarmBrowseNameField];
        var alarmType = alarmFields[alarmTypeField];

        if (!alarmFactoryMap.ContainsKey(alarmType))
            throw new Exception("unknown alarm type " + alarmType);

        var alarmFactory = alarmFactoryMap[alarmType];
        var alarm = alarmFactory(browseName);

        InitializeAlarm(alarm, alarmFields);

        return alarm;
    }

    private void InitializeAlarm(AlarmController alarm, List<string> alarmFields) {
        if (alarm is DigitalAlarm)
            InitializeDigitalAlarmConfiguration((DigitalAlarm)alarm, alarmFields);
        else if (alarm is ExclusiveLevelAlarmController)
            InitializeExclusiveLevelAlarmConfiguration((ExclusiveLevelAlarmController)alarm, alarmFields);
        else if (alarm is NonExclusiveLevelAlarmController)
            InitializeNonExclusiveLevelAlarmConfiguration((NonExclusiveLevelAlarmController)alarm, alarmFields);
        else if (alarm is ExclusiveDeviationAlarmController)
            InitializeExclusiveDeviationAlarmConfiguration((ExclusiveDeviationAlarmController)alarm, alarmFields);
        else if (alarm is NonExclusiveDeviationAlarmController)
            InitializeNonExclusiveDeviationAlarmConfiguration((NonExclusiveDeviationAlarmController)alarm, alarmFields);
        else if (alarm is ExclusiveRateOfChangeAlarmController)
            InitializeExclusiveRateOfChangeAlarmConfiguration((ExclusiveRateOfChangeAlarmController)alarm, alarmFields);
        else if (alarm is NonExclusiveRateOfChangeAlarmController)
            InitializeNonExclusiveRateOfChangeAlarmConfiguration((NonExclusiveRateOfChangeAlarmController)alarm, alarmFields);
    }

    private void InitializeDigitalAlarmConfiguration(DigitalAlarm alarm, List<string> alarmFields) {
        if (alarmFields.Count < minimumDigitalAlarmFieldsNumber)
            throw new Exception("Some parameters are missing");

        InitializeBaseAlarmConfiguration(alarm, alarmFields);
        TrySetDoubleProperty(alarm, "NormalStateValue", alarmFields[normalValueField]);
    }

    private void InitializeExclusiveLevelAlarmConfiguration(ExclusiveLevelAlarmController alarm, List<string> alarmFields) {
        if (alarmFields.Count() < minimumLimitAlarmFieldsNumber)
            throw new Exception("Some parameters are missing");

        InitializeBaseAlarmConfiguration(alarm, alarmFields);
        InitializeBaseLevelAlarmConfiguration(alarm, alarmFields);
    }

    private void InitializeNonExclusiveLevelAlarmConfiguration(NonExclusiveLevelAlarmController alarm, List<string> alarmFields) {
        if (alarmFields.Count() < minimumLimitAlarmFieldsNumber)
            throw new Exception("Some parameters are missing");

        InitializeBaseAlarmConfiguration(alarm, alarmFields);
        InitializeBaseLevelAlarmConfiguration(alarm, alarmFields);
    }

    private void InitializeExclusiveDeviationAlarmConfiguration(ExclusiveDeviationAlarmController alarm, List<string> alarmFields) {
        if (alarmFields.Count < minimumDeviationAndRateOfChangeAlarmFieldsNumber)
            throw new Exception("Some parameters are missing");

        InitializeBaseAlarmConfiguration(alarm, alarmFields);
        InitializeBaseLevelAlarmConfiguration(alarm, alarmFields);
        TrySetDoubleProperty(alarm, "Setpoint", alarmFields[setPointField]);
    }

    private void InitializeNonExclusiveDeviationAlarmConfiguration(NonExclusiveDeviationAlarmController alarm, List<string> alarmFields) {
        if (alarmFields.Count < minimumDeviationAndRateOfChangeAlarmFieldsNumber)
            throw new Exception("Some parameters are missing");

        InitializeBaseAlarmConfiguration(alarm, alarmFields);
        InitializeBaseLevelAlarmConfiguration(alarm, alarmFields);
        TrySetDoubleProperty(alarm, "Setpoint", alarmFields[setPointField]);
    }

    private void InitializeExclusiveRateOfChangeAlarmConfiguration(ExclusiveRateOfChangeAlarmController alarm, List<string> alarmFields) {
        if (alarmFields.Count < minimumDeviationAndRateOfChangeAlarmFieldsNumber)
            throw new Exception("Some parameters are missing");

        InitializeBaseAlarmConfiguration(alarm, alarmFields);
        InitializeBaseLevelAlarmConfiguration(alarm, alarmFields);
        TrySetDoubleProperty(alarm, "PollingTime", alarmFields[pollingTimeField]);
    }

    private void InitializeNonExclusiveRateOfChangeAlarmConfiguration(NonExclusiveRateOfChangeAlarmController alarm, List<string> alarmFields) {
        if (alarmFields.Count < minimumDeviationAndRateOfChangeAlarmFieldsNumber)
            throw new Exception("Some parameters are missing");

        InitializeBaseAlarmConfiguration(alarm, alarmFields);
        InitializeBaseLevelAlarmConfiguration(alarm, alarmFields);
        TrySetDoubleProperty(alarm, "PollingTime", alarmFields[pollingTimeField]);
    }

    private void InitializeBaseAlarmConfiguration(AlarmController alarm, List<string> alarmFields) {
        SetInputValueProperty(alarm, alarmFields);
        TrySetBooleanOptionalProperty(alarm, "Enabled", alarmFields[enabledField]);
        TrySetBooleanOptionalProperty(alarm, "AutoAcknowledge", alarmFields[autoAcknowledgeField]);
        TrySetBooleanOptionalProperty(alarm, "AutoConfirm", alarmFields[autoConfirmField]);
        TrySetUshortOptionalProperty(alarm, "Severity", alarmFields[severifyField]);

        var message = alarmFields[messageField];

        // Interpret the message field read by the current alarm as TextID if MessageAsTranslationKey is set to true and
        // perform a lookup in the translation table
        if (GetMessageAsTranslationKey()) {
            var localizedMessage = new LocalizedText(message);
            if (!InformationModel.LookupTranslation(localizedMessage).HasTranslation) {
                Log.Warning("ImportExportAlarms", $"Alarm {alarm.BrowseName} Message with translation key \"{message}\" was not found (MessageAsTranslationKey = true)");
                return;
            }

            alarm.LocalizedMessage = localizedMessage;
        } else if (!string.IsNullOrEmpty(message)) {
            alarm.Message = message;
        }
    }

    private void SetInputValueProperty(AlarmController alarm, List<string> alarmFields) {
        if (alarmFields[inputVariablePathField] == "")
            return;

        var inputVariable = Project.Current.GetVariable(alarmFields[inputVariablePathField]);
        if (inputVariable == null)
            throw new Exception("input variable " + alarmFields[inputVariablePathField] + " not found");

        if (uint.TryParse(alarmFields[inputVariableIndexField], out uint index))
            alarm.InputValueVariable.SetDynamicLink(inputVariable, index);
        else
            alarm.InputValueVariable.SetDynamicLink(inputVariable);
    }

    private void InitializeBaseLevelAlarmConfiguration(LimitAlarmController alarm, List<string> alarmFields) {
        TrySetDoubleOptionalProperty(alarm, "HighHighLimit", alarmFields[highHighLimitField]);
        TrySetDoubleOptionalProperty(alarm, "HighLimit", alarmFields[highLimitField]);
        TrySetDoubleOptionalProperty(alarm, "LowLimit", alarmFields[lowLimitField]);
        TrySetDoubleOptionalProperty(alarm, "LowLowLimit", alarmFields[lowLowLimitField]);
    }

    private void TrySetDoubleOptionalProperty(AlarmController alarm, string propertyName, string propertyValue) {
        if (propertyValue == "")
            return;

        if (!double.TryParse(propertyValue, out double value))
            throw new Exception("Parameter " + propertyName + "is not a valid double");

        alarm.SetOptionalVariableValue(propertyName, value);
    }

    private void TrySetDoubleProperty(AlarmController alarm, string propertyName, string propertyValue) {
        if (propertyValue == "")
            throw new Exception("Missing mandatory parameter " + propertyName);

        if (!double.TryParse(propertyValue, out double value))
            throw new Exception("Parameter " + propertyName + " is not a valid double");

        var propertyNode = alarm.GetVariable(propertyName);
        propertyNode.SetValue(value);
    }

    private void TrySetBooleanOptionalProperty(AlarmController alarm, string propertyName, string propertyValue) {
        if (propertyValue == "")
            return;

        alarm.SetOptionalVariableValue(propertyName, ConvertStringToBool(propertyValue));
    }

    private void TrySetUshortOptionalProperty(AlarmController alarm, string propertyName, string propertyValue) {
        if (propertyValue == "")
            return;

        if (!ushort.TryParse(propertyValue, out ushort value))
            throw new Exception("Parameter " + propertyName + " is not a valid ushort");

        alarm.SetOptionalVariableValue(propertyName, value);
    }

    private bool ConvertStringToBool(string value) {
        return value == "1" || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }

    private List<string> CollectAlarmConfiguration(AlarmController alarm) {
        var alarmFields = new List<string>();

        if (alarm is DigitalAlarm)
            ExportDigitalAlarmConfiguration((DigitalAlarm)alarm, alarmFields);
        else if (alarm is ExclusiveLevelAlarmController)
            ExportExclusiveLevelAlarmConfiguration((ExclusiveLevelAlarmController)alarm, alarmFields);
        else if (alarm is NonExclusiveLevelAlarmController)
            ExportNonExclusiveLevelAlarmConfiguration((NonExclusiveLevelAlarmController)alarm, alarmFields);
        else if (alarm is ExclusiveDeviationAlarmController)
            ExportExclusiveDeviationAlarmConfiguration((ExclusiveDeviationAlarmController)alarm, alarmFields);
        else if (alarm is NonExclusiveDeviationAlarmController)
            ExportNonExclusiveDeviationAlarmConfiguration((NonExclusiveDeviationAlarmController)alarm, alarmFields);
        else if (alarm is ExclusiveRateOfChangeAlarmController)
            ExportExclusiveRateOfChangeAlarmConfiguration((ExclusiveRateOfChangeAlarmController)alarm, alarmFields);
        else if (alarm is NonExclusiveRateOfChangeAlarmController)
            ExportNonExclusiveRateOfChangeAlarmConfiguration((NonExclusiveRateOfChangeAlarmController)alarm, alarmFields);

        return alarmFields;
    }

    private void ExportDigitalAlarmConfiguration(DigitalAlarm alarm, List<string> alarmFields) {
        ExportBaseAlarmConfiguration(alarm, alarmFields);
        ExportOptionalProperty(alarm.GetVariable("NormalStateValue"), alarmFields);
    }

    private void ExportExclusiveLevelAlarmConfiguration(ExclusiveLevelAlarmController alarm, List<string> alarmFields) {
        ExportBaseAlarmConfiguration(alarm, alarmFields);
        ExportBaseLevelAlarmConfiguration(alarm, alarmFields);
    }

    private void ExportNonExclusiveLevelAlarmConfiguration(NonExclusiveLevelAlarmController alarm, List<string> alarmFields) {
        ExportBaseAlarmConfiguration(alarm, alarmFields);
        ExportBaseLevelAlarmConfiguration(alarm, alarmFields);
    }

    private void ExportExclusiveDeviationAlarmConfiguration(ExclusiveDeviationAlarmController alarm, List<string> alarmFields) {
        ExportBaseAlarmConfiguration(alarm, alarmFields);
        ExportBaseLevelAlarmConfiguration(alarm, alarmFields);
        alarmFields.Add(alarm.Setpoint.ToString());
    }

    private void ExportNonExclusiveDeviationAlarmConfiguration(NonExclusiveDeviationAlarmController alarm, List<string> alarmFields) {
        ExportBaseAlarmConfiguration(alarm, alarmFields);
        ExportBaseLevelAlarmConfiguration(alarm, alarmFields);
        alarmFields.Add(alarm.Setpoint.ToString());
    }

    private void ExportExclusiveRateOfChangeAlarmConfiguration(ExclusiveRateOfChangeAlarmController alarm, List<string> alarmFields) {
        ExportBaseAlarmConfiguration(alarm, alarmFields);
        ExportBaseLevelAlarmConfiguration(alarm, alarmFields);
        alarmFields.Add(alarm.PollingTime.ToString());
    }

    private void ExportNonExclusiveRateOfChangeAlarmConfiguration(NonExclusiveRateOfChangeAlarmController alarm, List<string> alarmFields) {
        ExportBaseAlarmConfiguration(alarm, alarmFields);
        ExportBaseLevelAlarmConfiguration(alarm, alarmFields);
        alarmFields.Add(alarm.PollingTime.ToString());
    }

    private void ExportBaseAlarmConfiguration(AlarmController alarm, List<string> alarmFields) {
        alarmFields.Add(alarm.ObjectType.BrowseName);
        alarmFields.Add(alarm.BrowseName);
        ExportAlarmInputVariable(alarm, alarmFields);
        ExportOptionalProperty(alarm.GetVariable("Enabled"), alarmFields);
        ExportOptionalProperty(alarm.GetVariable("AutoAcknowledge"), alarmFields);
        ExportOptionalProperty(alarm.GetVariable("AutoConfirm"), alarmFields);

        if (GetMessageAsTranslationKey()) {
            // When MessageAsTranslationKey is set to true, we need to export the TextID of Message (not the Message Text)
            var localizedTextMessage = alarm.LocalizedMessage;
            if (localizedTextMessage != null && localizedTextMessage.HasTextId)
                alarmFields.Add(localizedTextMessage.TextId);
            else {
                Log.Warning("ImportExportAlarms", $"Message of alarm {alarm.BrowseName} has no translation key. Message of this alarm will not exported (MessageAsTranslationKey = true)");
                alarmFields.Add("");
            }
        } else {
            // When MessageAsTranslationKey is set to false, we need to export the content of Message
            if (alarm.GetVariable("Message") != null)
                alarmFields.Add(alarm.Message);
            else
                alarmFields.Add("");
        }

        ExportOptionalProperty(alarm.GetVariable("Severity"), alarmFields);
    }

    private void ExportOptionalProperty(IUAVariable property, List<string> alarmFields) {
        if (property == null)
            alarmFields.Add("");
        else
            alarmFields.Add(property.Value);
    }

    private void ExportAlarmInputVariable(AlarmController alarm, List<string> alarmFields) {
        var inputPath = (DynamicLink)alarm.InputValueVariable.GetVariable("DynamicLink");
        if (inputPath == null) {
            alarmFields.Add("");
            alarmFields.Add("");
        } else {
            var resolvePathResult = LogicObject.Context.ResolvePath(alarm.InputValueVariable, inputPath.Value);
            var pathToInputValueNode = MakeBrowsePath(resolvePathResult.ResolvedNode);
            alarmFields.Add(pathToInputValueNode);

            if (resolvePathResult.Indexes != null && resolvePathResult.Indexes.Length > 0)
                alarmFields.Add(resolvePathResult.Indexes[0].ToString());
            else
                alarmFields.Add("");
        }
    }

    private void ExportBaseLevelAlarmConfiguration(LimitAlarmController alarm, List<string> alarmFields) {
        ExportOptionalProperty(alarm.GetVariable("HighHighLimit"), alarmFields);
        ExportOptionalProperty(alarm.GetVariable("HighLimit"), alarmFields);
        ExportOptionalProperty(alarm.GetVariable("LowLimit"), alarmFields);
        ExportOptionalProperty(alarm.GetVariable("LowLowLimit"), alarmFields);
    }

    private string GetCSVFilePath() {
        var csvPathVariable = LogicObject.GetVariable("CSVPath");
        if (csvPathVariable == null) {
            Log.Error("ImportExportAlarms", "CSVPath variable not found");
            return "";
        }

        return new ResourceUri(csvPathVariable.Value).Uri;
    }

    private char? GetFieldDelimiter() {
        var separatorVariable = LogicObject.GetVariable("CharacterSeparator");
        if (separatorVariable == null) {
            Log.Error("ImportExportAlarms", "CharacterSeparator variable not found");
            return null;
        }

        string separator = separatorVariable.Value;

        if (separator.Length != 1 || separator == String.Empty) {
            Log.Error("ImportExportAlarms", "Wrong CharacterSeparator configuration. Please insert a char");
            return null;
        }

        if (char.TryParse(separator, out char result))
            return result;

        return null;
    }

    private bool GetWrapFields() {
        var wrapFieldsVariable = LogicObject.GetVariable("WrapFields");
        if (wrapFieldsVariable == null) {
            Log.Error("ImportExportAlarms", "WrapFields variable not found");
            return false;
        }

        return wrapFieldsVariable.Value;
    }

    private bool GetMessageAsTranslationKey() {
        var messageAsTranslationKeyVariable = LogicObject.GetVariable("MessageAsTranslationKey");
        return messageAsTranslationKeyVariable == null ? false : (bool)messageAsTranslationKeyVariable.Value;
    }

    private List<AlarmController> GetAlarmList() {
        var alarms = new List<AlarmController>();

        var digitalAlarms = GetAlarmsByType(FTOptix.Alarm.ObjectTypes.OffNormalAlarmController);
        foreach (var digitalAlarm in digitalAlarms)
            alarms.Add((DigitalAlarm)digitalAlarm);

        var exclusiveLevelAlarms = GetAlarmsByType(FTOptix.Alarm.ObjectTypes.ExclusiveLevelAlarmController);
        foreach (var exclusiveLevelAlarm in exclusiveLevelAlarms)
            alarms.Add((ExclusiveLevelAlarmController)exclusiveLevelAlarm);

        var nonExclusiveLevelAlarms = GetAlarmsByType(FTOptix.Alarm.ObjectTypes.NonExclusiveLevelAlarmController);
        foreach (var nonExclusiveLevelAlarm in nonExclusiveLevelAlarms)
            alarms.Add((NonExclusiveLevelAlarmController)nonExclusiveLevelAlarm);

        var exclusiveDeviationAlarms = GetAlarmsByType(FTOptix.Alarm.ObjectTypes.ExclusiveDeviationAlarmController);
        foreach (var exclusiveDeviationAlarm in exclusiveDeviationAlarms)
            alarms.Add((ExclusiveDeviationAlarmController)exclusiveDeviationAlarm);

        var nonExclusiveDeviationAlarms = GetAlarmsByType(FTOptix.Alarm.ObjectTypes.NonExclusiveDeviationAlarmController);
        foreach (var nonExclusiveDeviationAlarm in nonExclusiveDeviationAlarms)
            alarms.Add((NonExclusiveDeviationAlarmController)nonExclusiveDeviationAlarm);

        var exclusiveRateOfChangeAlarms = GetAlarmsByType(FTOptix.Alarm.ObjectTypes.ExclusiveRateOfChangeAlarmController);
        foreach (var exclusiveRateOfChangeAlarm in exclusiveRateOfChangeAlarms)
            alarms.Add((ExclusiveRateOfChangeAlarmController)exclusiveRateOfChangeAlarm);

        var nonExclusiveRateOfChangeAlarms = GetAlarmsByType(FTOptix.Alarm.ObjectTypes.NonExclusiveRateOfChangeAlarmController);
        foreach (var nonExclusiveRateOfChangeAlarm in nonExclusiveRateOfChangeAlarms)
            alarms.Add((NonExclusiveRateOfChangeAlarmController)nonExclusiveRateOfChangeAlarm);

        return alarms;
    }

    private IReadOnlyList<IUAObject> GetAlarmsByType(NodeId type) {
        var digitalAlarmType = LogicObject.Context.GetObjectType(type);
        var digitalAlarms = digitalAlarmType.InverseRefs.GetObjects(OpcUa.ReferenceTypes.HasTypeDefinition, false);
        return digitalAlarms;
    }

    private string MakeBrowsePath(IUANode node) {
        string path = node.BrowseName;
        var current = node.Owner;

        while (current != Project.Current) {
            path = current.BrowseName + "/" + path;
            current = current.Owner;
        }
        return path;
    }

    private Dictionary<string, Func<string, AlarmController>> alarmFactoryMap = new Dictionary<string, Func<string, AlarmController>>()
    {
        { "OffNormalAlarmControllerType", browseName => InformationModel.MakeObject<DigitalAlarm>(browseName) },
        { "OffNormalAlarmController", browseName => InformationModel.MakeObject<DigitalAlarm>(browseName) },
        { "ExclusiveLevelAlarmControllerType", browseName => InformationModel.MakeObject<ExclusiveLevelAlarmController>(browseName) },
        { "ExclusiveLevelAlarmController", browseName => InformationModel.MakeObject<ExclusiveLevelAlarmController>(browseName) },
        { "NonExclusiveLevelAlarmControllerType", browseName => InformationModel.MakeObject<NonExclusiveLevelAlarmController>(browseName) },
        { "NonExclusiveLevelAlarmController", browseName => InformationModel.MakeObject<NonExclusiveLevelAlarmController>(browseName) },
        { "ExclusiveDeviationAlarmControllerType", browseName => InformationModel.MakeObject<ExclusiveDeviationAlarmController>(browseName) },
        { "ExclusiveDeviationAlarmController", browseName => InformationModel.MakeObject<ExclusiveDeviationAlarmController>(browseName) },
        { "NonExclusiveDeviationAlarmControllerType", browseName => InformationModel.MakeObject<NonExclusiveDeviationAlarmController>(browseName) },
        { "NonExclusiveDeviationAlarmController", browseName => InformationModel.MakeObject<NonExclusiveDeviationAlarmController>(browseName) },
        { "ExclusiveRateOfChangeAlarmControllerType", browseName => InformationModel.MakeObject<ExclusiveRateOfChangeAlarmController>(browseName) },
        { "ExclusiveRateOfChangeAlarmController", browseName => InformationModel.MakeObject<ExclusiveRateOfChangeAlarmController>(browseName) },
        { "NonExclusiveRateOfChangeAlarmControllerType", browseName => InformationModel.MakeObject<NonExclusiveRateOfChangeAlarmController>(browseName) },
        { "NonExclusiveRateOfChangeAlarmController", browseName => InformationModel.MakeObject<NonExclusiveRateOfChangeAlarmController>(browseName) }
    };

    private const int minimumFieldsNumber = 10;
    private const int minimumLimitAlarmFieldsNumber = 13;
    private const int minimumDeviationAndRateOfChangeAlarmFieldsNumber = 14;
    private const int minimumDigitalAlarmFieldsNumber = 10;

    private const int alarmTypeField = 0;
    private const int alarmBrowseNameField = 1;
    private const int inputVariablePathField = 2;
    private const int inputVariableIndexField = 3;
    private const int enabledField = 4;
    private const int autoAcknowledgeField = 5;
    private const int autoConfirmField = 6;
    private const int messageField = 7;
    private const int severifyField = 8;
    private const int normalValueField = 9;
    private const int highHighLimitField = 9;
    private const int highLimitField = 10;
    private const int lowLimitField = 11;
    private const int lowLowLimitField = 12;
    private const int setPointField = 13;
    private const int pollingTimeField = 13;

    #region CSVFileReader
    private class CSVFileReader : IDisposable {
        public char FieldDelimiter { get; set; } = ',';

        public char QuoteChar { get; set; } = '"';

        public bool WrapFields { get; set; } = false;

        public bool IgnoreMalformedLines { get; set; } = false;

        public CSVFileReader(string filePath, System.Text.Encoding encoding) {
            streamReader = new StreamReader(filePath, encoding);
        }

        public CSVFileReader(string filePath) {
            streamReader = new StreamReader(filePath, System.Text.Encoding.UTF8);
        }

        public CSVFileReader(StreamReader streamReader) {
            this.streamReader = streamReader;
        }

        public bool EndOfFile() {
            return streamReader.EndOfStream;
        }

        public List<string> ReadLine() {
            if (EndOfFile())
                return null;

            var line = streamReader.ReadLine();

            var result = WrapFields ? ParseLineWrappingFields(line) : ParseLineWithoutWrappingFields(line);

            currentLineNumber++;
            return result;

        }

        public List<List<string>> ReadAll() {
            var result = new List<List<string>>();
            while (!EndOfFile())
                result.Add(ReadLine());

            return result;
        }

        private List<string> ParseLineWithoutWrappingFields(string line) {
            if (string.IsNullOrEmpty(line) && !IgnoreMalformedLines)
                throw new FormatException($"Error processing line {currentLineNumber}. Line cannot be empty");

            return line.Split(FieldDelimiter).ToList();
        }

        private List<string> ParseLineWrappingFields(string line) {
            var fields = new List<string>();
            var buffer = new StringBuilder("");
            var fieldParsing = false;

            int i = 0;
            while (i < line.Length) {
                if (!fieldParsing) {
                    if (IsWhiteSpace(line, i)) {
                        ++i;
                        continue;
                    }

                    // Line and column numbers must be 1-based for messages to user
                    var lineErrorMessage = $"Error processing line {currentLineNumber}";
                    if (i == 0) {
                        // A line must begin with the quotation mark
                        if (!IsQuoteChar(line, i)) {
                            if (IgnoreMalformedLines)
                                return null;
                            else
                                throw new FormatException($"{lineErrorMessage}. Expected quotation marks at column {i + 1}");
                        }

                        fieldParsing = true;
                    } else {
                        if (IsQuoteChar(line, i))
                            fieldParsing = true;
                        else if (!IsFieldDelimiter(line, i)) {
                            if (IgnoreMalformedLines)
                                return null;
                            else
                                throw new FormatException($"{lineErrorMessage}. Wrong field delimiter at column {i + 1}");
                        }
                    }

                    ++i;
                } else {
                    if (IsEscapedQuoteChar(line, i)) {
                        i += 2;
                        buffer.Append(QuoteChar);
                    } else if (IsQuoteChar(line, i)) {
                        fields.Add(buffer.ToString());
                        buffer.Clear();
                        fieldParsing = false;
                        ++i;
                    } else {
                        buffer.Append(line[i]);
                        ++i;
                    }
                }
            }

            return fields;
        }

        private bool IsEscapedQuoteChar(string line, int i) {
            return line[i] == QuoteChar && i != line.Length - 1 && line[i + 1] == QuoteChar;
        }

        private bool IsQuoteChar(string line, int i) {
            return line[i] == QuoteChar;
        }

        private bool IsFieldDelimiter(string line, int i) {
            return line[i] == FieldDelimiter;
        }

        private bool IsWhiteSpace(string line, int i) {
            return Char.IsWhiteSpace(line[i]);
        }

        private StreamReader streamReader;
        private int currentLineNumber = 1;

        #region IDisposable support
        private bool disposed = false;
        protected virtual void Dispose(bool disposing) {
            if (disposed)
                return;

            if (disposing)
                streamReader.Dispose();

            disposed = true;
        }

        public void Dispose() {
            Dispose(true);
        }
        #endregion
    }
    #endregion

    #region CSVFileWriter
    private class CSVFileWriter : IDisposable {
        public char FieldDelimiter { get; set; } = ',';

        public char QuoteChar { get; set; } = '"';

        public bool WrapFields { get; set; } = false;

        public CSVFileWriter(string filePath) {
            streamWriter = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
        }

        public CSVFileWriter(string filePath, System.Text.Encoding encoding) {
            streamWriter = new StreamWriter(filePath, false, encoding);
        }

        public CSVFileWriter(StreamWriter streamWriter) {
            this.streamWriter = streamWriter;
        }

        public void WriteLine(string[] fields) {
            var stringBuilder = new StringBuilder();

            for (var i = 0; i < fields.Length; ++i) {
                if (WrapFields)
                    stringBuilder.AppendFormat("{0}{1}{0}", QuoteChar, EscapeField(fields[i]));
                else
                    stringBuilder.AppendFormat("{0}", fields[i]);

                if (i != fields.Length - 1)
                    stringBuilder.Append(FieldDelimiter);
            }

            streamWriter.WriteLine(stringBuilder.ToString());
            streamWriter.Flush();
        }

        private string EscapeField(string field) {
            var quoteCharString = QuoteChar.ToString();
            return field.Replace(quoteCharString, quoteCharString + quoteCharString);
        }

        private StreamWriter streamWriter;

        #region IDisposable Support
        private bool disposed = false;
        protected virtual void Dispose(bool disposing) {
            if (disposed)
                return;

            if (disposing)
                streamWriter.Dispose();

            disposed = true;
        }

        public void Dispose() {
            Dispose(true);
        }

        #endregion
    }
    #endregion
}
