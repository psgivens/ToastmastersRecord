module ToastmastersRecord.Domain.DomainTypes
open ToastmastersRecord.Domain.Infrastructure

type MemberMessage = string
type MessageId = FsGuidType
type RequestAbbreviation = string
type MemberId = FsGuidType
type TMMemberId = FsType<int>
type MeetingId = FsGuidType
type RoleRequestId = FsGuidType

// These values are mirrored in the database
type RoleTypeId = 
    | Toastmaster = 1
    | TableTopicsMaster = 2
    | GeneralEvaluator = 3    
    | Speaker = 4
    | Evaluator = 5    
    | JokeMaster = 6
    | OpeningThoughtAndBallotCounter = 7
    | ClosingThoughtAndGreeter = 8    
    | Grammarian = 9
    | ErAhCounter = 10    
    | Timer = 11
    | Videographer = 12

let category = 
    function
    | RoleTypeId.Toastmaster
    | RoleTypeId.GeneralEvaluator
    | RoleTypeId.TableTopicsMaster -> "Facilitator"
    | RoleTypeId.Evaluator
    | RoleTypeId.Speaker -> "Major"
    | RoleTypeId.OpeningThoughtAndBallotCounter 
    | RoleTypeId.ClosingThoughtAndGreeter 
    | RoleTypeId.JokeMaster -> "Minor"
    | RoleTypeId.ErAhCounter
    | RoleTypeId.Grammarian
    | RoleTypeId.Timer
    | RoleTypeId.Videographer -> "Functionary"
    | _ -> "other"


