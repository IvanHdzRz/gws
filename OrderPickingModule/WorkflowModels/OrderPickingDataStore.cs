//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System;
    using System.Collections.Generic;
    using GuidedWork;
    using Newtonsoft.Json;
    using Retail;

    public class OrderPickingDataStore
    {
        public List<string> AcceptedIdentifiers { get; set; }
        public List<OrderPickingContainer> Containers { get; set; }
        public OrderPickingContainer CurrentPickingContainer { get; set; }
        public List<OrderPickingWorkItem> WorkItems { get; set; }
        public string CurrentProductIndex { get; set; }
        public OrderPickingContainer CurrentStagingContainer { get; set; }
        public string StockCodeResponse { get; set; }
        public string OriginalProductName { get; set; }
        public bool IsFirstSubstitution { get; set; }
        public string ProductImage { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string ProductIdentifier { get; set; }
        public List<Product> Products { get; set; }
        public string Price { get; set; }
        public string Size { get; set; }
        public string StagingLocation { get; set; }
        public uint StagingMaxSpokenLength { get; set; }
        public string LocationText { get; set; }
        public uint ExpectedStockCodeResponseLength { get; set; }
        public string OrderIdentifier { get; set; }
        public string TotalProducts { get; set; }
        public string PickedQuantity { get; set; }
        public int QuantityLastPicked { get; set; }
        public int RemainingQuantity { get; set; }
        public List<LocationDescriptor> LocationDescriptors { get; set; }

        public List<OrderPickingWorkItem> OrderPickingWorkItems { get; set; }

        public List<OrderPickingSummaryItem> OrderPickingSummaryItems { get; set; }

        public string SerializeObject()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static OrderPickingDataStore DeserializeObject(string jsonString)
        {
            return JsonConvert.DeserializeObject<OrderPickingDataStore>(jsonString);
        }

        private bool IsSmallStringFoundInTailOfBigString(string smallString, string bigString)
        {
            string substring = bigString.Substring(Math.Max(0, bigString.Length - smallString.Length));
            return smallString == substring;
        }

        /// <summary>
        /// Determine if the response parameter matches a valid identifier.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>true if there is a match.</returns>
        public bool IsValidIdentifier(string response)
        {
            foreach (var identifier in AcceptedIdentifiers)
            {
                if (IsSmallStringFoundInTailOfBigString(response, identifier))
                {
                    return true;
                }
            }
            return false;
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
