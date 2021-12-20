//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp
{
    using Honeywell.Firebird.CoreLibrary;

    public interface ILAppConfigRepository : IConfigRepository
    {
        string EmbeddedDemoValue { get; }
        string ServerValue { get; }
    }
}
