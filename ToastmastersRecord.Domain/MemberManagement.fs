module ToastmastersRecord.Domain.MemberManagement

open System
open ToastmastersRecord.Domain.CommandHandlers
open ToastmastersRecord.Domain.Infrastructure
open ToastmastersRecord.Domain.DomainTypes


type MemberDetails = { 
    Name:string; 
    DisplayName:string; 
    Awards:string;
    ToastmasterId:TMMemberId; 
    Email:string; 
    HomePhone:string; 
    MobilePhone:string;
    PaidUntil:DateTime;
    ClubMemberSince:DateTime;
    OriginalJoinDate:DateTime;
    PaidStatus:string;
    CurrentPosition:string;
    SpeechCountConfirmedDate:DateTime;
    }

type MemberManagementCommand =
    | Create of MemberDetails
    | Activate 
    | Deactivate 
    | Update of MemberDetails

type MemberManagementEvent = 
    | Created of MemberDetails
    | Activated
    | Deactivated
    | Updated of MemberDetails

type MemberManagementStateValue =
    | Active
    | Inactive

type MemberManagementState =
    { State:MemberManagementStateValue; Details:MemberDetails}

type MemberHistoryState = {
    SpeechCount:int
    LastToastmaster:System.DateTime
    LastTableTopicsMaster:System.DateTime
    LastGeneralEvaluator:System.DateTime
    LastSpeechGiven:System.DateTime
    LastEvaluationGiven:System.DateTime
    LastAssignment:DateTime
    LastMinorRole:DateTime
    LastMajorRole:DateTime
    LastFunctionaryRole:DateTime
    LastFacilitatorRole:DateTime
    WillAttend:bool
    SpecialRequest:string
    EligibilityCount:int
    }

type MemberHistoryConfirmation = {
    SpeechCount:int
    ConfirmationDate:System.DateTime
    DisplayName:string
    }        

let (|HasStateValue|_|) expected state =
    match state with 
    | Some(value) when value.State = expected -> Some value
    | _ -> None 

let handle (command:CommandHandlers<MemberManagementEvent, Version>) (state:MemberManagementState option) (cmdenv:Envelope<MemberManagementCommand>) =    
    match state, cmdenv.Item with 
    | None, Create user -> Created user
    | _, Create _ -> failwith "Cannot create a user which already exists"
    | HasStateValue MemberManagementStateValue.Inactive _, MemberManagementCommand.Activate -> MemberManagementEvent.Activated
    | _, MemberManagementCommand.Activate -> failwith "Member must exist and be inactive to activate"
    | HasStateValue MemberManagementStateValue.Active _, MemberManagementCommand.Deactivate -> MemberManagementEvent.Deactivated
    | _, MemberManagementCommand.Deactivate -> failwith "Member must exist and be active to deactivate"
    | Some _, MemberManagementCommand.Update details -> MemberManagementEvent.Updated details
    | None, MemberManagementCommand.Update _ -> failwith "Cannot update a user which does not exist"             
    |> command.event

let evolve (state:MemberManagementState option) (event:MemberManagementEvent) =
    match state, event with 
    | None, MemberManagementEvent.Created user -> { State=Active; Details=user }
    | HasStateValue MemberManagementStateValue.Inactive st, MemberManagementEvent.Activated -> { st with State=Active }
    | HasStateValue MemberManagementStateValue.Active st, MemberManagementEvent.Deactivated -> { st with State=Inactive }
    | Some st, MemberManagementEvent.Updated details -> { st with Details=details }
    | _, Created _ -> failwith "Cannot create a user which already exists"
    | _, MemberManagementEvent.Activated -> failwith "Member must exist and be inactive to activate"
    | _, MemberManagementEvent.Deactivated -> failwith "Member must exist and be active to deactivate"    
    | None, MemberManagementEvent.Updated _ -> failwith "Cannot update a user which does not exist"

