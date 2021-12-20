//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Threading.Tasks;
    using GuidedWork;

    public class WarehousePickingUser
    {
        public string Id { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// The model of the WArehouse Picking Workflow.  The interface abstracts the retrieval of data from the 
    /// data service.  The model serves as temporary storage of information between controllers associated 
    /// with simple WFAs.
    /// </summary>
    public interface IWarehousePickingModel : IWorkflowModel
    {
        /// <summary>
        /// Maintains the application's state in the workflow.
        /// </summary>
        IWarehousePickingStateMachine StateMachine { get; }

        /// <summary>
        /// Consolidated grouping of data items required for presentation layer.
        /// </summary>
        WarehousePickingDataStore DataStore { get; }

        /// <summary>
        /// The current user performing the workflow.
        /// </summary>
        WarehousePickingUser User { get; }

        /// <summary>
        /// The subcenter selected in the DisplaySubcenters state.
        /// </summary>
        string SelectedSubcenter { get; set; }

        /// <summary>
        /// The label printer entered in the DisplayLabelPrinter state.
        /// </summary>
        string LabelPrinter { get; set; }

        /// <summary>
        /// The user input from the DisplayPickTripInfo state.
        /// </summary>
        string AcknowledgePickTripInfo { get; set; }

        /// <summary>
        /// The user input from the DisplayAcknowledgeLocation state.
        /// </summary>
        string AcknowledgedLocation { get; set; }

        /// <summary>
        /// The checkdigit or user vocab entered in the DisplayEnterProduct state.
        /// </summary>
        string EnteredProduct { get; set; }

        /// <summary>
        /// The quantity or user vocab entered in the DisplayEnterQuantity state.
        /// </summary>
        string EnteredQuantityString { get; set; }

        /// <summary>
        /// The quantity value entered in the DisplayEnterQuantity state.
        /// </summary>
        int EnteredQuantity { get; set; }

        /// <summary>
        /// The user input from the DisplayConfirmQuantity state.
        /// </summary>
        bool ShortProductConfirmation { get; set; }

        /// <summary>
        /// The user input from the DisplayConfirmNoMore state.
        /// </summary>
        bool NoMoreConfirmation { get; set; }

        /// <summary>
        /// The user input from the DisplayConfirmSkipProduct state.
        /// </summary>
        bool SkipProductConfirmation { get; set; }

        /// <summary>
        /// The user input from the DisplayPickOrderStatus state.
        /// </summary>
        string AcknowledgePickOrderStatus { get; set; }

        /// <summary>
        /// The user input from the DisplayPickOrderSummary state.
        /// </summary>
        string AcknowledgePickOrderSummary { get; set; }

        /// <summary>
        /// The user input from the DisplayPickPerformance state.
        /// </summary>
        string AcknowledgePickPerformance { get; set; }

        /// <summary>
        /// The user input from the DisplayLastPick state.
        /// </summary>
        string AcknowledgeLastPick { get; set; }

        /// <summary>
        /// The current message to be displayed on the GUI.
        /// </summary>
        string CurrentUserMessage { get; set; }

        /// <summary>
        /// Gets the warehouse picking work item.
        /// </summary>
        WarehousePickingWorkItem CurrentWarehousePickingWorkItem { get; }

        /// <summary>
        /// Returns whether or not there are more warehouse picking work items to handle.
        /// </summary>
        bool MoreWorkItems { get; }

        /// <summary>
        /// Request that the model and state machine process the user input.
        /// </summary>
        /// <returns></returns>
        Task ProcessUserInputAsync();

        /// <summary>
        /// Update the credentials of the user performing the workflow.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="password">The password</param>
        void UpdateUser(string userId, string password);

        /// <summary>
        /// Validate the user's credentials in the User property.
        /// </summary>
        /// <returns>True if the user crendentials are valid.</returns>
        Task<bool> ValidateUserCredentialsAsync();

        /// <summary>
        /// Retrieve the subcenters from the host system.
        /// </summary>
        Task RetrieveSubcentersAsync();

        /// <summary>
        /// Retrieve the pick assignement from the host system.
        /// </summary>
        Task RetrievePicksAsync();

        /// <summary>
        /// Reset the data service so that the assignment can be executed again.
        /// </summary>
        void ResetPicks();

        /// <summary>
        /// Gets the aisle for the requested product ID.
        /// </summary>
        /// <param name="productID">The product ID of the requested product</param>
        /// <returns>The aisle location of the product.</returns>
        string GetAisleForProductID(long productID);

        /// <summary>
        /// Sets the Default and Current Product ID to the WarehousePickingWorkItem id (ie. the default product for the portion of the order)
        /// </summary>
        void SetRequestedProduct();

        /// <summary>
        /// Sets the current warehouse picking work item to the in-progress state in the data service and
        /// updates the server.
        /// </summary>
        void SetWorkItemInProgress();

        /// <summary>
        /// Sets the current warehouse picking work item to the complete state in the data service and
        /// updates the server.
        /// </summary>
        Task SetWorkItemCompleteAsync();

        /// <summary>
        /// Modifies the sequence number of the current work item to put it at
        /// the end of the assignment.
        /// </summary>
        void SetWorkItemSkipped();

        /// <summary>
        /// Updates the remaining quantity to be picked.
        /// </summary>
        void UpdateRemainingQuantity(int quantityPicked);

        /// <summary> 
        /// Sets the shorted indicator on the current work item. 
        /// </summary> 
        void SetShortedIndicator();

        /// <summary>
        /// Generates a record for the product with the quantity picked for that item
        /// </summary>
        Task GeneratePickedQuantityRecordAsync(int quantityPicked);
    }
}
