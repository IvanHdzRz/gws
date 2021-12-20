//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System;
    using System.Threading.Tasks;
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary;
    using RESTCommunication;

    public class ReceivingPrintLabelController : AbortableServerRequestController
    {
        private bool _WasAborted;
        private int _StartTickCount;
        private bool _CanNavigateBack = true;

        private readonly IGuidedWorkRunner _GuidedWorkRunner;
        private readonly IGuidedWorkStore _GuidedWorkStore;

        public ReceivingPrintLabelController(CoreViewControllerDependencies dependencies,
                                             ITemporaryDataStorage dataStorage,
                                             IServerReachability reachability,
                                             IRESTService restService,
                                             IGuidedWorkRunner guidedWorkRunner, 
                                             IGuidedWorkStore guidedWorkStore)
            : base(dependencies, dataStorage, reachability, restService)
        {
            _GuidedWorkRunner = guidedWorkRunner;
            _GuidedWorkStore = guidedWorkStore;
        }

        /// <summary>
        /// Whether or not back navigation is allowed.  Defaults to true.
        /// </summary>
        /// <returns><c>true</c>, if allow back navigation was shoulded, <c>false</c> otherwise.</returns>
        public override bool ShouldAllowBackNavigation()
        {
            return _CanNavigateBack;
        }

        protected override void BeginBackgroundAction()
        {
            {
                _StartTickCount = Environment.TickCount;
                _CanNavigateBack = false;

                Task.Run(async () =>
                {
                    Object result = null;
                    Log.Info("Starting background action.");
                    bool exceptionOccurred = false;
                    bool actionPassed = false;
                    bool failureOccurred = true;
                    if (null != BackgroundAction)
                    {
                        try
                        {
                            result = await ExecuteBackgroundActionAsync(ProgressUpdateIntervalMs);
                            actionPassed = ActionPassed(result);
                        }
                        catch (OperationCanceledException)
                        {
                            Log.Info($"Caught {nameof(OperationCanceledException)}. Aborting.");
                            _WasAborted = true;
                        }
                        catch (Exception e)
                        {
                            Log.Error(m => m("Error executing background action: {0}{1}{2}", e.Message, Environment.NewLine, e.StackTrace));
                            exceptionOccurred = true;
                        }
                        finally
                        {
                            // A failure excludes an exception occurrence
                            failureOccurred = !actionPassed && !exceptionOccurred;
                        }
                    }

                    Log.Info("operation complete");

                    await _UiDelegate.BeginInvokeOnMainThread(async () =>
                    {
                        if (_WasAborted)
                        {
                            Log.Info("Aborted");
                            return;
                        }

                        // Disable the abort command
                        _ViewModel.AbortCommand = new Command(() => { });

                        // Make sure we have waited the minimum amount of time
                        await DelayMinimumAsync();

                        UpdateUIAfterAction(actionPassed, exceptionOccurred, failureOccurred);

                        _CanNavigateBack = true;

                        // Speak error prompt if specifed
                        if (exceptionOccurred)
                        {
                            await _ViewModel.SpeakOnCommandAsync(SpokenPromptOnException);
                        }
                        else if (failureOccurred)
                        {
                            await _ViewModel.SpeakOnCommandAsync(SpokenPromptOnFailure);
                        }

                        // Reprogram the AbortCommand if not autoforwarding
                        if (exceptionOccurred && !AutoForwardOnException)
                        {
                            _ViewModel.AbortCommand = new Command(() =>
                            {
                                TriggerEvent(GetExceptionEventName());
                            });
                        }
                        else if (failureOccurred && !AutoForwardOnFailure)
                        {
                            _ViewModel.AbortCommand = new Command(() =>
                            {
                                TriggerEvent(GetFailedEventName());
                            });
                        }

                        // Publish the event if autoforwarding
                        if (exceptionOccurred && AutoForwardOnException)
                        {
                            await Task.Delay(DelayOnExceptionMs);
                            TriggerEvent(GetExceptionEventName());
                        }
                        else if (actionPassed)
                        {
                            await Task.Delay(DelayOnSuccessMs);
                            await _GuidedWorkRunner.RespondAsync();
                            await _GuidedWorkRunner.RequestAsync();
                            TriggerEvent(_GuidedWorkRunner.WorkflowEventName);
                        }
                        else if (failureOccurred && AutoForwardOnFailure)
                        {
                            await Task.Delay(DelayOnFailureMs);
                            TriggerEvent(GetFailedEventName());
                        }
                    });
                });
                Log.Info("Created background task.");
            }
        }

        /// <summary>
        /// Starts and awaits the background action, updating the UI with progress info if necessary.
        /// </summary>
        /// <param name="uiUpdateDelayMs">If greater than 0, the UI will be updated with progress
        /// information on the specified interval, until the background action completes.</param>
        /// <returns></returns>
        private async Task<object> ExecuteBackgroundActionAsync(int uiUpdateDelayMs = -1)
        {
            if (BackgroundAction == null)
            {
                return null;
            }
            if (uiUpdateDelayMs < 1)
            {
                return await BackgroundAction();
            }
            var backgroundTask = BackgroundAction();
            while (true)
            {
                var delayTask = Task.Delay(uiUpdateDelayMs);
                var awaitedTask = await Task.WhenAny(backgroundTask, delayTask).ConfigureAwait(false);
                if (awaitedTask == backgroundTask)
                {
                    return await backgroundTask;
                }
                else
                {
                    await DisplayProgressAsync();
                }
            }
        }

        private readonly object _Locker = new object();
        private bool _EventPublished = false;
        
        /// <summary>
        /// Prevent multiple events from being published.  Block
        /// requests after the first event is received.
        /// </summary>
        private void TriggerEvent(string eventName)
        {
            lock (_Locker)
            {
                if (_EventPublished)
                {
                    return;
                }

                PublishWorkflowActivityEvent(eventName);
                _EventPublished = true;
            }
        }

        private void UpdateUIAfterAction(bool actionPassed, bool exceptionOccurred, bool failureOccurred)
        {
            bool delayOccurring = false;
            if ((exceptionOccurred && (!AutoForwardOnException || DelayOnExceptionMs > 0)) ||
                (failureOccurred && (!AutoForwardOnFailure || DelayOnFailureMs > 0)) ||
                (actionPassed && DelayOnSuccessMs > 0))
            {
                delayOccurring = true;
            }

            // Determine the header
            string newHeader;
            string newSubHeader;
            if (exceptionOccurred)
            {
                Log.Info("Exception Occurred");
                newHeader = ExceptionHeaderMessage;
                newSubHeader = ExceptionSubHeaderMessage;
            }
            else if (actionPassed)
            {
                Log.Info("Action Passed");
                newHeader = CompletedHeaderMessage;
                newSubHeader = CompletedSubHeaderMessage;
            }
            else
            {
                Log.Info("Action Failed");
                newHeader = FailedHeaderMessage;
                newSubHeader = FailedSubHeaderMessage;
                if (AutoForwardOnFailure)
                {
                    DataStorage.StoreData(FailureMsgKey, FailedHeaderMessage, true);
                }
            }

            // Determine if the button should still be displayed
            //    If success and there's a delay, then hide the button
            //    If autofowarding and there's a delay, then hide the button
            if ((exceptionOccurred && AutoForwardOnException && DelayOnExceptionMs > 0) ||
                (failureOccurred && AutoForwardOnFailure && DelayOnFailureMs > 0) ||
                (actionPassed && DelayOnSuccessMs > 0))
            {
                _ViewModel.DisplayButton = false;
            }

            // Determine if the button text should be changed
            //    If not autofowarding, then change the button text
            if (delayOccurring)
            {
                _ViewModel.AbortButtonText = AlternateAbortButtonText;
            }

            // Determine if the spinner should be made invisible
            //   If success, then hide the spinner
            //   If not autofowarding, then hide the spinner since we still have the button
            //   Due to the timing of the transition, it looks strange if the spinner is removed.  
            //   If we don't do this, the spinner goes away and the header text moves down.
            //   For a split second, it's visible to the user and then the screen transitions.
            //   This change is so the screen doesn't change quckly and the user thinks something was missed.
            if (delayOccurring)
            {
                _ViewModel.IsRunning = false;
            }

            // Only update the header and subheader if it will be seen.
            if (delayOccurring)
            {
                _ViewModel.Header = newHeader;
                _ViewModel.SubHeader = newSubHeader;
            }
        }

        private async Task DelayMinimumAsync()
        {
            int currentTickCount = Environment.TickCount;
            int elapsedTicks = Math.Abs(currentTickCount - _StartTickCount);

            if (elapsedTicks < MinimumUiDelayMs)
            {
                await Task.Delay(MinimumUiDelayMs - elapsedTicks);
            }
        }

        /// <summary>
        /// Gets the background action, which is nothing.
        /// </summary>
        /// <value>The background action.</value>
        protected override Func<Task<object>> BackgroundAction
        {
            get
            {
                return async () =>
                {
                    await Task.Delay(2500);
                    _GuidedWorkStore.UpdateActiveObjectExtraData("Button", "Ready");
                    return Task.CompletedTask;
                };
            }
        }

        /// <summary>
        /// Gets the abort background action, which is nothing.
        /// </summary>
        /// <value>The abort background action.</value>
        protected override Func<Task> AbortBackgroundAction { get { return null; } }

        protected override bool ActionPassed(object actionResult)
        {
            return true;
        }
    }
}
