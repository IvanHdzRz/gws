//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using Retail;

    public class OrderInfoDTO : VersionedDTO
    {                
        public string OrderIdentifier { get; set; }        
        
        public string RequestedCompletionDate { get; set; }     
    }
}
