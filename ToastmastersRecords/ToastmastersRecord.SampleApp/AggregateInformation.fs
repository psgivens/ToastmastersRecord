module ToastmastersRecord.SampleApp.AggregateInformation

open System
open FSharp.Data

open ToastmastersRecord.Domain
open ToastmastersRecord.Domain.Infrastructure
open ToastmastersRecord.Domain.DomainTypes
open ToastmastersRecord.Domain.MemberManagement

let calculateHistory system userId date actorGroups = 
    let meeting = Persistence.ClubMeetings.findByDate date
    Persistence.MemberManagement.getMemberHistories ()
    |> Seq.map (fun (clubMember, history) ->
        let historyState = 
            history.Id 
            |> Persistence.RolePlacements.getRolePlacmentsByMember
//            |> Seq.where (fun (d, p) -> d < date && p.State = 2)
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

    |> Seq.map (fun (historyId, state) ->
        let absent, specialRequest = Persistence.RoleRequests.getMemberRequest userId historyId (MeetingId.box meeting.Id)
        
        historyId, {
            state with
                MemberHistoryState.WillAttend = not absent
                MemberHistoryState.SpecialRequest = specialRequest })
    
    |> Seq.iter (fun (historyId, state) -> state |> Persistence.MemberManagement.persistHistory userId historyId)

let interpret = interpret "NA" "MM/dd/yyyy"
let generateMessagesToMembers system userId actorGroups =
    use writer = new System.IO.StreamWriter "C:\Users\Phillip Givens\OneDrive\Toastmasters\messages.txt"
    Persistence.MemberManagement.getMemberHistories ()
    |> Seq.where (fun (m,h) -> m.Awards |> System.String.IsNullOrWhiteSpace |> not &&
                               h.ConfirmedSpeechCount = 0)
    |> Seq.iter (fun (m, h) ->
        sprintf """
-------------------------
%s

Hello %s,

I would like to finish this term strong with accurate data about
our members and where they are in their journey. Please let me 
know if you know how many speeches you've accomplished toward 
your next award or when you held a major role. 

Toastmaster ID: %d
Awards: %s
Speeches toward next award: %s
Last Toastmaster: %s
Last Table Topics Master Role: %s
Last General Evaluator Spot Held: %s
Last Speech Given: %s
Last Evaluation: %s
Last Opening Thought, Closing Thought or Joke Master: %s
Last Functionary Role: %s

Please also let me know if you plan on continuing toward the 
old award system or if you are going to start fresh with 
Pathways. 

Thanks,
Phillip Scott Givens, CC
Vice President of Education
Toastmasters - Santa Monica Club 21
            """ 
                m.Email
                h.DisplayName 
                m.ToastmasterId 
                (if m.Awards |> System.String.IsNullOrWhiteSpace 
                    then "None" 
                    else m.Awards)
                (h.CalculatedSpeechCount.ToString ())
                (interpret h.DateAsToastmaster)
                (interpret h.DateAsTableTopicsMaster)
                (interpret h.DateAsGeneralEvaluator)
                (interpret h.DateOfLastSpeech)
                (interpret h.DateOfLastEvaluation)
                (interpret h.DateOfLastMinorRole)
                (interpret h.DateOfLastFunctionaryRole)
        |> writer.Write
        ())

(* Creating a CSV file *)
// prepare a string for writing to CSV  
let prepareStr obj =
    obj.ToString()
     .Replace("\"","\"\"") // replace single with double quotes
     |> sprintf "\"%s\""   // surround with quotes

// convert a list of strings to a CSV
let listToCsv list =
    let combine s1 s2 = s1 + "," + s2
    list 
    |> Seq.map prepareStr 
    |> Seq.reduce combine 

type MemHistCsvType = 
    CsvProvider<
        Schema = "Status (string), Attend (string), Name (string), Eligibility (string), Last Speech (string), Last Evaluation (string), Last TM (string), Last TTM (string), Last GE (string), Last Major (string), Last Minor (string), Last Functionary (string), Last Facilitator (string), Last Role (string), Speech Count (string), Requests (string)",
        HasHeaders=false>

let generateStatistics system userId actorGroups =    
    let fileName = "C:\Users\Phillip Givens\OneDrive\Toastmasters\HistoryStats3.csv"
    let histories = Persistence.MemberManagement.getMemberHistories ()
    let csvFile = new MemHistCsvType ([("Status", "Attend", "Name", "Eligibility", "Last Speech", "Last Evaluation", "Last TM", "Last TTM", "Last GE", "Last Major", "Last Minor", "Last Functionary", "Last Facilitator", "Last Role", "Speech Count", "Requests") |> MemHistCsvType.Row])

    let csv = 
        histories
        |> List.map (fun (m,h) -> 
            m.PaidStatus,
            h.WillAttend.ToString (),
            h.DisplayName, 
            h.EligibilityCount.ToString (),
            interpret h.DateOfLastSpeech,
            interpret h.DateOfLastEvaluation,
            interpret h.DateAsToastmaster,
            interpret h.DateAsTableTopicsMaster,
            interpret h.DateAsGeneralEvaluator,
            interpret h.DateOfLastMajorRole,
            interpret h.DateOfLastMinorRole,
            interpret h.DateOfLastFunctionaryRole,
            interpret h.DateOfLastFacilitatorRole,
            interpret h.DateOfLastRole,
            h.CalculatedSpeechCount.ToString (),            
            h.SpecialRequest
            )
        |> Seq.map MemHistCsvType.Row
        |> csvFile.Append
    csv.Save fileName



