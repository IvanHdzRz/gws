//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    public class Pick
    {
        public long PickId;
        public string CheckDigits;
        public string Aisle;
        public string Slot;
        public int QuantityToPick;
        public int ContainerPosition;
        public string ProductName;
        public string ProductScannedVerification;
        public string ProductSpokenVerification;
        public string ContainerSpokenVerification;
        public string ContainerScannedVerification;

        public int QuantityPicked;
        public bool Picked;
    }
}
