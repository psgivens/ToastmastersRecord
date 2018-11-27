module ToastmastersRecord.SampleApp.IngestMessages

open System
open FSharp.Data
open Akka.Actor
open Akka.FSharp

open ToastmastersRecord.Actors

open ToastmastersRecord.Domain
open ToastmastersRecord.Domain.Infrastructure
open ToastmastersRecord.Domain.DomainTypes
open ToastmastersRecord.Domain.RoleRequests
open ToastmastersRecord.Domain.MemberMessages
open ToastmastersRecord.SampleApp.Infrastructure
open ToastmastersRecord.SampleApp.Initialize

let messagesFileName = "C:\Users\Phillip Givens\OneDrive\Toastmasters\RoleRequestMessagesId.txt"

let absentsFileName = "C:\Users\Phillip Givens\OneDrive\Toastmasters\RequestOff.csv"
let requestFileName = "C:\Users\Phillip Givens\OneDrive\Toastmasters\RequestOn.csv"

type MessagesCsvType = 
    CsvProvider<
        Schema = "Message Id (string), Name (string), Date (string), Message (string)", 
        Separators = "\t",
        HasHeaders=false>

type DayOffCsvType =
    CsvProvider<
        Schema = "Meeting Id (string), Message Id (string), Name (string)", 
        Separators = "\t",
        HasHeaders=false>

type RoleRequestCsvType =
    CsvProvider<
        Schema = "Meeting Id (string), Message Id (string), Name (string), Description (string)", 
        Separators = "\t",
        HasHeaders=false>

let unpackMeetingIdsFromRequest (midsString:string) =
    midsString.Split ';'
    |> Array.toList
    |> List.map (fun idstr -> idstr |> Guid.Parse |> MeetingId.box)
         
let ingestMemberMessages system userId (actorGroups:ActorGroups) = 

    let memberMessagesFile = MessagesCsvType.Load (messagesFileName)

    let messageRequestReply = 
        RequestReplyActor.spawnRequestReplyActor<MemberMessageCommand,unit> 
            system "memberMessage_ingest" actorGroups.MessageActors
    
    memberMessagesFile.Rows 
    |> Seq.iter (fun row ->
        printfn "Count: (%s, %s, %s)" 
            (row.Name) (row.Date) (row.Message))

    memberMessagesFile.Rows 
    |> Seq.map (fun row ->
        (row.``Message Id`` |> Guid.Parse |> MessageId.box),
        (row.Name), 
        (row.Date |> DateTime.Parse), 
        (row.Message))
    |> Seq.map (fun (msgid, name, date, message) ->
        let mbrid = Persistence.MemberManagement.findMemberByDisplayName name
        (mbrid.Id |> MemberId.box, date, message)
        |> MemberMessageCommand.Create 
        |> envelopWithDefaults
            (userId) 
            (TransId.create ()) 
            (msgid |> MessageId.unbox |> StreamId.box) 
            (Version.box 0s) 
        |> messageRequestReply.Ask
        |> Async.AwaitTask)

    // Wait for completion of all meeting creations
    |> Async.Parallel
    |> Async.Ignore
    |> Async.RunSynchronously

    messageRequestReply <! "Unsubscribe"

let ingestDaysOff system userId (actorGroups:ActorGroups) = 

    let dayOffsFile = DayOffCsvType.Load (absentsFileName)
    
    let dayOffsRequestReply = 
        RequestReplyActor.spawnRequestReplyActor<DayOffRequestCommand,unit> 
            system "dayOffRequest_ingest" actorGroups.DayOffActors

    dayOffsFile.Rows
    |> Seq.map (fun row ->
        (row.``Message Id`` |> Guid.Parse |> MessageId.box),
        (row.``Meeting Id`` |> Guid.Parse |> MeetingId.box),
        (row.Name))
    |> Seq.map (fun (messageId, meetingId, name) ->
        let mbr = Persistence.MemberManagement.findMemberByDisplayName name
        (mbr.Id |> MemberId.box, meetingId, messageId)
        |> DayOffRequestCommand.Create
        |> envelopWithDefaults
            (userId)
            (TransId.create ())
            (StreamId.create ())
            (Version.box 0s)
        |> dayOffsRequestReply .Ask
        |> Async.AwaitTask)
    |> Async.Parallel
    |> Async.Ignore
    |> Async.RunSynchronously

    dayOffsRequestReply <! "Unsubscribe"


let ingestRoleRequests system userId (actorGroups:ActorGroups) = 

    let roleRequestsFile = RoleRequestCsvType.Load (requestFileName )
    
    let roleRequestsRequestReplyCreated = 
        RequestReplyActor.spawnRequestReplyActor<RoleRequestCommand,RoleRequestEvent> 
            system "RoleRequest_ingest" actorGroups.RoleRequestActors

    let x = roleRequestsFile.Rows |> Seq.toList

    roleRequestsFile.Rows
    |> Seq.map (fun row ->
        (row.``Message Id`` |> Guid.Parse |> MessageId.box),
        (row.``Meeting Id`` |> unpackMeetingIdsFromRequest),
        (row.Name),
        (row.Description))
    |> Seq.map (fun (messageId, meetingIds, name, description) ->
        let mbr = Persistence.MemberManagement.findMemberByDisplayName name
        (mbr.Id |> MemberId.box, messageId, description, meetingIds)
        //MemberId * MessageId * RequestAbbreviation * DateTime list
        |> RoleRequestCommand.Request 
        |> envelopWithDefaults
            (userId)
            (TransId.create ())
            (StreamId.create ())
            (Version.box 0s)
        |> roleRequestsRequestReplyCreated .Ask
        |> Async.AwaitTask)
    |> Async.Parallel
    |> Async.Ignore
    |> Async.RunSynchronously

    roleRequestsRequestReplyCreated <! "Unsubscribe"
