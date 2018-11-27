module ToastmastersRecord.Domain.ClubMeetings

open ToastmastersRecord.Domain.CommandHandlers
open ToastmastersRecord.Domain.Infrastructure
open ToastmastersRecord.Domain.DomainTypes

type ClubMeetingCommand =
    | Create of System.DateTime
    | Cancel
    | Occur

type ClubMeetingEvent =
    | Created of System.DateTime
    | Initialized
    | Canceling
    | Canceled
    | Occurred

type ClubMeetingStateValue =
    | Initializing = 0
    | Pending = 10
    | Canceling = 20
    | Canceled = 30
    | Occurred = 40

type ClubMeetingState = { State:ClubMeetingStateValue; Date:System.DateTime }

let (|MatchStateValue|_|) state =
    match state with 
    | Some(value) -> Some(value.State, value)
    | _ -> None 

type RoleActions = { 
    createRole: Envelope<ClubMeetingCommand> -> RoleTypeId -> Async<obj>
    cancelRoles: Envelope<ClubMeetingCommand> -> Async<unit>
    }

let handle         
        (roleActions:RoleActions) 
        (command:CommandHandlers<ClubMeetingEvent, Version>) 
        (state:ClubMeetingState option) 
        (cmdenv:Envelope<ClubMeetingCommand>) =

    let createMeeting date =
        command.block {
            do! ClubMeetingEvent.Created date |> Handler.Raise 
            return async {

                // Define a meeting
                do! [RoleTypeId.Toastmaster
                     RoleTypeId.TableTopicsMaster
                     RoleTypeId.GeneralEvaluator
                     RoleTypeId.Evaluator
                     RoleTypeId.Evaluator
                     RoleTypeId.Evaluator
                     RoleTypeId.Speaker
                     RoleTypeId.Speaker
                     RoleTypeId.Speaker
                     RoleTypeId.OpeningThoughtAndBallotCounter
                     RoleTypeId.ClosingThoughtAndGreeter
                     RoleTypeId.JokeMaster
                     RoleTypeId.ErAhCounter
                     RoleTypeId.Grammarian
                     RoleTypeId.Timer
                     RoleTypeId.Videographer ] 
                    |> List.map (fun roleId -> roleId |> roleActions.createRole cmdenv) 
                    |> Async.Parallel
                    |> Async.Ignore
                    
                return ClubMeetingEvent.Initialized    
            }
        }

    let cancelMeeting () = 
        command.block {
            do! ClubMeetingEvent.Canceling |> Handler.Raise

            return async {
                do! roleActions.cancelRoles cmdenv 
            
                return ClubMeetingEvent.Canceled
            }
        } 

    match state, cmdenv.Item with
    | None, ClubMeetingCommand.Create date -> createMeeting date
    | MatchStateValue (ClubMeetingStateValue.Pending, _), ClubMeetingCommand.Cancel -> cancelMeeting ()
    | MatchStateValue (ClubMeetingStateValue.Pending, _), ClubMeetingCommand.Occur -> command.event <| ClubMeetingEvent.Occurred
    | None, _ -> failwith "A meeting must first be created to cancel or occur"
    | Some _, ClubMeetingCommand.Create _ -> failwith "cannot create a meeting which already exists"
    | MatchStateValue (ClubMeetingStateValue.Occurred, _), _ -> failwith "Occurred is an ending state"
    | MatchStateValue (ClubMeetingStateValue.Canceled, _), _ -> failwith "Canceled is an ending state"
    | _, _ -> failwith "Unexpected state/command combination"

let evolve (state:ClubMeetingState option) (event:ClubMeetingEvent) =
    match state, event with
    | None, ClubMeetingEvent.Created date -> { State=ClubMeetingStateValue.Initializing; Date=date }
    | MatchStateValue (ClubMeetingStateValue.Initializing, s), ClubMeetingEvent.Initialized -> { s with State=ClubMeetingStateValue.Pending }
    | MatchStateValue (ClubMeetingStateValue.Pending, s), ClubMeetingEvent.Canceling -> { s with State=ClubMeetingStateValue.Canceling }
    | MatchStateValue (ClubMeetingStateValue.Canceling, s), ClubMeetingEvent.Canceled -> { s with State=ClubMeetingStateValue.Canceled }
    | MatchStateValue (ClubMeetingStateValue.Pending, s), ClubMeetingEvent.Occurred -> { s with State=ClubMeetingStateValue.Occurred }
    | None, _ -> failwith "A meeting must first be created to cancel or occur"
    | Some _, ClubMeetingEvent.Created _ -> failwith "cannot create a meeting which already exists"
    | MatchStateValue (ClubMeetingStateValue.Occurred, _), _ -> failwith "Occurred is an ending state"
    | MatchStateValue (ClubMeetingStateValue.Canceled, _), _ -> failwith "Canceled is an ending state"
    | _, _ -> failwith "Unexpected state/command combination"
