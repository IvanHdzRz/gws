//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System;
    using System.Collections.Generic;

    public class WarehousePickingActivityTracker : IWarehousePickingActivityTracker
    {
        private TimeEndPoints _Trip = new TimeEndPoints();
        private List<TimeEndPoints> _Aisles = new List<TimeEndPoints>();
        private List<TimeEndPoints> _Slots = new List<TimeEndPoints>();

        public void StartTrip()
        {
            System.Diagnostics.Debug.WriteLine("#!# Start Trip");
            _Trip.Start = DateTime.Now;
        }

        public void EndTrip()
        {
            System.Diagnostics.Debug.WriteLine("#!# End Trip");
            _Trip.End = DateTime.Now;
            _Trip.Duration = _Trip.End - _Trip.Start;
            System.Diagnostics.Debug.WriteLine($"#!# Trip time is {GetTripTime().ToString()}");
        }

        public TimeSpan GetTripTime()
        {
            return _Trip.Duration;
        }

        public void StartAisle(string aisle)
        {
            System.Diagnostics.Debug.WriteLine($"#!# Start Aisle {aisle}");
            DateTime now = DateTime.Now;
            for (int i = 0; i < _Aisles.Count; i++)
            {
                if (_Aisles[i].Name == aisle)
                {
                    if (_Aisles[i].Start != _Aisles[i].End)
                    {
                        _Aisles[i].Start = now;
                        _Aisles[i].End = now;
                    }
                    return;
                }
            }

            _Aisles.Add(new TimeEndPoints { Name = aisle, Start = now, End = now });
        }

        public void EndAisle(string aisle)
        {
            System.Diagnostics.Debug.WriteLine($"#!# End Aisle {aisle}");
            for (int i = _Aisles.Count - 1; i >= 0; i--)
            {
                if (_Aisles[i].Name == aisle)
                {
                    _Aisles[i].End = DateTime.Now;
                    _Aisles[i].Duration += _Aisles[i].End - _Aisles[i].Start;
                    System.Diagnostics.Debug.WriteLine($"#!# Aise {aisle} time is {_Aisles[i].Duration.ToString()}");
                    return;
                }
            }

            System.Diagnostics.Debug.WriteLine($"#!# Aise {aisle} not found");
        }

        public List<TimeEndPoints> GetAisleTimes()
        {
            return _Aisles;
        }

        public void StartSlot(string slot, string aisle)
        {
            System.Diagnostics.Debug.WriteLine($"#!# Start Slot {slot} in aisle {aisle}");
            DateTime now = DateTime.Now;
            for (int i = 0; i < _Slots.Count; i++)
            {
                if (_Slots[i].Name == slot && _Slots[i].Tag == aisle)
                {
                    if (_Slots[i].Start != _Slots[i].End)
                    {
                        _Slots[i].Start = now;
                        _Slots[i].End = now;
                    }
                    return;
                }
            }

            _Slots.Add(new TimeEndPoints { Name = slot, Tag = aisle, Start = now, End = now });
        }

        public void EndSlot(string slot)
        {
            System.Diagnostics.Debug.WriteLine($"#!# End Slot {slot}");
            for (int i = _Slots.Count - 1; i >= 0; i--)
            {
                if (_Slots[i].Name == slot)
                {
                    _Slots[i].End = DateTime.Now;
                    _Slots[i].Duration += _Slots[i].End - _Slots[i].Start;
                    System.Diagnostics.Debug.WriteLine($"#!# Slot {slot} time is {_Slots[i].Duration.ToString()}");
                    return;
                }
            }

            System.Diagnostics.Debug.WriteLine($"#!# Slot {slot} not found");
        }

        public List<TimeEndPoints> GetSlotTimes()
        {
            return _Slots;
        }
    }
}
