//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2015 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GuidedWork;
    using Retail;

    /// <summary>
    /// The model of the Order Picking Workflow.  The interface abstracts the retrieval of data from the 
    /// data service.  The model serves as temporary storage of information between controllers associated 
    /// with simple WFAs.
    /// </summary>
    public interface IOrderPickingModel : IWorkflowModel
    {
        /// <summary>
        /// Maintains the application's state in the workflow.
        /// </summary>
        IOrderPickingStateMachine StateMachine { get; }

        /// <summary>
        /// Consolidated grouping of data items required for presentation layer.
        /// </summary>
        OrderPickingDataStore DataStore { get; }

        /// <summary>
        /// Response from DisplayGetContainers state
        /// </summary>
        string GetContainersResponse { get; set; }

        /// <summary>
        /// Response from DisplayAcknowledgeLocation state
        /// </summary>
        string AcknowledgeLocationResponse { get; set; }

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
        /// The user input from the DisplayConfirmNoMore state.
        /// </summary>
        bool NoMoreConfirmation { get; set; }

        /// <summary>
        /// The user input from the DisplayConfirmSkipProduct state.
        /// </summary>
        bool SkipProductConfirmation { get; set; }

        /// <summary>
        /// The user input from the DisplayConfirmQuantity state.
        /// </summary>
        bool ProductQuantityConfirmation { get; set; }

        /// <summary>
        /// The user input from the DisplayConfirmOverflow state.
        /// </summary>
        bool OverflowConfirmation { get; set; }

        /// <summary>
        /// The user input from the DisplayGoToStaging state.
        /// </summary>
        string GoToStagingResponse { get; set; }

        /// <summary>
        /// The user input from the DisplayEnterStagingLocation state.
        /// </summary>
        string StagingLocationResponse { get; set; }

        /// <summary>
        /// The user input from the DisplayConfirmStagingLocation state.
        /// </summary>
        bool StagingLocationConfirmation { get; set; }

        /// <summary>
        /// Gets the order picking work item.
        /// </summary>
        /// <value>The order picking work item.</value>
        OrderPickingWorkItem CurrentOrderPickingWorkItem { get; }

        /// <summary>
        /// Gets all of the non-completed OrderPickingWorkItems for the current order
        /// </summary>
        /// <returns>List of non-completed OrderPickingWorkItem objects</returns>
        List<OrderPickingWorkItem> CurrentAndUpcomingOrderPickingWorkItems { get; }

        /// <summary>
        /// Gets the current order picking customer.
        /// </summary>
        /// <value>The order picking customer.</value>
        OrderPickingOrderInfo OrderPickingOrderInfo { get; }

        /// <summary>
        /// Gets the list of order picking containers for the current group of orders.
        /// </summary>
        List<OrderPickingContainer> Containers { get; }

        /// <summary>
        /// Gets the current container for picking.
        /// </summary>
        /// <value>The container.</value>
        OrderPickingContainer CurrentPickingContainer { get; }

        /// <summary>
        /// Gets the current container to be staged.
        /// </summary>
        OrderPickingContainer CurrentStagingContainer { get; }

        /// <summary>
        /// Indicates whether or not to send a fill request if a picked quantity is shorted.
        /// </summary>
        bool RequestFillOnShort { get; }

        /// <summary>
        /// Gets the expected stock code response length.
        /// </summary>
        uint ExpectedStockCodeResponseLength { get; }

        /// <summary>
        /// Indicates whether or not the current product being picked is a substitution.
        /// </summary>
        /// <value>bool indicating if the current product is a substitution.</value>
        bool ProcessingSubstitution { get; set; }

        /// <summary>
        /// Gets the order identifier
        /// </summary>
        /// <value>The order identifier.</value>
        string OrderIdentifier { get; }

        /// <summary>
        /// Gets and sets the quantity of product last picked
        /// </summary>
        /// <value>The quantity last picked.</value>
        int QuantityLastPicked { get; set; }

        /// <summary>
        /// Gets the staging location
        /// </summary>
        string StagingLocation { get; set; }

        /// <summary>
        /// Gets the max spoken length for staging locations
        /// </summary>
        uint StagingMaxSpokenLength { get; set; }

        /// <summary>
        /// Gets the stock code response
        /// </summary>
        string StockCodeResponse { get; set; }

        /// <summary>
        /// Gets the remaining quantity to be picked
        /// </summary>
        /// <value>The remaining quantity.</value>
        int RemainingQuantity { get; }

        /// <summary>
        /// Indicates whether or not substitutions are available
        /// </summary>
        /// <value>The substitution state.</value>       
        bool SubstitutionsAvailable { get; }

        /// <summary>
        /// Determines if the current product is the first substitution for the original (default) product for the order
        /// </summary>
        /// <returns>true or false indiciating if the current product is the first substitution</returns>
        bool IsFirstSubstitution();

        /// <summary>
        /// Holds a reference to the requested product for the order picking work item.
        /// </summary>
        Product RequestedProduct { get; }

        /// <summary>
        /// Gets the current product associated with the order picking work item.
        /// </summary>
        /// <value>The product.</value>
        Product CurrentProduct { get; }

        /// <summary>
        /// Gets the accepted identifiers.
        /// </summary>
        /// <value>The accepted identifiers.</value>
        IReadOnlyList<string> AcceptedIdentifiers { get; }

        /// <summary>
        /// Gets the image path for the product associated with the current order picking work item.
        /// </summary>
        string CurrentImagePath { get; }

        /// <summary>
        /// Returns whether or not there are more order picking work items to handle.
        /// </summary>
        bool MoreWorkItems { get; }

        /// <summary>
        /// Gets the index of the current order picking work item.  This can be presented to give
        /// an indication of progress, e.g. item 1 of 7.
        /// </summary>
        int CurrentWorkItemIndex { get; }

        /// <summary>
        /// Gets the total number of order picking work items assigned to the user.  This can be
        /// presented to give an indication of progress, e.g. item 1 of 7.
        /// </summary>
        int TotalNumberOfWorkItems { get; }

        /// <summary>
        /// Request that the model and state machine process the user input.
        /// </summary>
        /// <returns></returns>
        Task ProcessUserInputAsync();

        Task GetPicksAndAssociateContainersAsync();

        /// <summary>
        /// Associates a new container with the order.
        /// </summary>
        void HandleOverflow();

        /// <summary>
        /// Sets the Default and Current Product ID to the OrderPickingWorkItem id (ie. the default product for the portion of the order)
        /// </summary>
        void SetRequestedProduct();

        /// <summary>
        /// Sets the Current Product ID property to the next available substitute product and indicates we are processing a substituation.
        /// </summary>
        void SetSubstitutedProduct();

        /// <summary>
        /// Send the staging location for an order ID.
        /// </summary>
        /// <param name="orderId">The ID of the Order</param>
        /// <param name="stagingLocation">The staging location</param>
        /// <returns>A Task to indicate when the operation is complete</returns>
        Task StoreStagingLocationAsync(long orderId, string stagingLocation);

        /// <summary>
        /// Used by the OrderPickingEnterSubItemController when the back button is pressed to "undo" any previous 
        /// picking actions to bring the user back to the enter quantity screen they saw last
        /// </summary>
        /// <returns>True if more substitutions exist on stack, False if no substitutions exist on stack</returns>
        bool RevertPickingActions();

        /// <summary>
        /// Sets the current order picking work item to the in-progress state in the data service and
        /// updates the server.
        /// </summary>
        void SetWorkItemInProgress();

        /// <summary>
        /// Sets the current order picking work item to the complete state in the data service and
        /// updates the server.
        /// </summary>
        Task SetWorkItemCompleteAsync();

        /// <summary>
        /// Determine if the productIdentifier parameter matches any part
        /// of either the product identifier or any of the product numbers.
        /// </summary>
        /// <param name="productIdentifier"></param>
        /// <returns>true if there is a match.</returns>
        bool IsValidIdentifier(string productIdentifier);

        /// <summary>
        /// Gets the response expressions based for the product being picked.
        /// </summary>
        /// <param name="hintLength"> The lenght of the hint</param>
        /// <returns>response expressions.</returns>
        HashSet<string> GetResponseExpressions(int hintLength);

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
        /// Generates a record for the product with the quantity picked for that item
        /// </summary>
        Task GeneratePickedQuantityRecordAsync(int quantityPicked);

        List<LocationDescriptor> GetLocationDescriptorsForProduct();
    }
}
