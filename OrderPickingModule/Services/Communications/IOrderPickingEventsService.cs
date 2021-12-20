//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System.Threading.Tasks;

    public interface IOrderPickingEventsService
    {
        /// <summary>
        /// Sends Start Order Picking Assignment.
        /// </summary>
        /// <returns>A task to signal the asynchronous operation.</returns>
        Task StartAssignmentEvent();

        /// <summary>
        /// Sends Stop Order Picking Assignment.
        /// </summary>
        /// <returns>A task to signal the asynchronous operation.</returns>
        Task StopAssignmentEvent();

        /// <summary>
        /// Sends skip item event.
        /// </summary>
        /// <returns>A task to signal the asynchronous operation.</returns>
        Task SendSkipItemEvent();

        /// <summary>
        /// Sends container overflow event.
        /// </summary>
        /// <returns>A task to signal the asynchronous operation.</returns>
        Task SendOverflowExceptionEvent();
    }
}
