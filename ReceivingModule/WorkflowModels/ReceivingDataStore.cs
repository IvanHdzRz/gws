//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    public class ReceivingDataStore
    {
        public string OrderIdentifier { get; set; }
        public string ProductIdentifier { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public string ProductLocationText { get; set; }
        public string RequestedQuantity { get; set; }
        public string RemainingQuantity { get; set; }
        public string ProductDescription { get; set; }
        public string CurrentProductIndex { get; set; }
        public string TotalProducts { get; set; }
        public string HiQuantityLastReceived { get; set; }
        public string TiQuantityLastReceived { get; set; }
        public string QuantityLastReceived { get; set; }
        public string HiLabel { get; set; }
        public string TiLabel { get; set; }
        public string TotalLabel { get; set; }
        public string ReadyVocabWord { get; set; }
        public string NextVocabWord { get; set; }
        public string CancelVocabWord { get; set; }
        public List<ReceivingWorkItem> AllReceivingWorkItems { get; set; }
        public List<ReceivingWorkItem> CurrentAndUpcomingReceivingWorkItems { get; set; }

        public List<ReceivingSummaryItem> ReceivingSummaryItems { get; set; }

        public List<string> SelectionOptions { get; set; }

        public string SerializeObject()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static ReceivingDataStore DeserializeObject(string jsonString)
        {
            return JsonConvert.DeserializeObject<ReceivingDataStore>(jsonString);
        }

        public List<ReceivingSummaryItem> RetrieveMatchingOrders(string productId)
        {
            return ReceivingSummaryItems.Where(product => IsSmallStringFoundInTailOfBigString(productId, product.ProductIdentifier)).ToList();
        }

        private bool IsSmallStringFoundInTailOfBigString(string smallString, string bigString)
        {
            string substring = bigString.Substring(Math.Max(0, bigString.Length - smallString.Length));
            return smallString == substring;
        }
    }
}
