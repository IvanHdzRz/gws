//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System;
    using System.Collections.Generic;

    public interface IWarehousePickingIntentBuilder
    {
        Tuple<string, string> DecodeSignIn(string slots);
        string DecodeAcknowledgeLocation(string slots);
        string DecodeSubcenters(string slots);
        string DecodeLabelPrinter(string slots);
        string DecodePickTripInfo(string slots);
        string DecodeEnterProduct(string slots);
        string DecodeEnterQuantity(string slots);
        bool DecodeConfirmQuantity(string slots);
        bool DecodeConfirmNoMore(string slots);
        bool DecodeConfirmSkipProduct(string slots);
        string DecodePickOrderStatus(string slots);
        string DecodePickOrderSummary(string slots);
        string DecodePickPerformance(string slots);
        string DecodeLastPick(string slots);

        string EncodeSignIn(string message);
        string EncodeOperPrep();
        string EncodeAcknowledgeLocation(string serializedData);
        string EncodeSubcenters(string serializedData);
        string EncodeLabelPrinter(string serializedData);
        string EncodePickTripInfo(string serializedData);
        string EncodeEnterProduct(string serializedData);
        string EncodeEnterQuantity(string serializedData);
        string EncodeConfirmQuantity(string serializedData);
        string EncodeConfirmNoMore(string serializedData);
        string EncodeConfirmSkipProduct(string serializedData);
        string EncodeOrderStatus(string serializedData);
        string EncodeOrderSummary(string serializedData);
        string EncodePickPerformance(string serializedData);
        string EncodeLastPick(string serializedData);

        string EncodeBackgroundActivity(string header);
    }
}
