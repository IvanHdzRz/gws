//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
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

    public class OrderPickingIntentBuilder : GuidedWorkIntentBuilder, IOrderPickingIntentBuilder
    {
        private readonly ILog _Log = LogManager.GetLogger(nameof(OrderPickingIntentBuilder));

        private const string GetContainersIntent = "GetContainers";
        private const string AcknowledgeLocationIntent = "AcknowledgeLocation";
        private const string ConfirmNoMoreWorkIntent = "ConfirmNoMoreWork";
        private const string ConfirmSkipProductIntent = "ConfirmSkipProduct";
        private const string EnterProductIntent = "EnterProduct";
        private const string EnterQuantityIntent = "EnterQuantity";
        private const string ConfirmQuantityIntent = "ConfirmQuantity";
        private const string EnterSubProductIntent = "EnterSubProduct";
        private const string ConfirmOverflowIntent = "ConfirmOverflow";
        private const string OrderStatusIntent = "OrderStatus";
        private const string GoToStagingIntent = "GoToStaging";
        private const string EnterStagingLocationIntent = "EnterStagingLocation";
        private const string ConfirmStagingLocationIntent = "ConfirmStagingLocation";
        private const string AllDoneIntent = "AllDone";

        public string DecodeGetContainers(string slots)
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

                if (jsonSlotIntent == GetContainersIntent)
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

        public string DecodeEnterSubProduct(string slots)
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

                if (jsonSlotIntent == EnterSubProductIntent)
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

        public bool DecodeConfirmOverflow(string slots)
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

                if (jsonSlotIntent == ConfirmOverflowIntent)
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

        public string DecodeGoToStaging(string slots)
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

                if (jsonSlotIntent == GoToStagingIntent)
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

        public string DecodeEnterStagingLocation(string slots)
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

                if (jsonSlotIntent == EnterStagingLocationIntent)
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

        public bool DecodeConfirmStagingLocation(string slots)
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

                if (jsonSlotIntent == ConfirmStagingLocationIntent)
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

        public string EncodeGetContainers(string serializedData)
        {
            return EncodeCustomIntent(GetContainersIntent, SlotDataSource.ExtraData, serializedData);
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

        public string EncodeEnterSubProduct(string serializedData)
        {
            return EncodeCustomIntent(EnterSubProductIntent, SlotDataSource.Data, serializedData);
        }

        public string EncodeConfirmOverflow(string serializedData)
        {
            return EncodeCustomIntent(ConfirmOverflowIntent, SlotDataSource.ExtraData, serializedData);
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

        public string EncodeGoToStaging(string serializedData)
        {
            return EncodeCustomIntent(GoToStagingIntent, SlotDataSource.ExtraData, serializedData);
        }

        public string EncodeAllDone(string serializedData)
        {
            return EncodeCustomIntent(AllDoneIntent, SlotDataSource.ExtraData, serializedData);
        }

        public string EncodeEnterStagingLocation(string serializedData)
        {
            return EncodeCustomIntent(EnterStagingLocationIntent, SlotDataSource.Data, serializedData);
        }

        public string EncodeConfirmStagingLocation(string serializedData)
        {
            return EncodeCustomIntent(ConfirmStagingLocationIntent, SlotDataSource.ExtraData, serializedData);
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
