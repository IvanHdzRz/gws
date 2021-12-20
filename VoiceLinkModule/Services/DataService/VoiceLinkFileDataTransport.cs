//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Logging;
    using GuidedWork;
    using GuidedWorkRunner;

    /// <summary>
    /// Helper extension to make it cleaner to add items to dictionary 
    /// if they do not already exists
    /// </summary>
    public static class DictionaryExtensionsClass
    {
        public static void AddIfNotExists<T, U>(this Dictionary<T, U> dict, T key, U create)
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, create);
            }
        }
    }

    public class VoiceLinkFileDataTransport : WorkflowFileDataTransport, IVoiceLinkDataTransport
    {
        private readonly ILog _Log = LogManager.GetLogger(nameof(VoiceLinkFileDataTransport));
        public Dictionary<string, List<string>> Responses { get; set; } = null;
        private readonly IConfigurationDataService _ConfigurationDataService;
        private bool _MultipleManualAssignments = true;

        public VoiceLinkFileDataTransport(IWorkflowParameterService workflowParameterService,
            IWorkflowResourceRegistry workflowResourceRegistry,
            IConfigurationDataService configurationDataService) : base(workflowParameterService, workflowResourceRegistry)
        {
            _ConfigurationDataService = configurationDataService;
        }

        public void Initialize()
        {
            Responses = _ConfigurationDataService.LoadConfigurationObject<Dictionary<string, List<string>>>("VoiceLinkDemo.dataset") ?? new Dictionary<string, List<string>>();
            Responses.AddIfNotExists("prTaskLUTCoreConfiguration", new List<string> { "Vocollect,,0,0," });
            Responses.AddIfNotExists("prTaskLUTCoreBreakTypes", new List<string> { ",,0," });
            Responses.AddIfNotExists("prTaskLUTCoreValidFunctions", new List<string> { "3,Normal Assignments,0,",
                                                                                       "4,Chase Assignments,0,",
                                                                                       "6,Normal And Chase Assignments,0,"});
            Responses.AddIfNotExists("prTaskLUTRegionPermissionsForWorkType", new List<string> { "101,Demo 101,0,", "102,Demo 102,0," });
            Responses.AddIfNotExists("prTaskLUTPickingRegion", new List<string> { "101,Region 101,1,0,1,1,1,0,1,0,0,1,1,1,0,0,0,4,0,0,0,X,X,X,X,0,0,0,2,0,0,",
                                                                                  "102,Region 102,1,1,1,1,1,0,2,0,2,1,0,0,1,0,1,-1,0,0,0,X,X,X,X,0,0,0,2,0,0,"});
            Responses.AddIfNotExists("prTaskLUTRequestWork", new List<string> { "1020006,4,",
                                                                                "1030006,4,",
                                                                                "1040006,4,"});
            Responses.AddIfNotExists("prTaskLUTGetAssignment", new List<string> { "100,0,706,1020006,1,6.00,6,       00,0,0,,0," });
            Responses.AddIfNotExists("prTaskLUTGetPicks", new List<string> { "N,0,3526,2004,102,Building 751 Site 1,7 5 1,Bay 751,7 5 1,1,,751,0,0.0,0.0,0,25,751,751,Item 751 Site 1,751,751,706,1020006,6,0,6,,,0,,Pick Message Site 1,0,0,0,0,0,",
                                                                             "N,0,3527,2005,102,Building 752 Site 1,7 5 2,Bay 752,7 5 2,2,,752,0,0.0,0.0,0,25,752,752,Item 752 Site 1,752,752,706,1020006,6,0,6,,,0,,Pick Message Site 1,0,0,0,0,0,",
                                                                             "N,0,3528,2006,102,Building 753 Site 1,7 5 3,Bay 753,7 5 3,3,,753,0,0.0,0.0,0,25,753,753,Item 753 Site 1,753,753,706,1020006,6,0,6,,,0,,Pick Message Site 1,0,0,0,0,0,",
                                                                             "N,0,3529,2007,102,Building 754 Site 1,7 5 4,Bay 754,7 5 4,4,,754,0,0.0,0.0,0,25,754,754,Item 754 Site 1,754,754,706,1020006,6,0,6,,,0,,Pick Message Site 1,0,0,0,0,0,",
                                                                             "N,0,3530,2008,102,Building 755 Site 1,7 5 5,Bay 755,7 5 5,5,,755,0,0.0,0.0,0,25,755,755,Item 755 Site 1,755,755,706,1020006,6,0,6,,,0,,Pick Message Site 1,0,0,0,0,0," });

            LoadIndivdualFiles();
        }

        /// <summary>
        /// Looks for lone csv files to load that override any already loaded configurations
        /// </summary>
        private void LoadIndivdualFiles()
        {
            Dictionary<string, string> recordDefaults = new Dictionary<string, string>
            {
                { "prTaskLUTCoreConfiguration", "Vocollect,,0,0," },
                { "prTaskLUTCoreBreakTypes", "0,,0," },
                { "prTaskLUTCoreValidFunctions", "0,,0," },
                { "prTaskLUTRegionPermissionsForWorkType", "0,,0," },
                { "prTaskLUTPickingRegion", ",,1,0,1,1,1,0,1,0,0,1,1,1,0,0,0,4,0,0,0,X,X,X,X,0,0,0,2,0,0," },
                { "prTaskLUTRequestWork", ",4," },
                { "prTaskLUTGetAssignment", "100,0,706,1020006,1,0.00,,0,0,0,,0," },
                { "prTaskLUTGetPicks", "N,0,{count},{count},000,,,,XXX,1,,Sample Item Number,0,0.0,0.0,0,99,,,Sample Item Description,size,Sample UPC,706,1020006,,0,Sample Store,,,0,,,0,0,0,0,0," }
            };

            foreach (KeyValuePair<string, string> defaultValues in recordDefaults)
            {
                string[] contents = _ConfigurationDataService.LoadConfigFileLines($"{defaultValues.Key}.csv");
                //Expected format of lines in file are
                // Line 0 = Field indexes of each column in comma separated file
                // Line 1 = Field headers/descriptions, not used here, only helpful when creating file
                // Line 2+ = Data records to use in data set. 
                if (contents != null && contents.Length > 2)
                {
                    try
                    {
                        int[] indexes = Array.ConvertAll(contents[0].Split(','), s => int.Parse(s));
                        Responses[defaultValues.Key].Clear(); //Remove existing records so they can be replace with what is in the file
                        for (int i = 2; i < contents.Length; i++)
                        {
                            //Create new record of all fields split based on commas, based on default values
                            string[] record = defaultValues.Value.Split(',');
                            string[] values = contents[i].Split(',');
                            //Replace default values with values from file.
                            for (int j = 0; j < indexes.Length; j++)
                            {
                                record[indexes[j]] = values[j];
                            }
                            //Join values back together into comma separated string, and add to record set.
                            Responses[defaultValues.Key].Add(string.Join(",", record).Replace("{count}", i.ToString()));
                        }
                    }
                    catch (Exception e)
                    {
                        _Log.ErrorFormat("Error loading file {0}.csv for demo data", e, Responses[defaultValues.Key]);
                    }
                }
            }
        }

        private Task<string> FormatResponse(List<string> records)
        {
            return Task.FromResult<string>(string.Join("@r@@n@", records) + "@r@@n@@n@@n@");
        }

        public Task<string> GetBreakTypesAsync()
        {
            return FormatResponse(Responses["prTaskLUTCoreBreakTypes"]);
        }

        public Task<string> GetRegionPermissionsForWorkTypeAsync(int workType)
        {
            return FormatResponse(Responses["prTaskLUTRegionPermissionsForWorkType"]);
        }

        public Task<string> GetPickingRegionForWorkTypeAsync(string pickingRegion, int workType)
        {
            _MultipleManualAssignments = true;
            List<string> regionResponse = new List<string> { Responses["prTaskLUTPickingRegion"][0] };
            foreach (string response in Responses["prTaskLUTPickingRegion"])
            {
                if (response.StartsWith(pickingRegion))
                {
                    regionResponse[0] = response;
                }
            }
            return FormatResponse(regionResponse);
        }

        public Task<string> GetRequestWorkAsync(string workId, int scanned, int assignmentType)
        {
            if (_MultipleManualAssignments && Responses["prTaskLUTRequestWork"].Count > 1)
            {
                _MultipleManualAssignments = false;
                return FormatResponse(Responses["prTaskLUTRequestWork"]);
            }
            else
            {
                return FormatResponse(new List<string> { "12345,0,"});
            }
        }

        public Task<string> GetAssignmentAsync(int numberOfAssignments, int assignmentType)
        {
            return FormatResponse(Responses["prTaskLUTGetAssignment"]);
        }

        public Task<string> GetPicksAsync(long? groupId, bool shortsAndSkipsFlag, int goBackForSkipsIndicator, int pickOrderFlag)
        {
            return FormatResponse(Responses["prTaskLUTGetPicks"]);
        }

        public Task<string> GetValidFunctionsAsync(int taskId)
        {
            return FormatResponse(Responses["prTaskLUTCoreValidFunctions"]);
        }


        public Task<string> SendConfigAsync()
        {
            return FormatResponse(Responses["prTaskLUTCoreConfiguration"]);
        }

        public Task<string> SignOnAsync(GuidedWorkRunner.Operator oper)
        {
            if (oper.Password == "123")
            {
                return FormatResponse(new List<string> { "0,0," });
            }
            return FormatResponse(new List<string> { ",1001,Invalid password for operator" });
        }

        public Task<string> PassAssignmentAsync(long? groupId)
        {
            return FormatResponse(new List<string> { "0," });
        }

        public Task<string> SignOffAsync()
        {
            return FormatResponse(new List<string> { "0," });
        }

        public Task<string> StopAssignmentAsync(long? groupId)
        {
            return FormatResponse(new List<string> { "0," });
        }

        public Task<string> UpdateStatusAsync(long? groupId, long? locationId, string slotAisle, string setStatusTo, int useLuts)
        {
            return FormatResponse(new List<string> { "0," });
        }

        public Task<string> VerifyReplenishmentAsync(long locationId, string itemNumber)
        {
            return FormatResponse(new List<string> { "0,1," });
        }

        public Task<string> GetContainersAsync(long? groupId, long assignmentId, string targetContainer, long? pickContainerId, string containerNumber, int operation, string labels)
        {
            return Task.FromResult<string>(",,,,,,,,0,");
        }

        public Task<string> ExecutePickAsync(long? groupId, long assignmentId, long locationId, int quantityPicked, bool endOfPartialPickingFlag, long? containerId, long pickId, string lotNumber, double? variableWeight, string itemSerialNumer, int useLuts)
        {
            return Task.FromResult<string>("0");
        }
    }
}
