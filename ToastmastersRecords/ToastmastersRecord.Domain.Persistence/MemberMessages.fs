module ToastmastersRecord.Domain.Persistence.MemberMessages

open ToastmastersRecord.Data
open ToastmastersRecord.Domain.Infrastructure
open ToastmastersRecord.Domain.DomainTypes
open ToastmastersRecord.Domain.MemberMessages
open ToastmastersRecord.Data.Entities

open Newtonsoft.Json
open System.Data.Entity

let persist (userId:UserId) (streamId:StreamId) (state:Envelope<MemberMessageCommand> option) =
    use context = new ToastmastersEFDbContext () 
    let entity = context.Messages.Find (StreamId.unbox streamId)
    match entity, state with
    | null, Option.None -> ()
    | null, Option.Some(env) -> 
        match env.Item with 
        | MemberMessageCommand.Create(memberId, date, message) ->
            context.Messages.Add (
                MemberMessageEntity (
                    Id = StreamId.unbox streamId,
                    MemberId = MemberId.unbox memberId,
                    Message = message,
                    MessageDate = date
                )) |> ignore
    | _, Option.None -> context.Messages.Remove entity |> ignore        
    | _, Some(item) -> ()
    context.SaveChanges () |> ignore

let fetch date =
    use context = new ToastmastersEFDbContext ()
    let messages =
        query {
            for message in context.Messages do
            where (message.MessageDate >= date)
            join history in context.MemberHistories
                on (message.MemberId = history.Id)
            select (message.Id, message.MemberId, history.DisplayName, message.MessageDate, message.Message)}
        |> Seq.map (fun (msgId, memId, name, date, message) ->
            msgId, memId, name, date, message,
            query {
                for dayOff in context.DaysOff do
                where (dayOff.MessageId = msgId)
                join meeting in context.ClubMeetings
                    on (dayOff.MeetingId = meeting.Id)
                select (meeting.Id, meeting.Date) }
            |> Seq.map (fun (id, date) -> id |> MeetingId.box, date)
            |> Seq.toList)
        |> Seq.map (fun (msgId, memId, name, date, message, daysOff) ->
            msgId, memId, name, date, message, daysOff,
            query {
                for request in context.RoleRequests do
                where (request.MessageId = msgId)
                select (request.Id, request.Brief, request.State) })
        |> Seq.map (fun (msgId, memId, name, date, message, daysOff, requests) ->
            msgId, memId, name, date, message, daysOff, 
            requests
            |> Seq.map (fun (id, brief, state) ->
                id |> RoleRequestId.box,
                brief,
                state,
                query {
                    for requestMeeting in context.RoleRequestMeetings do
                    where (requestMeeting.RoleRequestId = id)
                    join meeting in context.ClubMeetings
                        on (requestMeeting.MeetingId = meeting.Id)
                    select (meeting.Id, meeting.Date) }
                |> Seq.map (fun (id, date) -> id |> MeetingId.box, date)
                |> Seq.toList)
            |> Seq.toList)
        |> Seq.map (fun (msgId, memId, name, date, message, daysOff, requests) ->
            (msgId |> MessageId.box, memId |> MemberId.box, name, date, message), daysOff, requests)
        |> Seq.toList
    messages


