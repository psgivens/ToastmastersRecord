module ToastmastersRecord.SampleApp.IngestMembers

open System
open FSharp.Data
open Akka.Actor
open Akka.FSharp

open ToastmastersRecord.Domain
open ToastmastersRecord.Domain.Infrastructure
open ToastmastersRecord.Domain.DomainTypes
open ToastmastersRecord.Domain.MemberManagement
open ToastmastersRecord.Actors
open ToastmastersRecord.SampleApp.Infrastructure
open ToastmastersRecord.SampleApp.Initialize

let ingestMembers system userId actorGroups (fileName:string) =
    let memberRequestReply = RequestReplyActor.spawnRequestReplyActor<MemberManagementCommand,MemberManagementEvent> system "memberManagement" actorGroups.MemberManagementActors
    
    let roster = CsvFile.Load(fileName).Cache()
    
    // Map CSV rows to Member Details
    roster.Rows 
    |> Seq.map (fun row ->
        let recordName = row.GetColumn "Name"
        let commaIndex = recordName.IndexOf ','
        let name, awards = 
            if commaIndex = -1 then recordName, ""
            else recordName.Substring (0, commaIndex), commaIndex + 2 |> recordName.Substring 
        let toastmasterId = 
            row.GetColumn "Customer ID"
            |> System.Int32.Parse 
            |> TMMemberId.box;

        {   MemberDetails.ToastmasterId = toastmasterId
            Name = name
            DisplayName = name
            Awards = awards
            Email= row.GetColumn "Email";
            HomePhone = row.GetColumn "Home Phone";
            MobilePhone= row.GetColumn "Mobile Phone";               
            PaidUntil = row.GetColumn "Paid Until" |> System.DateTime.Parse;
            ClubMemberSince = row.GetColumn "Member of Club Since" |> System.DateTime.Parse;
            OriginalJoinDate = row.GetColumn "Original Join Date" |> System.DateTime.Parse;
            SpeechCountConfirmedDate = row.GetColumn "Original Join Date" |> System.DateTime.Parse;
            PaidStatus = row.GetColumn "status (*)";
            CurrentPosition = row.GetColumn "Current Position";
            })

    // Use the member details to send envelope with command to actor and wait for reply
    |> Seq.map (fun memberDetails -> 
        async {
            printfn "Send: (%s, %s, %d)" 
                memberDetails.DisplayName
                memberDetails.Awards
                (TMMemberId.unbox memberDetails.ToastmasterId)

            let clubMember = Persistence.MemberManagement.findMemberByToastmasterId memberDetails.ToastmasterId
            let streamId, cmd = 
                match clubMember with
                | null -> StreamId.create (), MemberManagementCommand.Create
                | _ -> clubMember.Id |> StreamId.box, MemberManagementCommand.Update
            do! memberDetails
                |> cmd
                |> envelopWithDefaults
                    (userId)
                    (TransId.create ())
                    (streamId)
                    (Version.box 0s)
                |> memberRequestReply.Ask
                |> Async.AwaitTask
                |> Async.Ignore

            printfn "Created: (%s, %s, %d)" 
                memberDetails.DisplayName
                memberDetails.Awards
                (TMMemberId.unbox memberDetails.ToastmasterId)
        })
    |> Async.Parallel
    |> Async.Ignore
    |> Async.RunSynchronously

    // Unsubscribe and stop the actor
    memberRequestReply <! "Unsubscribe"

let ingestSpeechCount system userId (fileName:string) (actorGroups:ActorGroups) = 
    let roster = CsvFile.Load(fileName).Cache()
    
    roster.Rows 
    |> Seq.iter (fun row ->
        printfn "Count: (%s, %s, %s)" 
            (row.GetColumn "Name") (row.GetColumn "Count") (row.GetColumn "Date"))

    let historyRequestReplyCanceled = 
        RequestReplyActor.spawnRequestReplyActor<MemberHistoryConfirmation,unit> 
            system "history_countconfirmed" actorGroups.MemberHistoryActors

    roster.Rows    
    |> Seq.map (fun row -> 
        let refDate : System.DateTime ref = ref DateTime.MinValue
        let defaultDate = "1900/1/1" |> System.DateTime.Parse
        let objNext = if DateTime.TryParse (row.GetColumn "ObjNext", refDate)
                        then !refDate
                        else defaultDate
        let date = if DateTime.TryParse (row.GetColumn "Date", refDate)
                        then !refDate
                        else defaultDate
        let intRef : int ref = ref 0
        let count = if System.Int32.TryParse(row.GetColumn "Count", intRef) then !intRef else 0
        row.GetColumn "Customer ID" |> System.Int32.Parse |> TMMemberId.box, 
        row.GetColumn "Name", 
        row.GetColumn "Display Name",
        objNext,
        count,
        date)
    |> Seq.map (fun (tmid, name, displayName, objNext, count, date) ->        
        let clubMember = Persistence.MemberManagement.findMemberByToastmasterId tmid
        
        clubMember.Id |> StreamId.box,
        {   MemberHistoryConfirmation.SpeechCount = count
            MemberHistoryConfirmation.ConfirmationDate = date
            MemberHistoryConfirmation.DisplayName = displayName
             })
    |> Seq.map (fun (id, confirmation) ->
        confirmation
        |> envelopWithDefaults
            userId
            (TransId.create ())
            id
            (Version.box 0s)
        |> historyRequestReplyCanceled.Ask
        |> Async.AwaitTask)

    // Wait for completion of all meeting creations
    |> Async.Parallel
    |> Async.Ignore
    |> Async.RunSynchronously


