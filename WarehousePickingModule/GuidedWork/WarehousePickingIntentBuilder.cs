//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Common.Logging;
    using GuidedWorkRunner;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class WarehousePickingIntentBuilder : GuidedWorkIntentBuilder, IWarehousePickingIntentBuilder
    {
        private readonly ILog _Log = LogManager.GetLogger(nameof(WarehousePickingIntentBuilder));

        private const string SubcentersIntent = "Subcenters";
        private const string LabelPrinterIntent = "LabelPrinter";
        private const string PickTripInfoIntent = "PickTripInfo";
        private const string AcknowledgeLocationIntent = "AcknowledgeLocation";
        private const string ConfirmNoMoreWorkIntent = "ConfirmNoMoreWork";
        private const string ConfirmSkipProductIntent = "ConfirmSkipProduct";
        private const string EnterProductIntent = "EnterProduct";
        private const string EnterQuantityIntent = "EnterQuantity";
        private const string ConfirmQuantityIntent = "ConfirmQuantity";
        private const string OrderStatusIntent = "OrderStatus";
        private const string OrderSummaryIntent = "OrderSummary";
        private const string PickPerformanceIntent = "PickPerformance";
        private const string LastPickIntent = "LastPick";

        public Tuple<string, string> DecodeSignIn(string slots)
        {
            if (slots == null)
            {
                throw new ArgumentNullException();
            }

            var parsedSlots = JObject.Parse(slots);

            var jsonSlots =
                from p in parsedSlots["slots"]
                select p;

            foreach (var jsonSlot in jsonSlots)
            {
                string jsonSlotIntent = jsonSlot["Intent"].ToString();

                if (jsonSlotIntent == WorkflowIntent.SignIn)
                {
                    Dictionary<string, string> extraData =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonSlot["ExtraData"].ToString());

                    extraData.TryGetValue("UserId", out string userId);
                    extraData.TryGetValue("Password", out string password);
                    return Tuple.Create(userId, password);
                }
            }

            return null;
        }

        public string DecodeAcknowledgeLocation(string slots)
        {
            if (slots == null)
            {
                throw new ArgumentNullException();
            }

            var parsedSlots = JObject.Parse(slots);

            var jsonSlots =
                from p in parsedSlots["slots"]
                select p;

            foreach (var jsonSlot in jsonSlots)
            {
                string jsonSlotIntent = jsonSlot["Intent"].ToString();

                if (jsonSlotIntent == AcknowledgeLocationIntent)
                {
                    if (jsonSlot["ExtraData"] != null)
                    {
                        Dictionary<string, string> extraData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonSlot["ExtraData"].ToString());

                        return extraData["Button"];
                    }
                }
            }

            return null;
        }

        public string DecodeEnterProduct(string slots)
        {
            if (slots == null)
            {
                throw new ArgumentNullException();
            }

            var parsedSlots = JObject.Parse(slots);

            var jsonSlots =
                from p in parsedSlots["slots"]
                select p;

            foreach (var jsonSlot in jsonSlots)
            {
                string jsonSlotIntent = jsonSlot["Intent"].ToString();

                if (jsonSlotIntent == EnterProductIntent)
                {
                    if (jsonSlot["ExtraData"] != null)
                    {
                        Dictionary<string, string> extraData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonSlot["ExtraData"].ToString());

                        return extraData["Button"];
                    }

                    return jsonSlot["Data"].ToString();
                }
            }

            return null; 
        }

        public string DecodeEnterQuantity(string slots)
        {
            if (slots == null)
            {
                throw new ArgumentNullException();
            }

            var parsedSlots = JObject.Parse(slots);

            var jsonSlots =
                from p in parsedSlots["slots"]
                select p;

            foreach (var jsonSlot in jsonSlots)
            {
                string jsonSlotIntent = jsonSlot["Intent"].ToString();

                if (jsonSlotIntent == EnterQuantityIntent)
                {
                    if (jsonSlot["ExtraData"] != null)
                    {
                        Dictionary<string, string> extraData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonSlot["ExtraData"].ToString());

                        return extraData["Button"];
                    }

                    return jsonSlot["Data"].ToString();
                }
            }

            return string.Empty;
        }

        public bool DecodeConfirmQuantity(string slots)
        {
            if (slots == null)
            {
                throw new ArgumentNullException();
            }

            var parsedSlots = JObject.Parse(slots);

            var jsonSlots =
                from p in parsedSlots["slots"]
                select p;

            foreach (var jsonSlot in jsonSlots)
            {
                string jsonSlotIntent = jsonSlot["Intent"].ToString();

                if (jsonSlotIntent == ConfirmQuantityIntent)
                {
                    if (jsonSlot["ExtraData"] != null)
                    {
                        Dictionary<string, string> extraData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonSlot["ExtraData"].ToString());

                        // need to match the button key in the ExtraData
                        if (Translate.GetLocalizedTextForKey("VocabWord_Yes").Equals(extraData["Button"]))
                        {
                            return true;
                        }
                        else if (Translate.GetLocalizedTextForKey("VocabWord_No").Equals(extraData["Button"]))
                        {
                            return false;
                        }

                        // TODO: handle overflow items
                    }
                }
            }

            return false;
        }

        public bool DecodeConfirmNoMore(string slots)
        {
            if (slots == null)
            {
                throw new ArgumentNullException();
            }

            var parsedSlots = JObject.Parse(slots);

            var jsonSlots =
                from p in parsedSlots["slots"]
                select p;

            foreach (var jsonSlot in jsonSlots)
            {
                string jsonSlotIntent = jsonSlot["Intent"].ToString();

                if (jsonSlotIntent == ConfirmNoMoreWorkIntent)
                {
                    Dictionary<string, string> extraData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonSlot["ExtraData"].ToString());

                    // need to match the button key in the ExtraData
                    if (Translate.GetLocalizedTextForKey("VocabWord_Yes").Equals(extraData["Button"]))
                    {
                        return true;
                    }
                    else if (Translate.GetLocalizedTextForKey("VocabWord_No").Equals(extraData["Button"]))
                    {
                        return false;
                    }

                    // TODO: handle overflow items
                }
            }

            return false;
        }

        public bool DecodeConfirmSkipProduct(string slots)
        {
            if (slots == null)
            {
                throw new ArgumentNullException();
            }

            var parsedSlots = JObject.Parse(slots);

            var jsonSlots =
                from p in parsedSlots["slots"]
                select p;

            foreach (var jsonSlot in jsonSlots)
            {
                string jsonSlotIntent = jsonSlot["Intent"].ToString();

                if (jsonSlotIntent == ConfirmSkipProductIntent)
                {
                    Dictionary<string, string> extraData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonSlot["ExtraData"].ToString());

                    // need to match the button key in the ExtraData
                    if (Translate.GetLocalizedTextForKey("VocabWord_Yes").Equals(extraData["Button"]))
                    {
                        return true;
                    }
                    else if (Translate.GetLocalizedTextForKey("VocabWord_No").Equals(extraData["Button"]))
                    {
                        return false;
                    }

                    // TODO: handle overflow items
                }
            }

            return false;
        }

        public string DecodeSubcenters(string slots)
        {
            if (slots == null)
            {
                throw new ArgumentNullException();
            }

            var parsedSlots = JObject.Parse(slots);

            var jsonSlots =
                from p in parsedSlots["slots"]
                select p;

            foreach (var jsonSlot in jsonSlots)
            {
                string jsonSlotIntent = jsonSlot["Intent"].ToString();

                if (jsonSlotIntent == SubcentersIntent)
                {
                    Dictionary<string, string> extraData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonSlot["ExtraData"].ToString());

                    // TODO: Handle Cancel
                    return extraData["Button"];
                }
            }
            return null;
        }

        public string DecodeLabelPrinter(string slots)
        {
            if (slots == null)
            {
                throw new ArgumentNullException();
            }

            var parsedSlots = JObject.Parse(slots);

            var jsonSlots =
                from p in parsedSlots["slots"]
                select p;

            foreach (var jsonSlot in jsonSlots)
            {
                string jsonSlotIntent = jsonSlot["Intent"].ToString();

                if (jsonSlotIntent == LabelPrinterIntent)
                {
                    if (jsonSlot["ExtraData"] != null)
                    {
                        Dictionary<string, string> extraData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonSlot["ExtraData"].ToString());

                        return extraData["Button"];
                    }

                    return jsonSlot["Data"].ToString();
                }
            }

            return string.Empty;
        }

        public string DecodePickTripInfo(string slots)
        {
            if (slots == null)
            {
                throw new ArgumentNullException();
            }

            var parsedSlots = JObject.Parse(slots);

            var jsonSlots =
                from p in parsedSlots["slots"]
                select p;

            foreach (var jsonSlot in jsonSlots)
            {
                string jsonSlotIntent = jsonSlot["Intent"].ToString();

                if (jsonSlotIntent == PickTripInfoIntent)
                {
                    if (jsonSlot["ExtraData"] != null)
                    {
                        Dictionary<string, string> extraData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonSlot["ExtraData"].ToString());

                        return extraData["Button"];
                    }
                }
            }

            return null;
        }

        public string DecodePickOrderStatus(string slots)
        {
            if (slots == null)
            {
                throw new ArgumentNullException();
            }

            var parsedSlots = JObject.Parse(slots);

            var jsonSlots =
                from p in parsedSlots["slots"]
                select p;

            foreach (var jsonSlot in jsonSlots)
            {
                string jsonSlotIntent = jsonSlot["Intent"].ToString();

                if (jsonSlotIntent == OrderStatusIntent)
                {
                    if (jsonSlot["ExtraData"] != null)
                    {
                        Dictionary<string, string> extraData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonSlot["ExtraData"].ToString());

                        return extraData["Button"];
                    }
                }
            }

            return null;
        }

        public string DecodePickOrderSummary(string slots)
        {
            if (slots == null)
            {
                throw new ArgumentNullException();
            }

            var parsedSlots = JObject.Parse(slots);

            var jsonSlots =
                from p in parsedSlots["slots"]
                select p;

            foreach (var jsonSlot in jsonSlots)
            {
                string jsonSlotIntent = jsonSlot["Intent"].ToString();

                if (jsonSlotIntent == OrderSummaryIntent)
                {
                    if (jsonSlot["ExtraData"] != null)
                    {
                        Dictionary<string, string> extraData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonSlot["ExtraData"].ToString());

                        return extraData["Button"];
                    }
                }
            }

            return null;
        }

        public string DecodePickPerformance(string slots)
        {
            if (slots == null)
            {
                throw new ArgumentNullException();
            }

            var parsedSlots = JObject.Parse(slots);

            var jsonSlots =
                from p in parsedSlots["slots"]
                select p;

            foreach (var jsonSlot in jsonSlots)
            {
                string jsonSlotIntent = jsonSlot["Intent"].ToString();

                if (jsonSlotIntent == PickPerformanceIntent)
                {
                    if (jsonSlot["ExtraData"] != null)
                    {
                        Dictionary<string, string> extraData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonSlot["ExtraData"].ToString());

                        return extraData["Button"];
                    }
                }
            }

            return null;
        }

        public string DecodeLastPick(string slots)
        {
            if (slots == null)
            {
                throw new ArgumentNullException();
            }

            var parsedSlots = JObject.Parse(slots);

            var jsonSlots =
                from p in parsedSlots["slots"]
                select p;

            foreach (var jsonSlot in jsonSlots)
            {
                string jsonSlotIntent = jsonSlot["Intent"].ToString();

                if (jsonSlotIntent == LastPickIntent)
                {
                    if (jsonSlot["ExtraData"] != null)
                    {
                        Dictionary<string, string> extraData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonSlot["ExtraData"].ToString());

                        return extraData["Button"];
                    }
                }
            }

            return null;
        }

        public string EncodeBackgroundActivity(string header)
        {
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                OpenIntentWriter(writer);

                WriteBackgroundActivityIntent(writer, header);

                CloseIntentWriter(writer);
            }

            _Log.Debug(sb.ToString());

            return sb.ToString();
        }

        public string EncodeSignIn(string message)
        {
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                OpenIntentWriter(writer);

                WriteSignInIntent(writer,
                    //Translate.GetLocalizedTextForKey("VoiceLink_SignOn_Header"),
                    "Selection",
                    "SignIn",
                    message,
                    navigateBackEnabled: true,
                    navigateBackLeavesWorkflow: true);

                CloseIntentWriter(writer);
            }

            _Log.Debug(sb.ToString());

            return sb.ToString();
        }

        public string EncodeOperPrep()
        {
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                OpenIntentWriter(writer);
                WriteOperPrepIntent(writer);
                CloseIntentWriter(writer);
            }

            _Log.Debug(sb.ToString());

            return sb.ToString();
        }

        public string EncodeSubcenters(string serializedData)
        {
            return EncodeCustomIntent(SubcentersIntent, SlotDataSource.ExtraData, serializedData);
        }

        public string EncodeLabelPrinter(string serializedData)
        {
            return EncodeCustomIntent(LabelPrinterIntent, SlotDataSource.Data, serializedData);
        }

        public string EncodePickTripInfo(string serializedData)
        {
            return EncodeCustomIntent(PickTripInfoIntent, SlotDataSource.ExtraData, serializedData);
        }

        public string EncodeAcknowledgeLocation(string serializedData)
        {
            return EncodeCustomIntent(AcknowledgeLocationIntent, SlotDataSource.ExtraData, serializedData);
        }

        public string EncodeEnterProduct(string serializedData)
        {
            return EncodeCustomIntent(EnterProductIntent, SlotDataSource.Data, serializedData);
        }

        public string EncodeEnterQuantity(string serializedData)
        {
            return EncodeCustomIntent(EnterQuantityIntent, SlotDataSource.Data, serializedData);
        }

        public string EncodeConfirmQuantity(string serializedData)
        {
            return EncodeCustomIntent(ConfirmQuantityIntent, SlotDataSource.ExtraData, serializedData);
        }

        public string EncodeConfirmNoMore(string serializedData)
        {
            return EncodeCustomIntent(ConfirmNoMoreWorkIntent, SlotDataSource.ExtraData, serializedData);
        }

        public string EncodeConfirmSkipProduct(string serializedData)
        {
            return EncodeCustomIntent(ConfirmSkipProductIntent, SlotDataSource.ExtraData, serializedData);
        }

        public string EncodeOrderStatus(string serializedData)
        {
            return EncodeCustomIntent(OrderStatusIntent, SlotDataSource.ExtraData, serializedData);
        }

        public string EncodeOrderSummary(string serializedData)
        {
            return EncodeCustomIntent(OrderSummaryIntent, SlotDataSource.ExtraData, serializedData);
        }

        public string EncodePickPerformance(string serializedData)
        {
            return EncodeCustomIntent(PickPerformanceIntent, SlotDataSource.ExtraData, serializedData);
        }

        public string EncodeLastPick(string serializedData)
        {
            return EncodeCustomIntent(LastPickIntent, SlotDataSource.ExtraData,serializedData);
        }

        private string EncodeCustomIntent(string intentName, string slotDataSource, string serializedData)
        {
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                OpenIntentWriter(writer);
                WriteCustomIntent(writer, intentName, slotDataSource, serializedData);
                CloseIntentWriter(writer);
            }

            _Log.Debug(sb.ToString());

            return sb.ToString();
        }
    }
}
