//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
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

    public class ReceivingIntentBuilder : GuidedWorkIntentBuilder, IReceivingIntentBuilder
    {
        private readonly ILog _Log = LogManager.GetLogger(nameof(ReceivingIntentBuilder));

        private const string OrderItemsIntent = "OrderItems";
        private const string EnterHiQuantityIntent = "EnterHiQuantity";
        private const string EnterTiQuantityIntent = "EnterTiQuantity";
        private const string ConfirmQuantityIntent = "ConfirmQuantity";
        private const string PrintingLabelIntent = "PrintingLabel";
        private const string ApplyLabelIntent = "ApplyLabel";
        private const string ConfirmConditionIntent = "ConfirmCondition";
        private const string ReportDamageIntent = "ReportDamage";
        private const string ReceivingSummaryIntent = "ReceivingSummary";

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

        public string DecodeOrder(string slots)
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

                if (jsonSlotIntent == OrderItemsIntent)
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

        public string DecodeHiQuantity(string slots)
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

                if (jsonSlotIntent == EnterHiQuantityIntent)
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

        public string DecodeTiQuantity(string slots)
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

                if (jsonSlotIntent == EnterTiQuantityIntent)
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

        public string DecodePrintingLabel(string slots)
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

                if (jsonSlotIntent == PrintingLabelIntent)
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

        public string DecodeApplyLabel(string slots)
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

                if (jsonSlotIntent == ApplyLabelIntent)
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

        public bool DecodePalletCondition(string slots)
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

                if (jsonSlotIntent == ConfirmConditionIntent)
                {
                    Dictionary<string, string> extraData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonSlot["ExtraData"].ToString());

                    // need to match the button key in the ExtraData
                    if (Translate.GetLocalizedTextForKey("VocabWord_Good").Equals(extraData["Button"]))
                    {
                        return true;
                    }
                    else if (Translate.GetLocalizedTextForKey("VocabWord_Damaged").Equals(extraData["Button"]))
                    {
                        return false;
                    }

                    // TODO: handle overflow items
                }
            }

            return false;
        }

        public string DecodeDamageReason(string slots)
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

                if (jsonSlotIntent == ReportDamageIntent)
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

        public string DecodeInvoiceSummary(string slots)
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

                if (jsonSlotIntent == ReceivingSummaryIntent)
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
                    "Receiving",
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

        public string EncodeOrders(string serializedData)
        {
            return EncodeCustomIntent(OrderItemsIntent, SlotDataSource.Data, serializedData);
        }

        public string EncodeHiQuantity(string serializedData)
        {
            return EncodeCustomIntent(EnterHiQuantityIntent, SlotDataSource.Data, serializedData);
        }

        public string EncodeTiQuantity(string serializedData)
        {
            return EncodeCustomIntent(EnterTiQuantityIntent, SlotDataSource.Data, serializedData);
        }

        public string EncodeConfirmQuantity(string serializedData)
        {
            return EncodeCustomIntent(ConfirmQuantityIntent, SlotDataSource.ExtraData, serializedData);
        }

        public string EncodePrintingLabel(string serializedData)
        {
            return EncodeCustomIntent(PrintingLabelIntent, SlotDataSource.ExtraData, serializedData);
        }

        public string EncodeApplyLabel(string serializedData)
        {
            return EncodeCustomIntent(ApplyLabelIntent, SlotDataSource.ExtraData, serializedData);
        }

        public string EncodePalletCondition(string serializedData)
        {
            return EncodeCustomIntent(ConfirmConditionIntent, SlotDataSource.ExtraData, serializedData);
        }

        public string EncodeDamageReason(string serializedData)
        {
            return EncodeCustomIntent(ReportDamageIntent, SlotDataSource.ExtraData, serializedData);
        }

        public string EncodeInvoiceSummary(string serializedData)
        {
            return EncodeCustomIntent(ReceivingSummaryIntent, SlotDataSource.ExtraData, serializedData);
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
