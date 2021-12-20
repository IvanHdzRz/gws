//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////
namespace LApp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Logging;
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.CoreLibrary.Localization;

    public class LAppModuleVocab : DefaultModuleVocab
    {
        //Build common platform independent vocab 
        public override VocabWordInfo[] PlatformIndependentVocab { get; } =
            Numerics
                .Concat(Alphas)
                .Concat(BaseRequiredVocab)
                .Concat(Common)
            .ToArray();

        //Defined platform independent, app specific vocab as static for easy reference later
        public static readonly VocabWordInfo ChangeWareHouse = new VocabWordInfo("LApp_Overflow_ChangeWarehouse");
        public static readonly VocabWordInfo SkipProduct = new VocabWordInfo("LApp_Overflow_SkipProduct");
        public static readonly VocabWordInfo Pallet = new VocabWordInfo("LApp_LicensePlate_Pallet");
        public static readonly VocabWordInfo Box = new VocabWordInfo("LApp_LicensePlate_Box");
        public static readonly VocabWordInfo Tote = new VocabWordInfo("LApp_LicensePlate_Tote");

    }

    #region modelDefintion
    public enum ReturnStateActionEnum
    {
        Next,
        Cancel
    }

    public enum DamageTypes
    {
        Unspecified,
        Cosmetic,
        Mechanical,
        Electrical
    }

    public enum DamageSeverities
    {
        Unspecified = 0,
        Minor = 1,
        Moderate = 2,
        Severe = 3
    }


    public interface ILAppModel : IGenericIntentModel<ILAppModel>
    {
        ILAppDataService DataService { get; }
        ILAppConfigRepository LAppConfigRepository { get; }
        IConfigurationDataService ConfigurationDataService { get; }

        ReturnStateActionEnum ReturnStateAction { get; set; }

        List<Warehouse> Warehouses { get; set; }
        Warehouse CurrentWarehouse { get; set; }

        List<Zone> Zones { get; set; }
        Zone CurrentZone { get; set; }

        Dictionary<DamageTypes, DamageSeverities> Damages { get; set; }

        int CurrentTransactionId { get; set; }

        List<int> Orders { get; set; }
        int? CurrentOrder { get; set; }

        PickRoute CurrentPickRoute { get; set; }

        string CurrentLicensePlateName { get; set; }
        int CurrentLicensePlateId { get; set; }

        string EnteredSerialNumber { get; set; }
        string SelectedBatchNumber { get; set; }

        string LocationResponse { get; set; }
        string ProductCheckDigitResponse { get; set; }

        string EnteredQuantityString { get; set; }
        int EnteredQuantity { get; set; }

        List<string> ValidSerialNumbers { get; set; }
        List<string> ValidBatchNumbers { get; set; }

        DateTime CurrentLicensePlateLastVisitDate { get; set; }

        void ResetOperator();
        void ResetWarehouse();
        void DeletePhotos();

        /// <summary>
        /// Returns whether or not there are more LApp picks to handle.
        /// </summary>
        bool MoreWorkItems { get; }

        /// <summary>
        /// Returns List that stores Photo capture intent result. This is a list of file paths captured or selected by the user
        /// </summary>
        List<Uri> FilePathList { get; set; }

        /// <summary>
        /// Returns whether or not the product is damaged
        /// </summary>
        bool? CheckProductDamagedResponse { get; set; }
        List<DamageTypes> SpecifiedDamages { get; set; }

        /// <summary>
        /// Returns the total step count for the progress bar
        /// </summary>
        int TotalStepCount { get; set; }

        /// <summary>
        /// Returns the current step count for the progress bar
        /// </summary>
        int CurrentStepCount { get; set; }

        /// <summary>
        /// The the type of container that this license plate belongs to
        /// </summary>
        VocabWordInfo LicensePlateContainerType { get; set; }
    }

    public static class LAppMappings
    {
        public static Dictionary<string, DamageTypes> MapDamageTypesToEnum { get; } = new Dictionary<string, DamageTypes>()
                {
                    { Translate.GetLocalizedTextForKey("LApp_CosmeticDamage"), DamageTypes.Cosmetic},
                    { Translate.GetLocalizedTextForKey("LApp_MechanicalDamage"), DamageTypes.Mechanical},
                    { Translate.GetLocalizedTextForKey("LApp_ElectricalDamage"), DamageTypes.Electrical}
                };

        public static Dictionary<DamageTypes, string> MapDamageTypesToString { get; } = MapDamageTypesToEnum.ToDictionary((i) => i.Value, (i) => i.Key);

        public static Dictionary<string, DamageSeverities> MapSeveritiesToEnum { get; } = new Dictionary<string, DamageSeverities>()
                {
                    { Translate.GetLocalizedTextForKey("LApp_DamageSevere"), DamageSeverities.Severe},
                    { Translate.GetLocalizedTextForKey("LApp_DamageModerate"), DamageSeverities.Moderate},
                    { Translate.GetLocalizedTextForKey("LApp_DamageMinor"), DamageSeverities.Minor}
                };

        public static Dictionary<DamageSeverities, string> MapSeveritiesToString { get; } = MapSeveritiesToEnum.ToDictionary((i) => i.Value, (i) => i.Key);
    }

    public class LAppModel : SimplifiedIntentModel<LAppBusinessLogic, ILAppModel>, ILAppModel
    {
        public ILAppDataService DataService { get; private set; }
        public ILAppConfigRepository LAppConfigRepository { get; private set; }
        public IConfigurationDataService ConfigurationDataService { get; private set; }
        public IDataPath DataPath { get; private set; }

        public ReturnStateActionEnum ReturnStateAction { get; set; }

        public List<Warehouse> Warehouses { get; set; }
        public Warehouse CurrentWarehouse { get; set; }

        public List<Zone> Zones { get; set; }
        public Zone CurrentZone { get; set; }

        public List<int> Orders { get; set; } = new List<int>();
        public int? CurrentOrder { get; set; }

        public int CurrentTransactionId { get; set; }

        public PickRoute CurrentPickRoute { get; set; }

        public string CurrentLicensePlateName { get; set; }
        public int CurrentLicensePlateId { get; set; }

        public string CurrentBatchNumber { get; set; }
        public string EnteredSerialNumber { get; set; }
        public string SelectedBatchNumber { get; set; }

        public string LocationResponse { get; set; }
        public string ProductCheckDigitResponse { get; set; }

        public string EnteredQuantityString { get; set; }
        public int EnteredQuantity { get; set; }

        public List<string> ValidSerialNumbers { get; set; }
        public List<string> ValidBatchNumbers { get; set; }

        public DateTime CurrentLicensePlateLastVisitDate { get; set; }

        public Dictionary<DamageTypes, DamageSeverities> Damages { get; set; } = new Dictionary<DamageTypes, DamageSeverities>();
        public List<DamageTypes> SpecifiedDamages { get; set; } = new List<DamageTypes>();

        /// <summary>
        /// Returns Photo capture intent result. This is a list of file paths captured or selected by the user
        /// </summary>
        public List<Uri> FilePathList { get; set; }

        /// <summary>
        /// Returns whether or not the product is damaged
        /// </summary>
        public bool? CheckProductDamagedResponse { get; set; }

        /// <summary>
        /// Returns whether or not there are more items to handle.
        /// </summary>
        public bool MoreWorkItems => CurrentPickRoute != null;

        /// <summary>
        /// Returns the total step count for the progress bar
        /// </summary>
        public int TotalStepCount { get; set; }

        /// <summary>
        /// Returns the current step count for the progress bar
        /// </summary>
        public int CurrentStepCount { get; set; }

        /// <summary>
        /// The the type of container that this license plate belongs to
        /// </summary>
        public VocabWordInfo LicensePlateContainerType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LApp.LAppModel"/> class.
        /// </summary>
        /// <param LAppModel="dataProxy">Data proxy.</param>
        public LAppModel(ILAppDataService lappDataService,
            ILAppConfigRepository lappConfigRepository,
            IConfigurationDataService configurationDataService, IDataPath dataPath)
        {
            DataService = lappDataService;
            LAppConfigRepository = lappConfigRepository;
            DataPath = dataPath;
            //Register repositories for application to allow for external configuration
            ConfigurationDataService = configurationDataService;
        }

        public override Task InitializeWorkflowAsync()
        {
            DataService.Initialize();
            return base.InitializeWorkflowAsync();
        }

        public void ResetOperator()
        {
            CurrentOperator = null;
            ResetWarehouse();
        }

        public void ResetWarehouse()
        {
            CurrentWarehouse = null;
        }

        public void DeletePhotos()
        {
            if (System.IO.Directory.Exists(DataPath.PhotoPath))
            {
                System.IO.Directory.Delete(DataPath.PhotoPath, true);
            }
        }
    }
    #endregion

    public class LAppBusinessLogic : SimplifiedBaseBusinessLogic<ILAppModel, LAppBusinessLogic, LAppConfigRepository>
    {
        //Sign on states
        public static readonly CoreAppSMState SignOnState = new CoreAppSMState(nameof(SignOnState));

        //Warehouse states
        public static readonly CoreAppSMState GetWarehouseState = new CoreAppSMState(nameof(GetWarehouseState));
        public static readonly CoreAppSMState DisplayWarehouseState = new CoreAppSMState(nameof(DisplayWarehouseState));
        public static readonly CoreAppSMState ProcessWarehouseState = new CoreAppSMState(nameof(ProcessWarehouseState));

        //Zone states
        public static readonly CoreAppSMState GetZonesState = new CoreAppSMState(nameof(GetZonesState));
        public static readonly CoreAppSMState DisplayZonesState = new CoreAppSMState(nameof(DisplayZonesState));
        public static readonly CoreAppSMState ProcessZonesState = new CoreAppSMState(nameof(ProcessZonesState));

        //Order picking state machine
        public static readonly CoreAppSMState StartPickOrderState = new CoreAppSMState(nameof(StartPickOrderState));


        private LAppGetOrderLogic _GetOrderStateMachine;
        private LAppGetOrderLogic GetOrderStateMachine { get { return Manager.CreateStateMachine(ref _GetOrderStateMachine); } }

        private readonly ILog _Log = LogManager.GetLogger(nameof(LAppBusinessLogic));

        public override bool IsPrimaryStateMachine => true;

        public LAppBusinessLogic(SimplifiedStateMachineManager<LAppBusinessLogic, ILAppModel> manager, ILAppModel model) : base(manager, model)
        {
        }

        public override void ConfigureStates()
        {
            //------------------------------------------------
            //Sign On
            ConfigureLogin(SignOnState, GetWarehouseState, LAppConfigRepository.OperatorId, ExecuteSignOnAsync);

            //------------------------------------------------
            //Get and Select warehouse
            ConfigureLogicState(GetWarehouseState, async () =>
            {
                try
                {
                    CurrentBackGroundActivityHeader = Translate.GetLocalizedTextForKey("LApp_BackgroundActivity_Header_RetrieveWarehouses");
                    Model.Warehouses = await Model.DataService.GetWarehouses();
                    NextState = DisplayWarehouseState;
                }
                catch (Exception)
                {
                    NextState = GetWarehouseState;
                }
            }, DisplayWarehouseState, GetWarehouseState);

            ConfigureDisplayState(DisplayWarehouseState, 
                                  ProcessWarehouseState,
                                  SignOnState,
                                  followedByBackgroundActivity: true,
                                  backgroundActivityHeaderKey: "LApp_BackgroundActivity_Header_RetrieveZones",
                                  encodeAction: EncodeSelectWarehouse, 
                                  decodeAction: DecodeSelectWareHouse);
                
            ConfigureLogicState(ProcessWarehouseState, () =>
            {
                if (Model.CurrentWarehouse == null)
                {
                    NextState = SignOnState;
                }
                else
                {
                    NextState = GetZonesState;
                }
            }, SignOnState, GetZonesState);

            //------------------------------------------------
            //Get and select Zones
            ConfigureLogicState(GetZonesState, async () =>
            {
                try
                {
                    Model.Zones = await Model.DataService.GetZones();
                    NextState = DisplayZonesState;
                }
                catch (OperationCanceledException)
                {
                    _Log.Debug("Operation canceled via REST Timeout Handler");
                    Model.CurrentUserMessage = Translate.GetLocalizedTextForKey("LApp_NetworkTimeout_Error");
                    NextState = DisplayWarehouseState;
                }
                catch (Exception)
                {
                    NextState = GetZonesState;
                }
            }, DisplayZonesState, DisplayWarehouseState, GetZonesState);

            ConfigureDisplayState(DisplayZonesState, 
                                  ProcessZonesState,
                                  GetWarehouseState, 
                                  followedByBackgroundActivity: true,
                                  backgroundActivityHeaderKey: "LApp_BackgroundActivity_Header_RetrieveOrders",
                                  encodeAction: EncodeSelectZone, 
                                  decodeAction: DecodeSelectZone);
            
            ConfigureLogicState(ProcessZonesState, () =>
            {
                if (Model.CurrentZone == null)
                {
                    NextState = GetWarehouseState;
                }
                else
                {
                    NextState = StartPickOrderState;
                }

            }, StartPickOrderState, GetWarehouseState);

            //------------------------------------------------
            //Start Pick Order State Machine
            ConfigureLogicState(StartPickOrderState, async () =>
            {
                GetOrderStateMachine.Reset();
                await GetOrderStateMachine.InitializeStateMachineAsync();

                NextState = GetWarehouseState;
            }, GetWarehouseState);
        }


        #region BusinessLogicMethods
        private async Task<bool> ExecuteSignOnAsync(global::GuidedWorkRunner.Operator newOperator)
        {
            if (newOperator.OperatorIdentifier == null || newOperator.Password == null)
            {
                CurrentUserMessage = Translate.GetLocalizedTextForKey("LApp_SignOn_Failed_Required");
                return false;
            }

            if (Model.CurrentTransactionId != 0)
            {
                await Model.DataService.CloseTransactionAsync(Model.CurrentTransactionId);
                Model.CurrentTransactionId = 0;
            }

            // Sign on
            bool signOnResponse = await Model.DataService.ValidateCredentialsAsync(newOperator.OperatorIdentifier, newOperator.Password);
            if (signOnResponse)
            {
                Model.CurrentTransactionId = await Model.DataService.OpenTransactionAsync();
            }
            else
            {
                CurrentUserMessage = Translate.GetLocalizedTextForKey("LApp_SignOn_Authentication_Failed");
                return false;
            }
            return true;
        }

        #endregion

        #region EncodersDecoders
        private WorkflowObjectContainer EncodeSelectWarehouse(ILAppModel model)
        {

            var wfoContainer = new WorkflowObjectContainer();
            WorkflowObject wfo = null;
            if (model.Warehouses.Count > 1)
            {
                var warehouseStrings = model.Warehouses.ConvertAll(w => w.Name);
                wfo = WorkflowObjectFactory.CreateMenuItemsIntent(Translate.GetLocalizedTextForKey("LApp_SelectWarehouse_Header"),
                                                                  "selectedWarehouse",
                                                                  Translate.GetLocalizedTextForKey("LApp_SelectWarehouse_Prompt"),
                                                                  warehouseStrings,
                                                                  model.CurrentUserMessage,
                                                                  initialPrompt: model.CurrentUserMessage);
                wfo.MenuItemsProperties.DisplayKeys = true;
                wfo.MenuItemsProperties.EchoLastSelectedItem = true;
                wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(DefaultModuleVocab.VocabCancel));
                wfo.MenuItemsProperties.ReturnValueIfNoOptionsSelected = DefaultModuleVocab.VocabCancel.IdentificationKey;
            }
            else if (model.Warehouses.Count == 1)
            {
                wfo = WorkflowObjectFactory.CreateReadyUIElementIntent(Translate.GetLocalizedTextForKey("LApp_SelectWarehouseSingle_Header"),
                                                                       "readyOne",
                                                                       Translate.GetLocalizedTextForKey("LApp_SelectWarehouseSingle_Prompt", model.Warehouses[0].Name),
                                                                       GenerateWarehouseDisplayElements(model.Warehouses),
                                                                       model.CurrentUserMessage,
                                                                       initialPrompt: model.CurrentUserMessage);
            }

            // Make sure we got at least one Warehouse
            if (wfo != null)
            {
                wfo.MessageType = model.MessageType;
                wfoContainer.Add(wfo);
            }
            return wfoContainer;
        }

        private List<UIElement> GenerateWarehouseDisplayElements(List<Warehouse> warehouses)
        {
            return new List<UIElement>
            {
                new UIElement
                {
                    ElementType = UIElementType.Banner,
                    Label = Translate.GetLocalizedTextForKey("LApp_WarehouseDetails_WarehouseBanner_Label")
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = warehouses[0].Name,
                    Bold = true,
                    Centered = true
                },
            };
        }

        private void DecodeSelectWareHouse(SlotContainer slotContainer, ILAppModel model)
        {
            string selectedWarehouseName = null;
            foreach (var slot in slotContainer.Slots)
            {
                if (slot.Intent == WorkflowIntent.MenuItem)
                {
                    selectedWarehouseName = slot.ExtraData["Button"];
                }
                if (slot.Intent == WorkflowIntent.ReadyPrompt)
                {
                    selectedWarehouseName = model.Warehouses[0].Name;
                }
            }
            model.CurrentWarehouse = model.Warehouses.FirstOrDefault(w => w.Name == selectedWarehouseName);
        }

        private WorkflowObjectContainer EncodeSelectZone(ILAppModel model)
        {

            var wfoContainer = new WorkflowObjectContainer();
            WorkflowObject wfo = null;
            if (model.Zones.Count > 1)
            {
                var zoneStrings = model.Zones.ConvertAll(z => z.Name);
                wfo = WorkflowObjectFactory.CreateMenuItemsIntent(Translate.GetLocalizedTextForKey("LApp_SelectZone_Header"),
                                                                  "selectedZone",
                                                                  Translate.GetLocalizedTextForKey("LApp_SelectZone_Prompt"),
                                                                  zoneStrings,
                                                                  model.CurrentUserMessage,
                                                                  initialPrompt: model.CurrentUserMessage);
                wfo.MenuItemsProperties.EchoLastSelectedItem = true;
                wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(DefaultModuleVocab.VocabCancel));
                wfo.MenuItemsProperties.ReturnValueIfNoOptionsSelected = DefaultModuleVocab.VocabCancel.IdentificationKey;
            }
            else if (model.Zones.Count == 1)
            {
                wfo = WorkflowObjectFactory.CreateReadyUIElementIntent(Translate.GetLocalizedTextForKey("LApp_SelectZoneSingle_Header"),
                                                                       "readyOne",
                                                                       Translate.GetLocalizedTextForKey("LApp_SelectZoneSingle_Prompt", model.Zones[0].Name),
                                                                       GenerateZoneDisplayElements(model.Zones),
                                                                       model.CurrentUserMessage,
                                                                       initialPrompt: model.CurrentUserMessage);
            }

            // Make sure we got at least one Zone
            if (wfo != null)
            {
                wfo.MessageType = model.MessageType;
                wfoContainer.Add(wfo);
            }
            return wfoContainer;
        }

        private List<UIElement> GenerateZoneDisplayElements(List<Zone> zones)
        {
            return new List<UIElement>
            {
                new UIElement
                {
                    ElementType = UIElementType.Banner,
                    Label = Translate.GetLocalizedTextForKey("LApp_ZoneDetails_ZoneBanner_Label")
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = zones[0].Name,
                    Bold = true,
                    Centered = true
                },
            };
        }

        private void DecodeSelectZone(SlotContainer slotContainer, ILAppModel model)
        {
            string selectedZoneName = null;

            foreach (var slot in slotContainer.Slots)
            {
                if (slot.Intent == WorkflowIntent.MenuItem)
                {
                    selectedZoneName = slot.ExtraData["Button"];
                }
                if (slot.Intent == WorkflowIntent.ReadyPrompt)
                {
                    selectedZoneName = model.Zones[0].Name;
                }
            }
            model.CurrentZone = model.Zones.FirstOrDefault(z => z.Name == selectedZoneName);
        }

        #endregion
    }
}
