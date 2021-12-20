//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp
{
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using System;
    using System.Collections.Generic;
    using Honeywell.Firebird.CoreLibrary;
    using System.Threading.Tasks;

    /// <summary>
    /// Class that contains logic for selecting a work order and 
    /// picking it
    /// </summary>
    public class LAppGetOrderLogic : SimplifiedBaseBusinessLogic<ILAppModel, LAppBusinessLogic, LAppConfigRepository>
    {

        //States
        //Get and Select Order
        public static readonly CoreAppSMState GetOrderState = new CoreAppSMState(nameof(GetOrderState));
        public static readonly CoreAppSMState DisplayOrderState = new CoreAppSMState(nameof(DisplayOrderState));
        public static readonly CoreAppSMState ProcessOrderState = new CoreAppSMState(nameof(ProcessOrderState));

        //Retrieve Pick Route/License plate
        public static readonly CoreAppSMState GetPickRoute = new CoreAppSMState(nameof(GetPickRoute));
        public static readonly CoreAppSMState DisplayLicensePlate = new CoreAppSMState(nameof(DisplayLicensePlate));
        public static readonly CoreAppSMState ProcessLicensePlate = new CoreAppSMState(nameof(ProcessLicensePlate));
        public static readonly CoreAppSMState DisplayLicenseDate = new CoreAppSMState(nameof(DisplayLicenseDate));
        public static readonly CoreAppSMState GetLicensePlate = new CoreAppSMState(nameof(GetLicensePlate));

        //Start Pick License
        public static readonly CoreAppSMState StartPickOrderLogic = new CoreAppSMState(nameof(StartPickOrderLogic));

        private LAppPickOrderLogic _PickOrderStateMachine;
        private LAppPickOrderLogic PickOrderStateMachine { get { return Manager.CreateStateMachine(ref _PickOrderStateMachine); } }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="manager">State machine manager</param>
        /// <param name="model">Application model class</param>
        public LAppGetOrderLogic(SimplifiedStateMachineManager<LAppBusinessLogic, ILAppModel> manager, ILAppModel model) : base(manager, model)
        {
        }

        public override void ConfigureStates()
        {
            //------------------------------------------------
            //Get and select Order
            ConfigureReturnLogicState(GetOrderState, async () =>
            {
                try
                {
                    Model.Orders = await Model.DataService.GetOrdersAsync(Model.CurrentWarehouse.Id, Model.CurrentZone.Id);
                    //Not setting NextState if not orders received, this will cause state machine to return to calling state machine
                    if (Model.Orders.Count > 0)
                    {
                        NextState = DisplayOrderState;
                    }
                }
                catch (Exception)
                {
                    NextState = GetOrderState; //Retry state
                }

            }, DisplayOrderState, GetOrderState);

            ConfigureDisplayState(DisplayOrderState, 
                                  ProcessOrderState,
                                  CoreAppStates.Standby,
                                  () =>
                                  {
                                      Manager.ExecuteGoToStateAsync(LAppBusinessLogic.DisplayZonesState);
                                  },
                                  followedByBackgroundActivity: true,
                                  backgroundActivityHeaderKey: "LApp_BackgroundActivity_Header_RetrievePickRoute",
                                  encodeAction: EncodeSelectOrder, 
                                  decodeAction: DecodeSelectOrder);
            
            ConfigureReturnLogicState(ProcessOrderState, () =>
            {
                NextState = GetPickRoute;
                if (Model.CurrentOrder == null)
                {
                    Manager.ExecuteGoToStateAsync(LAppBusinessLogic.DisplayZonesState);
                }
            }, GetPickRoute);

            //------------------------------------------------
            //Get Pick route data
            ConfigureLogicState(GetPickRoute, async () =>
            {
                try
                {
                    Model.CurrentPickRoute = await Model.DataService.GetPickTasksAsync(Model.CurrentWarehouse.Id, Model.CurrentZone.Id, (int)Model.CurrentOrder);
                    NextState = DisplayLicensePlate;
                }
                catch (Exception)
                {
                    Manager.ExecuteGoToStateAsync(LAppBusinessLogic.SignOnState);
                }
            }, DisplayLicensePlate);

            ConfigureDisplayState(DisplayLicensePlate, ProcessLicensePlate, GetOrderState, encodeAction: EncodeEnterLicensePlate, decodeAction: DecodeLicensePlate);
            ConfigureReturnLogicState(ProcessLicensePlate, () =>
            {
                if (Model.CurrentLicensePlateName == DefaultModuleVocab.VocabCancel.IdentificationKey)
                {
                    NextState = GetOrderState;
                }
                else
                {
                    NextState = DisplayLicenseDate;
                }

            }, GetOrderState, DisplayLicenseDate);

            ConfigureDisplayState(DisplayLicenseDate, 
                                  GetLicensePlate,
                                  DisplayLicensePlate,
                                  followedByBackgroundActivity: true,
                                  backgroundActivityHeaderKey: "LApp_BackgroundActivity_Header_RetrieveLicensePlateId",
                                  encodeAction: EncodeEnterLicenseDate, 
                                  decodeAction: DecodeLicesnseDate);

            ConfigureLogicState(GetLicensePlate, async () =>
            {
                try
                {
                    Model.CurrentLicensePlateId = await Model.DataService.GetLicensePlateId(Model.CurrentLicensePlateName);
                    NextState = StartPickOrderLogic;
                }
                catch (Exception)
                {
                    NextState = DisplayLicensePlate;
                }
            }, DisplayLicensePlate, StartPickOrderLogic);

            //Start pick order state machine
            ConfigureLogicState(StartPickOrderLogic, async () => 
            {
                PickOrderStateMachine.Reset();
                await PickOrderStateMachine.InitializeStateMachineAsync();

                NextState = GetOrderState;
            }, GetOrderState);
        }

        #region EncodersDecoders
        private WorkflowObjectContainer EncodeSelectOrder(ILAppModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            WorkflowObject wfo = null;
            if (model.Orders.Count > 1)
            {
                // Don't pass orders to CreateMenuItemsIntent() as we need to add specific keys and items to the menu
                var orderStrings = model.Orders.ConvertAll(o => o.ToString());
                wfo = WorkflowObjectFactory.CreateMenuItemsIntent(Translate.GetLocalizedTextForKey("LApp_SelectOrder_Header"),
                                                                  "selectedOrder",
                                                                  Translate.GetLocalizedTextForKey("LApp_SelectOrder_Prompt"),
                                                                  message: model.CurrentUserMessage,
                                                                  initialPrompt: model.CurrentUserMessage);
                wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(DefaultModuleVocab.VocabCancel));
                wfo.MenuItemsProperties.ReturnValueIfNoOptionsSelected = DefaultModuleVocab.VocabCancel.IdentificationKey;

                // Add the order number as the key and display value in the menu
                foreach (var order in model.Orders)
                {
                    string orderString = order.ToString();
                    wfo.MenuItemsProperties.AddMenuItem(orderString, key: orderString);
                }
            }
            else if (model.Orders.Count == 1)
            {
                wfo = WorkflowObjectFactory.CreateReadyIntent(Translate.GetLocalizedTextForKey("LApp_SelectOrderSingle_Header"),
                                                              "readyOne",
                                                              Translate.GetLocalizedTextForKey("LApp_SelectOrderSingle_Prompt", model.Orders[0].ToString()),
                                                              model.CurrentUserMessage,
                                                              initialPrompt: model.CurrentUserMessage);
            }

            if (wfo != null)
            {
                wfo.MessageType = model.MessageType;
                wfoContainer.Add(wfo);
            }
            return wfoContainer;
        }

        private void DecodeSelectOrder(SlotContainer slotContainer, ILAppModel model)
        {
            model.CurrentOrder = null;
            foreach (var slot in slotContainer.Slots)
            {
                if (slot.Intent == WorkflowIntent.MenuItem)
                {
                    if (int.TryParse(slot.ExtraData["Button"], out int orderNumber))
                    {
                        model.CurrentOrder = orderNumber;
                    }
                }
            }
        }

        private WorkflowObjectContainer EncodeEnterLicensePlate(ILAppModel model)
        {
            var uiElements = GenerateLicensePlateDisplayElements(model.CurrentPickRoute);

            var anchorWords = new List<VocabWordInfo>
            {
                LAppModuleVocab.Pallet,
                LAppModuleVocab.Tote,
                LAppModuleVocab.Box
            };

            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateGetLongValueIntent(Translate.GetLocalizedTextForKey("LApp_LicensePlate_Header"),
                                                                     "enterLicensePlate",
                                                                     Translate.GetLocalizedTextForKey("LApp_LicensePlate_Label"),
                                                                     Translate.GetLocalizedTextForKey("LApp_LicensePlate_Prompt", anchorWords[0].SpokenWord, anchorWords[1].SpokenWord, anchorWords[2].SpokenWord),
                                                                     uiElements,
                                                                     model.CurrentUserMessage,
                                                                     initialPrompt: model.CurrentUserMessage);

            wfo.LongValueProperties.AllowedCharacters = CharacterSet.AlphaNumeric;
            wfo.LongValueProperties.AnchorWords = new HashSet<VocabWordInfo>(anchorWords);

            wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(LAppModuleVocab.ChangeWareHouse,
                confirm: true, processActionAsync: () => { return Task.FromResult(LAppBusinessLogic.GetWarehouseState); }));
            wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(DefaultModuleVocab.VocabCancel));

            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private List<UIElement> GenerateLicensePlateDisplayElements(PickRoute pickRoute)
        {
            return new List<UIElement>
            {
                new UIElement
                {
                    ElementType = UIElementType.Banner,
                    Label = Translate.GetLocalizedTextForKey("LApp_LicensePlateDetails_LicensePlateBanner_Label")
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Order# ",
                    Value = pickRoute.OrderId.ToString(),
                    ValueInlineWithLabel = true,
                    Bold = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Zone ",
                    Value = "1",
                    ValueInlineWithLabel = true,
                    Bold = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = Translate.GetLocalizedTextForKey("LApp_LicensePlate_Note_Label"),
                    Value = Translate.GetLocalizedTextForKey("LApp_LicensePlate_Note_Value"),
                },
            };
        }

        private void DecodeLicensePlate(SlotContainer slotContainer, ILAppModel model)
        {
            model.CurrentLicensePlateName = GenericBaseEncoder<ILAppModel>.DecodeValueEntry(slotContainer);

            var anchor = GenericBaseEncoder<ILAppModel>.DecodeAnchorWord(slotContainer);
            if (anchor != null)
            {
                model.LicensePlateContainerType = anchor;
            }
        }

        private WorkflowObjectContainer EncodeEnterLicenseDate(ILAppModel model)
        {
            DateTime outputDate = new DateTime(2000, 1, 1);
            DateTime today = DateTime.Today;
            DateTime? minDate = outputDate;
            DateTime? maxDate = today;

            model.CurrentUserMessage = model.LicensePlateContainerType?.DisplayedWord ?? string.Empty;
            model.MessageType = UserMessageType.Standard;

            var uiElements = GenerateLicensePlateGetDateDisplayElements(minDate, maxDate, WorkflowObjectFactory.DateEntryFormat);

            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateDateEntryIntent(Translate.GetLocalizedTextForKey("LApp_LicenseLastDate_Header"),
                                                                  "enterLicensePlateLastInspectionDate",
                                                                  Translate.GetLocalizedTextForKey("LApp_LicenseLastDate_Label"),
                                                                  Translate.GetLocalizedTextForKey("LApp_LicenseLastDate_Prompt"),
                                                                  model.CurrentUserMessage,
                                                                  uiElements: uiElements,
                                                                  initialPrompt: model.CurrentUserMessage);

            wfo.DateProperties.MinimumDate = minDate;
            wfo.DateProperties.MaximumDate = maxDate;
            wfo.DateProperties.PreSetDateValue = today;

            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private List<UIElement> GenerateLicensePlateGetDateDisplayElements(DateTime? minDate, DateTime? maxDate, string dateFormat)
        {
            List<UIElement> uiElements = new List<UIElement>(){
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = Translate.GetLocalizedTextForKey("LApp_LicenseLastDate_DateFormat_Label"),
                    Value = dateFormat,
                    ValueInlineWithLabel = true
                }
            };
            if (minDate != null)
            {
                uiElements.Add(new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = Translate.GetLocalizedTextForKey("LApp_LicenseLastDate_MinDate_Label"),
                    Value = minDate.Value.ToString(dateFormat),
                    ValueInlineWithLabel = true
                });
            }
            if (maxDate != null)
            {
                uiElements.Add(new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = Translate.GetLocalizedTextForKey("LApp_LicenseLastDate_MaxDate_Label"),
                    Value = maxDate.Value.ToString(dateFormat),
                    ValueInlineWithLabel = true
                });
            }

            return uiElements;
        }

        private void DecodeLicesnseDate(SlotContainer slotContainer, ILAppModel model)
        {
            model.CurrentLicensePlateLastVisitDate = GenericBaseEncoder<ILAppModel>.DecodeDateEntry(slotContainer);
        }

        #endregion
    }
}
