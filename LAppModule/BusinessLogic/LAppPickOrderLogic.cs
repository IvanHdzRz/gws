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
    using System.Linq;
    using LiquidState;

    public class LAppPickOrderLogic : SimplifiedBaseBusinessLogic<ILAppModel, LAppBusinessLogic, LAppConfigRepository>
    {

        public static readonly CoreAppSMState CheckMoreWork = new CoreAppSMState(nameof(CheckMoreWork));

        public static readonly CoreAppSMState DisplayLocationState = new CoreAppSMState(nameof(DisplayLocationState));
        public static readonly CoreAppSMState ProcessLocationState = new CoreAppSMState(nameof(ProcessLocationState));

        public static readonly CoreAppSMState DisplayProductState = new CoreAppSMState(nameof(DisplayProductState));
        public static readonly CoreAppSMState ProcessProductState = new CoreAppSMState(nameof(ProcessProductState));

        public static readonly CoreAppSMState DisplayCheckDamageState = new CoreAppSMState(nameof(DisplayCheckDamageState));
        public static readonly CoreAppSMState ProcessCheckDamageState = new CoreAppSMState(nameof(ProcessCheckDamageState));
        public static readonly CoreAppSMState DisplayEnquireDamageState = new CoreAppSMState(nameof(DisplayEnquireDamageState));
        public static readonly CoreAppSMState ProcessEnquireDamageState = new CoreAppSMState(nameof(ProcessEnquireDamageState));
        public static readonly CoreAppSMState DisplayEnquireSeverityState = new CoreAppSMState(nameof(DisplayEnquireSeverityState));
        public static readonly CoreAppSMState ProcessEnquireSeverityState = new CoreAppSMState(nameof(ProcessEnquireSeverityState));
        public static readonly CoreAppSMState DisplayTakePhotoState = new CoreAppSMState(nameof(DisplayTakePhotoState));
        public static readonly CoreAppSMState ProcessTakePhotoState = new CoreAppSMState(nameof(ProcessTakePhotoState));

        public static readonly CoreAppSMState DeterminePickMethodState = new CoreAppSMState(nameof(DeterminePickMethodState));

        public static readonly CoreAppSMState GetSerialNumberState = new CoreAppSMState(nameof(GetSerialNumberState));
        public static readonly CoreAppSMState DisplaySerialNumberState = new CoreAppSMState(nameof(DisplaySerialNumberState));
        public static readonly CoreAppSMState ProcessSerialNumberState = new CoreAppSMState(nameof(ProcessSerialNumberState));

        public static readonly CoreAppSMState GetBatchNumberState = new CoreAppSMState(nameof(GetBatchNumberState));
        public static readonly CoreAppSMState DisplayBatchNumberState = new CoreAppSMState(nameof(DisplayBatchNumberState));
        public static readonly CoreAppSMState ProcessBatchNumberState = new CoreAppSMState(nameof(ProcessBatchNumberState));

        public static readonly CoreAppSMState DisplayQuanityState = new CoreAppSMState(nameof(DisplayQuanityState));
        public static readonly CoreAppSMState ProcessQuantityState = new CoreAppSMState(nameof(ProcessQuantityState));


        public static readonly CoreAppSMState GetNextPickRoute = new CoreAppSMState(nameof(GetNextPickRoute));

        public static readonly CoreAppSMState DisplayAssignmentComplete = new CoreAppSMState(nameof(DisplayAssignmentComplete));
        public static readonly CoreAppSMState ProcessAssignmentComplete = new CoreAppSMState(nameof(ProcessAssignmentComplete));

        public LAppPickOrderLogic(SimplifiedStateMachineManager<LAppBusinessLogic, ILAppModel> manager, ILAppModel model) : base(manager, model)
        {
        }

        public override void ConfigureStates()
        {
            //------------------------------------------------
            //Check if more picks in order
            ConfigureLogicState(CheckMoreWork, () =>
            {
                NextState = DisplayAssignmentComplete;
                if (Model.MoreWorkItems)
                {
                    NextState = DisplayLocationState;
                }
            }, DisplayAssignmentComplete, DisplayLocationState);

            //------------------------------------------------
            //Location
            ConfigureDisplayState(DisplayLocationState, ProcessLocationState, CoreAppStates.Standby, () =>
            {
                Manager.ExecuteGoToStateAsync(LAppGetOrderLogic.DisplayLicenseDate);
            }, encodeAction: EncodeDisplayLocation, decodeAction: DecodeDisplayLocation);

            ConfigureLogicState(ProcessLocationState, () =>
            {
                NextState = DisplayProductState;
            }, DisplayProductState);

            //------------------------------------------------
            //Product
            ConfigureDisplayState(DisplayProductState, ProcessProductState, DisplayLocationState,
                encodeAction: EncodeDisplayProduct, decodeAction: DecodeDisplayProduct);

            ConfigureLogicState(ProcessProductState, () =>
            {
                NextState = DisplayCheckDamageState;
                if (Model.ProductCheckDigitResponse == DefaultModuleVocab.VocabCancel.IdentificationKey)
                {
                    NextState = DisplayLocationState;
                }

            }, DisplayCheckDamageState, DisplayProductState, DisplayLocationState);

            //------------------------------------------------
            //Check Damage
            ConfigureDisplayState(DisplayCheckDamageState, ProcessCheckDamageState, DisplayLocationState,
                encodeAction: EncodeDisplayCheckDamage, decodeAction: DecodeDisplayCheckDamage);
            
            ConfigureLogicState(ProcessCheckDamageState, () =>
            {
                NextState = DeterminePickMethodState; //TODO: Change this to additional states when added
                if (Model.CheckProductDamagedResponse == true)
                {
                    NextState = DisplayEnquireDamageState;
                }

            }, DeterminePickMethodState, DisplayEnquireDamageState);

            ConfigureDisplayState(DisplayEnquireDamageState, ProcessEnquireDamageState, DisplayCheckDamageState,
                encodeAction: EncodeEnquireDamage, decodeAction: DecodeEnquireDamage);

            ConfigureLogicState(ProcessEnquireDamageState, () =>
            {
                NextState = DisplayEnquireSeverityState;
                if (Model.SpecifiedDamages.Count <= 0)
                {
                    CurrentUserMessage = Translate.GetLocalizedTextForKey("LApp_NoDamageConfirmation");
                    NextState = DisplayCheckDamageState;
                }
            }, DisplayEnquireSeverityState, DisplayCheckDamageState);

            ConfigureDisplayState(DisplayEnquireSeverityState, ProcessEnquireSeverityState, DisplayEnquireDamageState,
                encodeAction: EncodeEnquireSeverity, decodeAction: DecodeEnquireSeverity);

            ConfigureLogicState(ProcessEnquireSeverityState, () =>
            {
                NextState = DisplayTakePhotoState;
                if (Model.SpecifiedDamages.Count > 0)
                {
                    NextState = DisplayEnquireSeverityState;
                }
            }, DisplayEnquireSeverityState, DisplayTakePhotoState);

            ConfigureDisplayState(DisplayTakePhotoState, 
                                  ProcessTakePhotoState,
                                  DisplayEnquireDamageState,
                                  followedByBackgroundActivity: true,
                                  backgroundActivityHeaderKey: "LApp_BackgroundActivity_Header_TransmitPhotos",
                                  encodeAction: EncodeTakePhotoOfDamage, 
                                  decodeAction: DecodeTakePhotoOfDamage);

            ConfigureLogicState(ProcessTakePhotoState, () =>
            {
                try
                {
                    for (int i = 0; i < Model.FilePathList.Count; i++)
                    {
                        Model.DataService.SendPhotoAsync(Model.CurrentTransactionId, Model.FilePathList[i].AbsolutePath);
                    }
                    NextState = GetNextPickRoute;
                    // Increment step count because product will have been confirmed
                    Model.CurrentStepCount += 1;
                }
                catch (Exception)
                {
                    NextState = DisplayTakePhotoState;
                }
            }, DisplayTakePhotoState, GetNextPickRoute);

            //Determine how to pick quantity, enter quantity, entering batch, or entering serial number
            //For batch and serial number sample how to start background state from logic state
            ConfigureLogicState(DeterminePickMethodState, () =>
            {
                Model.SelectedBatchNumber = string.Empty;
                Model.EnteredSerialNumber = string.Empty;
                NextState = DisplayQuanityState;

                if (Model.CurrentPickRoute.SerialNumbers.Count > 0)
                {
                    backgroundActivityNextState = GetSerialNumberState;
                    CurrentBackGroundActivityHeader = Translate.GetLocalizedTextForKey("LApp_BackgroundActivity_Header_RetrieveSerialNumbers");
                    NextState = CoreAppStates.BackgroundActvity;
                }
                else if (Model.CurrentPickRoute.BatchNumbers.Count > 0)
                {
                    backgroundActivityNextState = GetBatchNumberState;
                    CurrentBackGroundActivityHeader = Translate.GetLocalizedTextForKey("LApp_BackgroundActivity_Header_RetrieveBatchNumbers");
                    NextState = CoreAppStates.BackgroundActvity;
                }
            }, DisplayQuanityState, CoreAppStates.BackgroundActvity);

            //------------------------------------------------
            //Serial Number
            ConfigureLogicState(GetSerialNumberState, async () =>
            {
                try
                {
                    Model.ValidSerialNumbers = await Model.DataService.GetSerialNumbersAsync(Model.CurrentPickRoute.LocationId, Model.CurrentPickRoute.ProductId);
                    NextState = DisplaySerialNumberState;
                }
                catch (Exception)
                {
                    Manager.ExecuteGoToStateAsync(LAppBusinessLogic.SignOnState);
                }
            }, DisplaySerialNumberState);

            ConfigureDisplayState(DisplaySerialNumberState, 
                                  ProcessSerialNumberState,
                                  DisplayCheckDamageState,
                                  followedByBackgroundActivity: true,
                                  backgroundActivityHeaderKey: "LApp_BackgroundActivity_Header_TransmitPickSerial",
                                  encodeAction: EncodeSerialNumber, 
                                  decodeAction: DecodeSerialNumber);

            ConfigureLogicState(ProcessSerialNumberState, () =>
            {
                if (Model.EnteredSerialNumber == DefaultModuleVocab.VocabCancel.IdentificationKey)
                {
                    NextState = DisplayCheckDamageState;
                }
                else
                {
                    foreach (var serialNumber in Model.ValidSerialNumbers)
                    {
                        if (Model.EnteredSerialNumber == serialNumber)
                        {
                            try
                            {
                                Model.DataService.ConfirmPickTasksSerialAsync(Model.CurrentLicensePlateId, Model.CurrentTransactionId, Model.CurrentPickRoute.OrderId, Model.CurrentPickRoute.LineId, Model.EnteredSerialNumber);
                                // Increment step count because product will have been confirmed
                                Model.CurrentStepCount += 1;
                                NextState = GetNextPickRoute;
                                return;
                            }
                            catch
                            {
                                Manager.ExecuteGoToStateAsync(LAppBusinessLogic.SignOnState);
                            }
                        }
                    }

                    // In the encoder, we are conditionally setting the voice enabled flag based upon whether the runner requires voice (RunnerRequiresVoiceEnabled).
                    // Upon a RunnerRequiresVoiceEnabled slot response, EnteredSerialNumber number will be null.
                    if (!string.IsNullOrEmpty(Model.EnteredSerialNumber))
                    {
                        CurrentUserMessage = Translate.GetLocalizedTextForKey("LApp_EnterSerialNumber_Wrong");
                    }
                    NextState = DisplaySerialNumberState;
                }
            }, DisplayCheckDamageState, DisplaySerialNumberState, GetNextPickRoute);


            //------------------------------------------------
            //Batch Number
            ConfigureLogicState(GetBatchNumberState, async () =>
            {
                try
                {
                    Model.ValidBatchNumbers = await Model.DataService.GetBatchNumbersAsync(Model.CurrentPickRoute.LocationId, Model.CurrentPickRoute.ProductId);
                    NextState = DisplayBatchNumberState;
                }
                catch (Exception)
                {
                    Manager.ExecuteGoToStateAsync(LAppBusinessLogic.SignOnState);
                }
            }, DisplayBatchNumberState);

            ConfigureDisplayState(DisplayBatchNumberState, ProcessBatchNumberState, DisplayCheckDamageState,
                encodeAction: EncodeBatchNumber, decodeAction: DecodeBatchNumber);

            ConfigureLogicState(ProcessBatchNumberState, () =>
            {
                if (Model.SelectedBatchNumber == DefaultModuleVocab.VocabCancel.IdentificationKey)
                {
                    NextState = DisplayCheckDamageState;
                }
                else
                {
                    foreach (var batchNumber in Model.ValidBatchNumbers)
                    {
                        if (Model.SelectedBatchNumber == batchNumber)
                        {
                            Model.CurrentStepCount += 1;
                            NextState = GetNextPickRoute;
                            return;
                        }
                    }

                    NextState = DisplayBatchNumberState;
                    // In the encoder, we are conditionally setting the voice enabled flag based upon whether the runner requires voice (RunnerRequiresVoiceEnabled).
                    // Upon a RunnerRequiresVoiceEnabled slot response, SelectedBatchNumber number will be null.
                    if (!string.IsNullOrEmpty(Model.SelectedBatchNumber))
                    {
                        CurrentUserMessage = Translate.GetLocalizedTextForKey("LApp_EnterBatchNumber_Wrong");
                    }
                }
            }, DisplayBatchNumberState, DisplayCheckDamageState, GetNextPickRoute);


            //------------------------------------------------
            //Enter Quantity
            ConfigureDisplayState(DisplayQuanityState, 
                                  ProcessQuantityState,
                                  DisplayCheckDamageState,
                                  followedByBackgroundActivity: true,
                                  backgroundActivityHeaderKey: "LApp_BackgroundActivity_Header_TransmitPickQuantity",
                                  encodeAction: EncodeQuantity, 
                                  decodeAction: DecodeQuantity);

            ConfigureLogicState(ProcessQuantityState, () =>
            {
                bool parsed = int.TryParse(Model.EnteredQuantityString, out int enteredQuantity);
                NextState = DisplayQuanityState;

                if (Model.EnteredQuantityString == DefaultModuleVocab.VocabCancel.IdentificationKey)
                {
                    NextState = DisplayCheckDamageState;
                }
                else if (!parsed)
                {
                    CurrentUserMessage = Translate.GetLocalizedTextForKey("LApp_EnterQuantity_Failed");
                }
                else
                {

                    Model.EnteredQuantity = enteredQuantity;
                    int expectedQuantity = (int)Model.CurrentPickRoute.QtyToPick;

                    if (Model.EnteredQuantity == expectedQuantity)
                    {
                        try
                        {
                            Model.DataService.ConfirmPickTasksQuantityAsync(Model.CurrentLicensePlateId, Model.SelectedBatchNumber, Model.CurrentTransactionId, Model.CurrentPickRoute.OrderId, Model.CurrentPickRoute.LineId, Model.EnteredQuantity);
                            Model.CurrentStepCount += 1;
                            NextState = GetNextPickRoute;
                        }
                        catch (Exception)
                        {
                            Manager.ExecuteGoToStateAsync(LAppBusinessLogic.SignOnState);
                        }
                    }
                    else if (Model.EnteredQuantity < expectedQuantity)
                    {
                        CurrentUserMessage = Translate.GetLocalizedTextForKey("LApp_EnterQuantity_Wrong");
                    }
                    else
                    {
                        CurrentUserMessage = Translate.GetLocalizedTextForKey("LApp_EnterQuantity_Wrong");
                    }
                }
            }, DisplayQuanityState, DisplayCheckDamageState, GetNextPickRoute);

            //------------------------------------------------
            //Get next pick route
            ConfigureLogicState(GetNextPickRoute, async () =>
            {
                try
                {
                    Model.CurrentPickRoute = await Model.DataService.GetPickTasksAsync(Model.CurrentWarehouse.Id, Model.CurrentZone.Id, (int)Model.CurrentOrder);
                    NextState = CheckMoreWork;
                }
                catch (Exception)
                {
                    Manager.ExecuteGoToStateAsync(LAppBusinessLogic.SignOnState);
                }
            }, CheckMoreWork);

            //------------------------------------------------
            //Assignment complete return to previous state machine to get another order
            ConfigureDisplayState(DisplayAssignmentComplete, 
                                  ProcessAssignmentComplete,
                                  CoreAppStates.Standby, 
                                  () =>
                                  {
                                      Manager.ExecuteGoToStateAsync(LAppGetOrderLogic.GetOrderState);
                                  },
                                  followedByBackgroundActivity: true,
                                  backgroundActivityHeaderKey: "LApp_BackgroundActivity_Header_RetrieveOrders",
                                  encodeAction: EncodeDisplayAssignmentComplete, 
                                  decodeAction: DecodeDisplayAssignmentComplete);

            ConfigureReturnLogicState(ProcessAssignmentComplete, () => { });
        }

        #region EncodersDecoders
        private WorkflowObjectContainer EncodeDisplayLocation(ILAppModel model)
        {
            var uiElements = GenerateEnterLocationDisplayElements(model.CurrentPickRoute, model.CurrentLicensePlateId);

            var wfoContainer = new WorkflowObjectContainer();

            var wfo = WorkflowObjectFactory.CreateReadyUIElementIntent(Translate.GetLocalizedTextForKey("LApp_Location_Header"),
                                                       "readyOne",
                                                       Translate.GetLocalizedTextForKey("LApp_Location_Prompt", model.CurrentPickRoute.LocationName),
                                                       uiElements,
                                                       model.CurrentUserMessage,
                                                       initialPrompt: model.CurrentUserMessage);

            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private List<UIElement> GenerateEnterLocationDisplayElements(PickRoute pickRoute, int licensePlate)
        {
            return new List<UIElement>
            {
                new UIElement
                {
                    ElementType = UIElementType.Banner,
                    Label = Translate.GetLocalizedTextForKey("LApp_PickRouteDetails_LocationBanner_Label")
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = pickRoute.LocationName,
                    LabelInfo = Translate.GetLocalizedTextForKey("LApp_PickRouteDetails_Location_Label"),
                    LabelInfoVertical = true,
                    InlineWithNext = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Banner,
                    Label = Translate.GetLocalizedTextForKey("LApp_PickRouteDetails_DetailsBanner_Label")
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Order# ",
                    Value = pickRoute.OrderId.ToString(),
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Product Code ",
                    Value = pickRoute.ProductCode,
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Product Description",
                    Value = pickRoute.ProductName,
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "License Plate",
                    Value = licensePlate.ToString(),
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Parent Locs",
                    Value = "",
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Bin",
                    Value = "",
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = Translate.GetLocalizedTextForKey("LApp_PickRouteDetails_Quantity_Label"),
                    Value = ((int)pickRoute.QtyToPick).ToString(),
                    ValueInlineWithLabel = true
                }
            };
        }

        private void DecodeDisplayLocation(SlotContainer slotContainer, ILAppModel model)
        {
            model.LocationResponse = GenericBaseEncoder<ILAppModel>.CheckForButtonPress(slotContainer);
        }

        private WorkflowObjectContainer EncodeDisplayAssignmentComplete(ILAppModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();

            var wfo = WorkflowObjectFactory.CreateReadyIntent(Translate.GetLocalizedTextForKey("LApp_AssignmentComplete_Header", model.CurrentOrder.ToString()),
                                                              "assignmentComplete",
                                                              Translate.GetLocalizedTextForKey("LApp_AssignmentComplete_Prompt"),
                                                              model.CurrentUserMessage,
                                                              initialPrompt: model.CurrentUserMessage);

            wfo.MessageType = model.MessageType;
            wfo.ProgressBarProperties.isVisible = true;
            wfo.ProgressBarProperties.TotalStepCount = 3; // Real example would request this through the model and data service
            wfo.ProgressBarProperties.CurrentStepCount = model.CurrentStepCount;
            wfo.ProgressBarProperties.TitleLabelText = Translate.GetLocalizedTextForKey("LApp_Product_Progress_Header");
            wfo.ProgressBarProperties.ProgressLabelText = wfo.ProgressBarProperties.CurrentStepCount + "/" + wfo.ProgressBarProperties.TotalStepCount;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodeDisplayAssignmentComplete(SlotContainer slotContainer, ILAppModel model)
        {
            model.DeletePhotos();
        }

        private WorkflowObjectContainer EncodeDisplayProduct(ILAppModel model)
        {
            var uiElements = GenerateEnterProductDisplayElements(model.CurrentPickRoute, model.CurrentLicensePlateId);

            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateGetValueIntent(Translate.GetLocalizedTextForKey("LApp_Product_Header"),
                                                                "enterProduct",
                                                                Translate.GetLocalizedTextForKey("LApp_Product_Label"),
                                                                Translate.GetLocalizedTextForKey("LApp_Product_Prompt", model.CurrentPickRoute.ProductName),
                                                                uiElements,
                                                                model.CurrentUserMessage,
                                                                initialPrompt: model.CurrentUserMessage);

            // Add the product checkdigit as the response expression,
            // which is passed to the speech recognizer.  The recognizer uses
            // this information to modify its behavior when decoding and
            // scoring the resulting words.
            wfo.ValueProperties.ResponseExpressions = new List<string> { model.CurrentPickRoute.ProductCheckDigit };
            wfo.ValueProperties.MinRequiredLength = model.CurrentPickRoute.ProductCheckDigit.Length;
            wfo.ValueProperties.MaxAllowedLength = model.CurrentPickRoute.ProductCheckDigit.Length;

            // Add the product checkdigit as the expected spoken value and
            // the expected scanned value.  This allows the GuidedWorkRunner
            // to validate the response from the user to ensure that it is
            // correct before sending the result back to the decoder/state machine.
            wfo.ValueProperties.ExpectedSpokenOrTypedValues = new List<string> { model.CurrentPickRoute.ProductCheckDigit };
            wfo.ValueProperties.ExpectedScannedValues = new List<string> { model.CurrentPickRoute.ProductCheckDigit };

            wfo.MainTabTitle = Translate.GetLocalizedTextForKey("LApp_Main_Tab_Label");
            wfo.ProgressBarProperties.isVisible = true;
            wfo.ProgressBarProperties.TotalStepCount = 3; // Real example would request this through the model and data service
            wfo.ProgressBarProperties.CurrentStepCount = model.CurrentStepCount;
            wfo.ProgressBarProperties.TitleLabelText = Translate.GetLocalizedTextForKey("LApp_Product_Progress_Header");
            wfo.ProgressBarProperties.ProgressLabelText = wfo.ProgressBarProperties.CurrentStepCount + "/" + wfo.ProgressBarProperties.TotalStepCount;

            //SAMPLE: Use of validation message when command is not currently available
            wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(LAppModuleVocab.SkipProduct, 
                confirm: true, itemValidationDelegate: () => { return Translate.GetLocalizedTextForKey("LApp_Overflow_SkipProduct_Not_Implemented"); }));
            wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(DefaultModuleVocab.VocabCancel));

            // Remove hint for final version
            wfo.ValueProperties.Placeholder = model.CurrentPickRoute.ProductCheckDigit;

            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private List<UIElement> GenerateEnterProductDisplayElements(PickRoute pickRoute, int licensePlate)
        {
            return new List<UIElement>
            {
                new UIElement
                {
                    ElementType = UIElementType.Banner,
                    Label = Translate.GetLocalizedTextForKey("LApp_PickRouteDetails_LocationBanner_Label")
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = pickRoute.LocationName,
                    LabelInfo = Translate.GetLocalizedTextForKey("LApp_PickRouteDetails_Location_Label"),
                    LabelInfoVertical = true,
                    InlineWithNext = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Banner,
                    Label = Translate.GetLocalizedTextForKey("LApp_PickRouteDetails_DetailsBanner_Label")
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Order# ",
                    Value = pickRoute.OrderId.ToString(),
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Product Code ",
                    Value = pickRoute.ProductCode,
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Product Description",
                    Value = pickRoute.ProductName,
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Image,
                    Label = Translate.GetLocalizedTextForKey("LApp_Product_Image_Label"),
                    ImageData = pickRoute.GetImageDataFromEmbededResource(),
                    TabIndex = 1
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "License Plate",
                    Value = licensePlate.ToString(),
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Parent Locs",
                    Value = "",
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Bin",
                    Value = "",
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = Translate.GetLocalizedTextForKey("LApp_PickRouteDetails_Quantity_Label"),
                    Value = ((int)pickRoute.QtyToPick).ToString(),
                    ValueInlineWithLabel = true,
                    TabIndex = 1
                },
                new UIElement
                {
                    ElementType = UIElementType.Tab,
                    Label =  Translate.GetLocalizedTextForKey("LApp_Product_Tab_Label"),
                    TabIndex = 1
                }
            };
        }

        private void DecodeDisplayProduct(SlotContainer slotContainer, ILAppModel model)
        {
            model.ProductCheckDigitResponse = GenericBaseEncoder<ILAppModel>.DecodeValueEntry(slotContainer);
        }

        private WorkflowObjectContainer EncodeDisplayCheckDamage(ILAppModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateBooleanIntent(Translate.GetLocalizedTextForKey("LApp_CheckProductDamaged_Header"),
                                                                "productDamaged",
                                                                Translate.GetLocalizedTextForKey("LApp_CheckProductDamaged_Prompt"),
                                                                model.CurrentUserMessage,
                                                                initialPrompt: model.CurrentUserMessage);
            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodeDisplayCheckDamage(SlotContainer slotContainer, ILAppModel model)
        {
            model.CheckProductDamagedResponse = GenericBaseEncoder<ILAppModel>.DecodeBooleanPrompt(slotContainer);
        }

        private WorkflowObjectContainer EncodeEnquireDamage(ILAppModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();

            var wfo = WorkflowObjectFactory.CreateMenuItemsIntent(Translate.GetLocalizedTextForKey("LApp_EnquireDamageTypeHeader"),
                                                                     "enquireDamageType",
                                                                     Translate.GetLocalizedTextForKey("LApp_EnquireDamageTypePrompt"),
                                                                     LAppMappings.MapDamageTypesToEnum.Keys.ToList(),
                                                                     initialPrompt: model.CurrentUserMessage,
                                                                     maxSelection: LAppMappings.MapDamageTypesToEnum.Keys.Count);
            wfo.MenuItemsProperties.DisplayKeys = true;
            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodeEnquireDamage(SlotContainer slotContainer, ILAppModel model)
        {
            var (responses, _) = GenericBaseEncoder<ILAppModel>.DecodeMenuItems(slotContainer);
            model.Damages.Clear();
            model.SpecifiedDamages.Clear();
            foreach (var response in responses)
            {
                if (response.Selected)
                {
                    model.SpecifiedDamages.Add(LAppMappings.MapDamageTypesToEnum[response.DisplayName]);
                }
            }
        }

        private WorkflowObjectContainer EncodeEnquireSeverity(ILAppModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var damageType = LAppMappings.MapDamageTypesToString[model.SpecifiedDamages[0]];

            var wfo = WorkflowObjectFactory.CreateMenuItemsIntent(Translate.GetLocalizedTextForKey("LApp_EnquireDamageSeverityHeader", damageType),
                                                                     "enquireDamageSeverity",
                                                                     Translate.GetLocalizedTextForKey("LApp_EnquireDamageSeverityPrompt", damageType),
                                                                     LAppMappings.MapSeveritiesToEnum.Keys.ToList(),
                                                                     initialPrompt: model.CurrentUserMessage);
            wfo.MenuItemsProperties.DisplayKeys = true;
            wfo.MessageType = model.MessageType;
            wfo.ExtraData["DamageType"] = damageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodeEnquireSeverity(SlotContainer slotContainer, ILAppModel model)
        {
            var (response, _) = GenericBaseEncoder<ILAppModel>.DecodeMenuItems(slotContainer);

            DamageTypes damageType = DamageTypes.Unspecified;
            foreach (Slot slot in slotContainer.Slots)
            {
                if (LAppMappings.MapDamageTypesToEnum.Keys.Contains(slot.ExtraData["DamageType"]))
                {
                    damageType = LAppMappings.MapDamageTypesToEnum[slot.ExtraData["DamageType"]];
                    break;
                }
            }

            model.Damages[damageType] = LAppMappings.MapSeveritiesToEnum[response[0].DisplayName];
            model.SpecifiedDamages.Remove(damageType);
        }

        private WorkflowObjectContainer EncodeTakePhotoOfDamage(ILAppModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            //Check if photo intent is supported, on the device app is running on (i.e. if running on A700x, photo intent is not supported)
            //In this example if the photo intent is not supported, they we send a ready intent with a different prompt (instructions)
            //NOTE: depending on application, the check may also be required in the Decode method for this state
            if (model.UnsupportedIntents.Contains(WorkflowIntent.PhotoCapturePrompt))
            {
                var wfo = WorkflowObjectFactory.CreateReadyIntent("Alternate Header", "takePhoto",
                    Translate.GetLocalizedTextForKey("LApp_TakePhoto_Prompt_No_Camera"), "");
                wfoContainer.Add(wfo);
            }
            else
            {
                var wfo = WorkflowObjectFactory.CreatePhotoCaptureIntent(Translate.GetLocalizedTextForKey("LApp_TakePhoto_Header"),
                                                                         "takePhoto",
                                                                         Translate.GetLocalizedTextForKey("LApp_TakePhoto_Prompt"),
                                                                         model.CurrentUserMessage,
                                                                         initialPrompt: model.CurrentUserMessage);
                wfo.AdditionalProperties.ConfirmVoiceInput = true;
                wfo.AdditionalProperties.ConfirmScreenInput = true;
                wfo.MessageType = model.MessageType;
                wfoContainer.Add(wfo);

            }
            return wfoContainer;
        }

        private void DecodeTakePhotoOfDamage(SlotContainer slotContainer, ILAppModel model)
        {
            model.FilePathList = GenericBaseEncoder<ILAppModel>.DecodePhotoEntry(slotContainer);
        }

        private WorkflowObjectContainer EncodeSerialNumber(ILAppModel model)
        {
            var uiElements = GenerateEnterSerialNumberDisplayElements(Model.CurrentPickRoute, Model.CurrentLicensePlateId, model.ValidSerialNumbers[0]);

            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateGetValueIntent(Translate.GetLocalizedTextForKey("LApp_SerialNumber_Header"),
                                                                 "enterSerialNumber",
                                                                 Translate.GetLocalizedTextForKey("LApp_SerialNumber_Label"),
                                                                 Translate.GetLocalizedTextForKey("LApp_SerialNumber_Prompt", ((int)(Model.CurrentPickRoute).QtyToPick).ToString()),
                                                                 null,
                                                                 uiElements,
                                                                 model.CurrentUserMessage,
                                                                 initialPrompt: model.CurrentUserMessage);

            wfo.ValueProperties.AllowedCharacters = CharacterSet.AlphaNumeric;
            wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(DefaultModuleVocab.VocabCancel));

            //Set the voiceEnableEntry flag based on whether voice input is required (i.e. Running on A700x)
            //voiceRequired flag would be set in the model based on errors returned in an intent.  (see SpecialCaseEncoder.cs)
            //In this example if running on android,worker is expected to scan the value, but if the barcode is damaged they can type
            //it in. If running on A700x, then there would be no screen, and therefore voice should be enabled. Depending on business
            //case, prompt could also be changed, or an entirely different intent could have been sent
            wfo.ValueProperties.VoiceEnableEntry = model.RunnerRequiresVoiceEnabled;

            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private List<UIElement> GenerateEnterSerialNumberDisplayElements(PickRoute pickRoute, int licensePlate, string serialNumber)
        {
            return new List<UIElement>
            {
                new UIElement
                {
                    ElementType = UIElementType.Banner,
                    Label = Translate.GetLocalizedTextForKey("LApp_PickRouteDetails_LocationBanner_Label")
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = pickRoute.LocationName,
                    LabelInfo = Translate.GetLocalizedTextForKey("LApp_PickRouteDetails_Location_Label"),
                    LabelInfoVertical = true,
                    InlineWithNext = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Banner,
                    Label = Translate.GetLocalizedTextForKey("LApp_PickRouteDetails_DetailsBanner_Label")
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Order# ",
                    Value = pickRoute.OrderId.ToString(),
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Serial# ",
                    Value = serialNumber,
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Product Code ",
                    Value = pickRoute.ProductCode,
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Product Description",
                    Value = pickRoute.ProductName,
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "License Plate",
                    Value = licensePlate.ToString(),
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Parent Locs",
                    Value = "",
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Bin",
                    Value = "",
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = Translate.GetLocalizedTextForKey("LApp_PickRouteDetails_Quantity_Label"),
                    Value = ((int)pickRoute.QtyToPick).ToString(),
                    ValueInlineWithLabel = true
                }
            };
        }

        private void DecodeSerialNumber(SlotContainer slotContainer, ILAppModel model)
        {
            model.EnteredSerialNumber = GenericBaseEncoder<ILAppModel>.DecodeValueEntry(slotContainer);
        }

        private WorkflowObjectContainer EncodeBatchNumber(ILAppModel model)
        {
            var uiElements = GenerateEnterBatchNumberDisplayElements(model.CurrentPickRoute, model.CurrentLicensePlateId, model.ValidBatchNumbers[0]);

            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateGetValueIntent(Translate.GetLocalizedTextForKey("LApp_BatchNumber_Header"),
                                                                 "enterBatchNumber",
                                                                 Translate.GetLocalizedTextForKey("LApp_BatchNumber_Label"),
                                                                 Translate.GetLocalizedTextForKey("LApp_BatchNumber_Prompt", ((int)(model.CurrentPickRoute).QtyToPick).ToString()),
                                                                 null,
                                                                 uiElements,
                                                                 model.CurrentUserMessage,
                                                                 initialPrompt: model.CurrentUserMessage);

            wfo.ValueProperties.AllowedCharacters = CharacterSet.AlphaNumeric;
            wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(DefaultModuleVocab.VocabCancel));

            //Set the voiceEnableEntry flag based on whether voice input is required (i.e. Running on A700x)
            //voiceRequired flag would be set in the model based on errors returned in an intent.  (see SpecialCaseEncoder.cs)
            //In this example if running on android,worker is expected to scan the value, but if the barcode is damaged they can type
            //it in. If running on A700x, then there would be no screen, and therefore voice should be enabled. Depending on business
            //case, prompt could also be changed, or an entirely different intent could have been sent
            wfo.ValueProperties.VoiceEnableEntry = model.RunnerRequiresVoiceEnabled;

            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private List<UIElement> GenerateEnterBatchNumberDisplayElements(PickRoute pickRoute, int licensePlate, string batchlNumber)
        {
            return new List<UIElement>
            {
                new UIElement
                {
                    ElementType = UIElementType.Banner,
                    Label = Translate.GetLocalizedTextForKey("LApp_PickRouteDetails_LocationBanner_Label")
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = pickRoute.LocationName,
                    LabelInfo = Translate.GetLocalizedTextForKey("LApp_PickRouteDetails_Location_Label"),
                    LabelInfoVertical = true,
                    InlineWithNext = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Banner,
                    Label = Translate.GetLocalizedTextForKey("LApp_PickRouteDetails_DetailsBanner_Label")
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Order# ",
                    Value = pickRoute.OrderId.ToString(),
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Batch# ",
                    Value = batchlNumber,
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Product Code ",
                    Value = pickRoute.ProductCode,
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Product Description",
                    Value = pickRoute.ProductName,
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "License Plate",
                    Value = licensePlate.ToString(),
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Parent Locs",
                    Value = "",
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Bin",
                    Value = "",
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = Translate.GetLocalizedTextForKey("LApp_PickRouteDetails_Quantity_Label"),
                    Value = ((int)pickRoute.QtyToPick).ToString(),
                    ValueInlineWithLabel = true
                }
            };
        }

        private void DecodeBatchNumber(SlotContainer slotContainer, ILAppModel model)
        {
            model.SelectedBatchNumber = GenericBaseEncoder<ILAppModel>.DecodeValueEntry(slotContainer); ;
        }

        private WorkflowObjectContainer EncodeQuantity(ILAppModel model)
        {
            var uiElements = GenerateEnterQuantityDisplayElements(model.CurrentPickRoute, model.CurrentLicensePlateId);

            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateGetValueIntent(Translate.GetLocalizedTextForKey("LApp_Quantity_Header"),
                                                                  "enterQuantity",
                                                                  Translate.GetLocalizedTextForKey("LApp_Quantity_Label"),
                                                                  Translate.GetLocalizedTextForKey("LApp_Quantity_Prompt", ((int)(Model.CurrentPickRoute).QtyToPick).ToString()),
                                                                  uiElements,
                                                                  model.CurrentUserMessage,
                                                                  initialPrompt: model.CurrentUserMessage);

            wfo.ValueProperties.MinRequiredLength = 1;
            wfo.ValueProperties.MaxAllowedLength = 2;
            wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(DefaultModuleVocab.VocabCancel));

            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private List<UIElement> GenerateEnterQuantityDisplayElements(PickRoute pickRoute, int licensePlate)
        {
            return new List<UIElement>
            {
                new UIElement
                {
                    ElementType = UIElementType.Banner,
                    Label = Translate.GetLocalizedTextForKey("LApp_PickRouteDetails_LocationBanner_Label")
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = pickRoute.LocationName,
                    LabelInfo = Translate.GetLocalizedTextForKey("LApp_PickRouteDetails_Location_Label"),
                    LabelInfoVertical = true,
                    InlineWithNext = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Banner,
                    Label = Translate.GetLocalizedTextForKey("LApp_PickRouteDetails_DetailsBanner_Label")
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Order# ",
                    Value = pickRoute.OrderId.ToString(),
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Product Code ",
                    Value = pickRoute.ProductCode,
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Product Description",
                    Value = pickRoute.ProductName,
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "License Plate",
                    Value = licensePlate.ToString(),
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Parent Locs",
                    Value = "",
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = "Bin",
                    Value = "",
                    ValueInlineWithLabel = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = Translate.GetLocalizedTextForKey("LApp_PickRouteDetails_Quantity_Label"),
                    Value = ((int)pickRoute.QtyToPick).ToString(),
                    ValueInlineWithLabel = true
                }
            };
        }

        private void DecodeQuantity(SlotContainer slotContainer, ILAppModel model)
        {
            model.EnteredQuantityString = GenericBaseEncoder<ILAppModel>.DecodeValueEntry(slotContainer);
        }

        #endregion
    }
}
