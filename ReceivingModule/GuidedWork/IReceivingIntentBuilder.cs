//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System;

    public interface IReceivingIntentBuilder
    {
        Tuple<string, string> DecodeSignIn(string slots);
        string DecodeOrder(string slots);
        string DecodeHiQuantity(string slots);
        string DecodeTiQuantity(string slots);
        bool DecodeConfirmQuantity(string slots);
        string DecodePrintingLabel(string slots);
        string DecodeApplyLabel(string slots);
        bool DecodePalletCondition(string slots);
        string DecodeDamageReason(string slots);
        string DecodeInvoiceSummary(string slots);

        string EncodeSignIn(string message);
        string EncodeOperPrep();
        string EncodeOrders(string serializedData);
        string EncodeHiQuantity(string serializedData);
        string EncodeTiQuantity(string serializedData);
        string EncodeConfirmQuantity(string serializedData);
        string EncodePrintingLabel(string serializedData);
        string EncodeApplyLabel(string serializedData);
        string EncodePalletCondition(string serializedData);
        string EncodeDamageReason(string serializedData);
        string EncodeInvoiceSummary(string serializedData);

        string EncodeBackgroundActivity(string header);
    }
}
