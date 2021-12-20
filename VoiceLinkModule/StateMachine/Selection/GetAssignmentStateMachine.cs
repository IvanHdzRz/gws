//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using Common.Logging;
    using GuidedWork;
    using GuidedWorkRunner;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class GetAssignmentStateMachine : SimplifiedBaseBusinessLogic<IVoiceLinkModel, VoiceLinkStateMachine, IVoiceLinkConfigRepository>
    {
        public static readonly CoreAppSMState StartGetAssignment = new CoreAppSMState(nameof(StartGetAssignment));
        public static readonly CoreAppSMState CommGetAssignments = new CoreAppSMState(nameof(CommGetAssignments));
        public static readonly CoreAppSMState CommGetPicks = new CoreAppSMState(nameof(CommGetPicks));
        public static readonly CoreAppSMState CommGetPicksComplete = new CoreAppSMState(nameof(CommGetPicksComplete));
        public static readonly CoreAppSMState AfterGetAssignments = new CoreAppSMState(nameof(AfterGetAssignments));

        private readonly ILog _Log = LogManager.GetLogger(nameof(GetAssignmentStateMachine));

        protected int _NumberOfAssignmentsToRequest;
        public bool PickOnly { get; set; } = false;

        public GetAssignmentStateMachine(SimplifiedStateMachineManager<VoiceLinkStateMachine, IVoiceLinkModel> manager, IVoiceLinkModel model) : base(manager, model)
        {
        }

        public override void ConfigureStates()
        {
            DefineStates();
        }

        protected virtual void DefineStates()
        {
            DefineStartState();
            DefineGetAssignmentsState();
            DefineGetPicksState();
        }

        protected virtual void DefineStartState()
        {
            ConfigureLogicState(StartGetAssignment,
                                () => PerformStartGetAssignment(),
                                CommGetAssignments);
        }

        protected virtual void DefineGetAssignmentsState()
        {
            ConfigureLogicState(CommGetAssignments,
                                async () => await CommPerformGetAssignmentsAsync(),
                                AfterGetAssignments);

            ConfigureReturnLogicState(AfterGetAssignments,
                                      () =>
                                      {
                                          if (AssignmentsResponse.CurrentResponse.ErrorCode == 2)
                                          {
                                              CurrentUserMessage = AssignmentsResponse.CurrentResponse.ErrorMessage;
                                              MessageType = UserMessageType.Standard;
                                          }
                                          else
                                          {
                                              NextState = CommGetPicks;
                                          }
                                          SelectionStateMachine.InProgressWork = false;
                                      },
                                      CommGetPicks);
        }

        protected virtual void DefineGetPicksState()
        {
            ConfigureLogicState(CommGetPicks,
                                async () =>
                                {
                                    NextState = CommGetPicksComplete;
                                    await Model.LUTtransmit(LutType.GetPicks, "VoiceLink_BackgroundActivity_Header_Retrieving_Picks",
                                        goToStateIfFail: SelectionStateMachine.StartSelection
                                    );
                                },
                                CommGetPicksComplete);

            ConfigureReturnLogicState(CommGetPicksComplete,
                                      () =>
                                      {
                                          if (PicksResponse.CurrentResponse.ErrorCode == 0)
                                          {
                                              foreach (Pick pick in PicksResponse.CurrentResponse)
                                              {
                                                  if (PickingRegionsResponse.CurrentPickingRegion.PickByPick)
                                                  {
                                                      pick.Status = "N";
                                                  }
                                                  else if (PickOnly)
                                                  {
                                                      switch (pick.Status)
                                                      {
                                                          case "N":
                                                              pick.Status = "X";
                                                              break;
                                                          case "B":
                                                              pick.Status = "N";
                                                              break;
                                                          case "G":
                                                              pick.Status = "N";
                                                              break;
                                                          case "S":
                                                              pick.Status = "N";
                                                              break;
                                                      }
                                                  }
                                              }
                                          }
                                      });
        }

        public override void Reset()
        {
            PickOnly = false;

            _NumberOfAssignmentsToRequest = 1;
        }

        protected abstract void PerformStartGetAssignment();

        protected virtual async Task CommPerformGetAssignmentsAsync()
        {
            var assignmentsToRequest = 0;
            if (PickingRegionsResponse.CurrentPickingRegion.AutoAssign)
            {
                assignmentsToRequest = _NumberOfAssignmentsToRequest;
            }
            NextState = AfterGetAssignments;
            await Model.LUTtransmit(LutType.GetAssignments, "VoiceLink_BackgroundActivity_Header_Retrieving_Assignments",
                new List<int>() { 2 },
                new GetAssignmentsParam(assignmentsToRequest),
                SelectionStateMachine.StartSelection
            );
        }
    }
}
