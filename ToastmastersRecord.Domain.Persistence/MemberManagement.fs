module ToastmastersRecord.Domain.Persistence.MemberManagement

open ToastmastersRecord.Data
open ToastmastersRecord.Domain.Infrastructure
open ToastmastersRecord.Domain.DomainTypes
open ToastmastersRecord.Domain.MemberManagement
open ToastmastersRecord.Data.Entities

open Newtonsoft.Json
open System.Data.Entity

let defaultDT = "1/1/1900" |> System.DateTime.Parse
let persist (userId:UserId) (streamId:StreamId) (state:MemberManagementState option) =
    use context = new ToastmastersEFDbContext () 
    let entity = context.Members.Find (StreamId.unbox streamId)
    match entity, state with
    | null, Option.None -> ()
    | null, Option.Some(item) -> 
        let details = item.Details
        context.Members.Add (
            MemberEntity (
                Id = StreamId.unbox streamId,
                IsActive = (details.PaidStatus = "paid"),
                ToastmasterId = TMMemberId.unbox details.ToastmasterId,
                Name = details.Name,
                Awards=details.Awards,
                Email=details.Email,
                HomePhone=details.HomePhone,
                MobilePhone=details.MobilePhone,
                PaidUntil=details.PaidUntil,
                ClubMemberSince=details.ClubMemberSince,
                OriginalJoinDate=details.OriginalJoinDate,
                PaidStatus=details.PaidStatus,
                CurrentPosition=details.CurrentPosition                
                )) |> ignore
        context.MemberHistories.Add (
            MemberHistoryAggregate (
                Id = StreamId.unbox streamId,
                DisplayName=details.DisplayName,
                SpeechCountConfirmedDate = details.SpeechCountConfirmedDate,
                ConfirmedSpeechCount = 0,
                AggregateCalculationDate = defaultDT,
                CalculatedSpeechCount = 0,
                DateAsToastmaster = defaultDT,
                DateAsGeneralEvaluator = defaultDT,
                DateAsTableTopicsMaster = defaultDT,
                DateOfLastSpeech = defaultDT,
                DateOfLastEvaluation = defaultDT,
                DateOfLastMajorRole = defaultDT,
                DateOfLastMinorRole = defaultDT,
                DateOfLastFacilitatorRole = defaultDT,
                DateOfLastFunctionaryRole = defaultDT,
                DateOfLastRole = defaultDT,
                EligibilityCount = if System.String.IsNullOrWhiteSpace details.Awards then 0 else 5
            )
         ) |> ignore
        printfn "Persist: (%A, %A, %A)" details.DisplayName details.Awards details.ClubMemberSince
    | _, Option.None -> context.Members.Remove entity |> ignore        
    | _, Some(item) -> 
        let details = item.Details
        entity.Name <- details.Name
        entity.IsActive <- (details.PaidStatus = "paid")
        entity.ToastmasterId <- TMMemberId.unbox details.ToastmasterId
        entity.Name <- details.Name
        entity.Awards <- details.Awards
        entity.Email <- details.Email
        entity.HomePhone <- details.HomePhone
        entity.MobilePhone <- details.MobilePhone
        entity.PaidUntil <- details.PaidUntil
        entity.ClubMemberSince <- details.ClubMemberSince
        entity.OriginalJoinDate <- details.OriginalJoinDate
        entity.PaidStatus <- details.PaidStatus
        entity.CurrentPosition <- details.CurrentPosition                
    context.SaveChanges () |> ignore
    
let persistHistory (userId:UserId) (streamId:StreamId) (state:MemberHistoryState) =
    use context = new ToastmastersEFDbContext () 
    let entity = context.MemberHistories.Find (StreamId.unbox streamId)
    entity.AggregateCalculationDate <- System.DateTime.Now.Date
    entity.CalculatedSpeechCount <- state.SpeechCount
    entity.DateAsToastmaster <- state.LastToastmaster
    entity.DateAsTableTopicsMaster <- state.LastTableTopicsMaster
    entity.DateAsGeneralEvaluator <- state.LastGeneralEvaluator
    entity.DateOfLastSpeech <- state.LastSpeechGiven
    entity.DateOfLastEvaluation <- state.LastEvaluationGiven    
    entity.DateOfLastMinorRole <- state.LastMinorRole
    entity.DateOfLastMajorRole <- state.LastMajorRole
    entity.DateOfLastFunctionaryRole <- state.LastFunctionaryRole
    entity.DateOfLastFacilitatorRole <- state.LastFacilitatorRole    
    entity.DateOfLastRole <- state.LastAssignment
    entity.WillAttend <- state.WillAttend
    entity.SpecialRequest <- state.SpecialRequest
    entity.EligibilityCount <- state.EligibilityCount    
    context.SaveChanges () |> ignore

let persistConfirmation (userId:UserId) (streamId:StreamId) (env:Envelope<MemberHistoryConfirmation> option) =
    use context = new ToastmastersEFDbContext () 
    let envelope = env |> Option.get
    let confirmation = envelope.Item
    let entity = context.MemberHistories.Find (StreamId.unbox streamId)
    entity.AggregateCalculationDate <- System.DateTime.Now.Date
    entity.ConfirmedSpeechCount <- confirmation.SpeechCount
    entity.EligibilityCount <- System.Math.Max (confirmation.SpeechCount, entity.EligibilityCount)
    entity.SpeechCountConfirmedDate <- confirmation.ConfirmationDate
    entity.DisplayName <- confirmation.DisplayName
    context.SaveChanges () |> ignore

let execQuery (q:ToastmastersEFDbContext -> System.Linq.IQueryable<'a>) =
    use context = new ToastmastersEFDbContext () 
    q context
    |> Seq.toList

let find (userId:UserId) (streamId:StreamId) =
    use context = new ToastmastersEFDbContext () 
    context.Members.Find (StreamId.unbox streamId)

let findHistory (userId:UserId) (streamId:StreamId) =
    use context = new ToastmastersEFDbContext () 
    context.MemberHistories.Find (StreamId.unbox streamId)

let findMemberByName name =
    use context = new ToastmastersEFDbContext () 
    query { for clubMember in context.Members do            
            where (clubMember.Name = name)
            select clubMember
            exactlyOne }

let findMemberByDisplayName name =
    use context = new ToastmastersEFDbContext () 
    query { for history in context.MemberHistories do
            join clubMember in context.Members 
                on (history.Id = clubMember.Id)
            where (history.DisplayName = name)
            select clubMember
            exactlyOne }

let findMemberByToastmasterId (TMMemberId.Val ident) =
    use context = new ToastmastersEFDbContext () 
    query { for clubMember in context.Members do
            where (clubMember.ToastmasterId = ident)
            select clubMember
            exactlyOneOrDefault }

let getMemberHistory id =
    use context = new ToastmastersEFDbContext () 
    query { for history in context.MemberHistories do
            where (history.Id = id)
            select history
            exactlyOneOrDefault }


//let findMemberHistoryByDisplayName name =
//    use context = new ToastmastersEFDbContext () 
//    query { for clubMember in context.Members do
//            leftOuterJoin history in context.MemberHistories    
//                on (clubMember.Id = history.Id) into result
//            where (clubMember.DisplayName = name)
//            for history in result do
//            select history
//            exactlyOne }


let getMemberHistories () =
    use context = new ToastmastersEFDbContext ()
    query { for clubMember in context.Members do
            leftOuterJoin history in context.MemberHistories 
                on (clubMember.Id = history.Id) into result
            for history in result  do            
            select (clubMember, history)
        } 
        |> Seq.where (fun (_, history) -> not <| obj.ReferenceEquals (history, null))
        |> Seq.toList 