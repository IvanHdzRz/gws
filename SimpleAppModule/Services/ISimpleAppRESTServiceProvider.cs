//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace SimpleApp
{
    using System.Threading.Tasks;
    using GuidedWorkRunner;

    /// <summary>
    /// SimpleApp REST service provider
    /// </summary>
    public interface ISimpleAppRESTServiceProvider
    {
        Task<SimpleAppResponse> SignOnAsync(Operator newOperator);

        Task<SimpleAppResponse> DoSimpleAppRequestAsync(string someData);
    }
}
