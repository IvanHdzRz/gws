//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{

    public interface IOrderPickingIntentBuilder
    {
        string DecodeGetContainers(string slots);
        string DecodeAcknowledgeLocation(string slots);
        string DecodeEnterProduct(string slots);
        string DecodeEnterQuantity(string slots);
        bool DecodeConfirmQuantity(string slots);
        string DecodeEnterSubProduct(string slots);
        bool DecodeConfirmOverflow(string slots);
        bool DecodeConfirmNoMore(string slots);
        bool DecodeConfirmSkipProduct(string slots);
        string DecodePickOrderStatus(string slots);
        string DecodeGoToStaging(string slots);
        string DecodeEnterStagingLocation(string slots);
        bool DecodeConfirmStagingLocation(string slots);

        string EncodeGetContainers(string serializedData);
        string EncodeAcknowledgeLocation(string serializedData);
        string EncodeEnterProduct(string serializedData);
        string EncodeEnterQuantity(string serializedData);
        string EncodeConfirmQuantity(string serializedData);
        string EncodeEnterSubProduct(string serializedData);
        string EncodeConfirmOverflow(string serializedData);
        string EncodeConfirmNoMore(string serializedData);
        string EncodeConfirmSkipProduct(string serializedData);
        string EncodeOrderStatus(string serializedData);
        string EncodeGoToStaging(string serializedData);
        string EncodeAllDone(string serializedData);
        string EncodeEnterStagingLocation(string serializedData);
        string EncodeConfirmStagingLocation(string serializedData);

        string EncodeBackgroundActivity(string header);
    }
}
