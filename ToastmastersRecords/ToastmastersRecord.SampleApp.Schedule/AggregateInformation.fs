module ToastmastersRecord.SampleApp.Schedule.AggregateInformation

open System
open FSharp.Data

open ToastmastersRecord.Domain
open ToastmastersRecord.Domain.Infrastructure
open ToastmastersRecord.Domain.DomainTypes
open ToastmastersRecord.Domain.MemberManagement

let calculateHistory system userId actorGroups = 
    Persistence.MemberManagement.getMemberHistories ()
    |> Seq.map (fun (clubMember, history) ->
        let historyState = 
            history.Id 
            |> Persistence.RolePlacements.getRolePlacmentsByMember
            |> Seq.fold (fun state (date,placement) -> 
                match placement.RoleTypeId |> enum<RoleTypeId> with
                | RoleTypeId.Speaker ->           
                    if date <= history.SpeechCountConfirmedDate then state
                    else { state with 
                            MemberHistoryState.SpeechCount = state.SpeechCount + 1 
                            MemberHistoryState.LastSpeechGiven = date 
                            MemberHistoryState.EligibilityCount = 
                                if clubMember.Awards |> String.IsNullOrWhiteSpace then state.SpeechCount + 1
                                else 5 
                            MemberHistoryState.LastMajorRole = date
                            MemberHistoryState.LastAssignment = date }
                | RoleTypeId.Evaluator         -> 
                    { state with 
                        MemberHistoryState.LastEvaluationGiven = date 
                        MemberHistoryState.LastMajorRole = date
                        MemberHistoryState.LastAssignment = date }
                | RoleTypeId.Toastmaster ->       
                    { state with 
                        MemberHistoryState.LastToastmaster = date
                        MemberHistoryState.LastFacilitatorRole = date
                        MemberHistoryState.LastAssignment = date }
                | RoleTypeId.TableTopicsMaster -> 
                    { state with 
                        MemberHistoryState.LastTableTopicsMaster = date 
                        MemberHistoryState.LastFacilitatorRole = date
                        MemberHistoryState.LastAssignment = date }
                | RoleTypeId.GeneralEvaluator  -> 
                    { state with 
                        MemberHistoryState.LastGeneralEvaluator = date 
                        MemberHistoryState.LastFacilitatorRole = date
                        MemberHistoryState.LastAssignment = date }
                | RoleTypeId.OpeningThoughtAndBallotCounter  
                | RoleTypeId.ClosingThoughtAndGreeter
                | RoleTypeId.JokeMaster ->
                    { state with 
                        MemberHistoryState.LastMinorRole = date
                        MemberHistoryState.LastAssignment = date }
                | RoleTypeId.Timer
                | RoleTypeId.Grammarian
                | RoleTypeId.Videographer
                | RoleTypeId.ErAhCounter ->
                    { state with
                        MemberHistoryState.LastFunctionaryRole = date
                        MemberHistoryState.LastAssignment = date }
                | _ -> failwith "Unsupported RoleTypeId"
                ) {
                    MemberHistoryState.SpeechCount = history.ConfirmedSpeechCount
                    MemberHistoryState.LastToastmaster = history.DateAsToastmaster
                    MemberHistoryState.LastTableTopicsMaster = history.DateAsTableTopicsMaster
                    MemberHistoryState.LastGeneralEvaluator = history.DateAsGeneralEvaluator
                    MemberHistoryState.LastSpeechGiven = history.DateOfLastSpeech
                    MemberHistoryState.LastEvaluationGiven = history.DateOfLastEvaluation
                    MemberHistoryState.WillAttend = history.WillAttend
                    MemberHistoryState.SpecialRequest = history.SpecialRequest
                    MemberHistoryState.EligibilityCount = history.EligibilityCount
                    MemberHistoryState.LastMinorRole = history.DateOfLastMinorRole
                    MemberHistoryState.LastMajorRole = history.DateOfLastMajorRole
                    MemberHistoryState.LastFunctionaryRole = history.DateOfLastFunctionaryRole
                    MemberHistoryState.LastFacilitatorRole = history.DateOfLastFacilitatorRole
                    MemberHistoryState.LastAssignment = history.DateOfLastRole
                }
        (StreamId.box history.Id, historyState))

//    |> Seq.map (fun (historyId, state) ->
//        let absent, specialRequest = Persistence.RoleRequests.getMemberRequest userId historyId (MeetingId.box meeting.Id)
//        
//        historyId, {
//            state with
//                MemberHistoryState.WillAttend = not absent
//                MemberHistoryState.SpecialRequest = specialRequest })
    
    |> Seq.iter (fun (historyId, state) -> state |> Persistence.MemberManagement.persistHistory userId historyId)


