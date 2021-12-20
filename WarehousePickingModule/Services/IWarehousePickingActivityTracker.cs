//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System;
    using System.Collections.Generic;

    public class TimeEndPoints
    {
        public string Name { get; set; }
        public string Tag { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public interface IWarehousePickingActivityTracker
    {
        void StartTrip();
        void EndTrip();
        TimeSpan GetTripTime(); 

        void StartAisle(string aisle);
        void EndAisle(string aisle);
        List<TimeEndPoints> GetAisleTimes();

        void StartSlot(string slot, string aisle);
        void EndSlot(string slot);
        List<TimeEndPoints> GetSlotTimes();
    }
}
