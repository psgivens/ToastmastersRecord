module ToastmastersRecord.Domain.RolePlacements 

open System
open ToastmastersRecord.Domain.CommandHandlers
open ToastmastersRecord.Domain.Infrastructure
open ToastmastersRecord.Domain.DomainTypes


type RolePlacementCommand = 
    | Open of RoleTypeId * MeetingId
    | Assign of MemberId * RoleRequestId
//    | Confirm
//    | Unconfirm
    | Unassign
    | Reassign of MemberId * MemberId * RoleRequestId
    | Complete
    | Cancel

type RolePlacementEvent =
    | Opened of RoleTypeId * MeetingId
    | Assigned of MemberId * RoleRequestId
    | Unassigned
    | Reassigned of MemberId * RoleRequestId
    | Completed
    | Canceled

type RolePlacementStateValue =
    | Open
    | Assigned of MemberId * RoleRequestId
    | Complete of MemberId * RoleRequestId
    | Canceled

type RolePlacementState = { State:RolePlacementStateValue; RoleTypeId:RoleTypeId; MeetingId:MeetingId  }

let (|MatchStateValue|_|) state =
    match state with 
    | Some(value) -> Some(value.State, value)
    | _ -> None 

let handle (command:CommandHandlers<RolePlacementEvent, Version>) (state:RolePlacementState option) (cmdenv:Envelope<RolePlacementCommand>) = 
    match state, cmdenv.Item with
    | None, RolePlacementCommand.Open(rid, mid) -> RolePlacementEvent.Opened(rid,mid)
    | MatchStateValue (RolePlacementStateValue.Open, _), RolePlacementCommand.Assign (mid,rrid) -> RolePlacementEvent.Assigned (mid,rrid)
    | MatchStateValue (RolePlacementStateValue.Assigned _, _), RolePlacementCommand.Unassign -> RolePlacementEvent.Unassigned
    | MatchStateValue (RolePlacementStateValue.Assigned _, _), RolePlacementCommand.Complete -> RolePlacementEvent.Completed
    | MatchStateValue (RolePlacementStateValue.Assigned (pmid,_), _), RolePlacementCommand.Reassign (pmid', mid, rrid) -> 
        if pmid = pmid' then RolePlacementEvent.Reassigned (mid, rrid)
        else failwith "Expected previous member id is incorrect"
    | MatchStateValue (RolePlacementStateValue.Complete _, _), _-> failwith "Once complete, the placement cannot be altered"
    | MatchStateValue (RolePlacementStateValue.Open _, _), RolePlacementCommand.Cancel -> RolePlacementEvent.Canceled
    | MatchStateValue (RolePlacementStateValue.Assigned _, _), RolePlacementCommand.Cancel -> RolePlacementEvent.Canceled
    | _, RolePlacementCommand.Assign _ -> failwith "Can only assign to an open slot"
    | _, RolePlacementCommand.Complete -> failwith "Can only complete an assigned position"
    | _, RolePlacementCommand.Unassign -> failwith "Can only unassign an assigned position"
    | _, RolePlacementCommand.Open _ -> failwith "Cannot open an open position"
    | _, RolePlacementCommand.Reassign _ -> failwith "Cannot reassign a role if it is not assigned"
    | _, RolePlacementCommand.Cancel _ -> failwith "Cannot cancel a role if does not exist or is complete"
    |> command.event

// TODO write 'evolve' function
let evolve (state:RolePlacementState option) (event:RolePlacementEvent) = 
    match state, event with
    | None, RolePlacementEvent.Opened(rid, mid) -> { RolePlacementState.State=Open; RoleTypeId=rid; MeetingId=mid }
    | MatchStateValue (RolePlacementStateValue.Open, st), RolePlacementEvent.Assigned (mid,rrid) -> { st with State=Assigned(mid,rrid) }
    | MatchStateValue (RolePlacementStateValue.Assigned _, st) , RolePlacementEvent.Unassigned -> { st with State=Open }
    | MatchStateValue (RolePlacementStateValue.Assigned (mid,rrid), st), RolePlacementEvent.Completed -> { st with State=Complete(mid,rrid) }
    | MatchStateValue (RolePlacementStateValue.Complete _, _), _-> failwith "Once complete, the placement cannot be altered"
    | MatchStateValue (RolePlacementStateValue.Assigned (_, _), st), RolePlacementEvent.Reassigned (mid, rrid) -> { st with State=Assigned (mid, rrid) }
    | MatchStateValue (RolePlacementStateValue.Open _, st), RolePlacementEvent.Canceled -> { st with State=Canceled }
    | MatchStateValue (RolePlacementStateValue.Assigned _, st), RolePlacementEvent.Canceled -> { st with State=Canceled }
    | _, RolePlacementEvent.Assigned _ -> failwith "Can only assign to an open slot"
    | _, RolePlacementEvent.Completed -> failwith "Can only complete an assigned position"
    | _, RolePlacementEvent.Unassigned -> failwith "Can only unassign an assigned position"
    | _, RolePlacementEvent.Opened _ -> failwith "Cannot open an open position"
    | _, RolePlacementEvent.Reassigned _ -> failwith "Cannot reassign a role that has not been assigned"
    | _, RolePlacementEvent.Canceled _ -> failwith "Cannot cancel a role if does not exist or is complete"
