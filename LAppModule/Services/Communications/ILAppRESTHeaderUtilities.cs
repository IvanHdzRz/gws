//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp
{
    using System.Threading.Tasks;
    using RESTCommunication;

    /// <summary>
    /// A collection of properties and methods that is used to modify REST
    /// requests for authentication.  This is a marker interface for the
    /// Business One implementation.
    /// </summary>
    public interface ILAppRESTHeaderUtilities : IRESTHeaderUtilities
    {
        /// <summary>
        /// 
        /// </summary>
        void ResetCredentials();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<bool> ValidateCredentialsAsync(string userName, string password);
    }
}
