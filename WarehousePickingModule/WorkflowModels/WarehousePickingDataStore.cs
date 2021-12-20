//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System;
    using System.Collections.Generic;
    using GuidedWork;
    using Newtonsoft.Json;

    public class WarehousePickingDataStore
    {
        public string LabelPrinter { get; set; }
        public string CurrentProductIndex { get; set; }
        public string ProductImage { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string ProductIdentifier { get; set; }
        public string DetailedLocationText { get; set; }
        public string Instructions { get; set; }
        public string TripIdentifier { get; set; }
        public string Price { get; set; }
        public string Size { get; set; }
        public string TotalProducts { get; set; }
        public string PickedQuantity { get; set; }
        public int QuantityLastPicked { get; set; }
        public int RemainingQuantity { get; set; }
        public List<LocationDescriptor> LocationDescriptors { get; set; }
        public string Aisle { get; set; }
        public string SlotID { get; set; }
        public string CheckDigit { get; set; }
        public string ReadyVocabWord { get; set; }
        public string NextVocabWord { get; set; }
        public string CancelVocabWord { get; set; }
        public string SkipProductVocabWord { get; set; }
        public string PickedCases { get; set; }
        public string RemainingCases { get; set; }

        public List<string> SelectionOptions { get; set; }

        public List<WarehousePickingSummaryItem> WarehousePickingSummaryItems { get; set; }

        public List<WarehousePickingWorkItem> WarehousePickingWorkItems { get; set; }

        public WarehousePickingWorkItem PreviousWarehousePickingWorkItem { get; set; }

        public string SerializeObject()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static WarehousePickingDataStore DeserializeObject(string jsonString)
        {
            return JsonConvert.DeserializeObject<WarehousePickingDataStore>(jsonString);
        }

        private bool IsSmallStringFoundInTailOfBigString(string smallString, string bigString)
        {
            string substring = bigString.Substring(Math.Max(0, bigString.Length - smallString.Length));
            return smallString == substring;
        }

        /// <summary>
        /// Determine if the response parameter matches the checkdigit.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>true if there is a match.</returns>
        public bool ValidCheckDigit(string response)
        {
            return IsSmallStringFoundInTailOfBigString(response, CheckDigit);
        }

        /// <summary>
        /// Gets the response expressions based for the product being picked.
        /// </summary>
        /// <param name="hintLength"> The lenght of the hint</param>
        /// <returns>response expressions.</returns>
        public HashSet<string> GetResponseExpressions(int hintLength)
        {
            var responseExpressions = new HashSet<string>();

            string barcode = ProductIdentifier;
            if (string.IsNullOrEmpty(barcode)) return responseExpressions;
            string barcodeHint = barcode.Substring(Math.Max(0, barcode.Length - hintLength));
            responseExpressions.Add(barcodeHint);

            return responseExpressions;
        }
    }
}
