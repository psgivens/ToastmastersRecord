module ToastmastersRecord.Domain.Persistence.RoleRequests

open ToastmastersRecord.Data
open ToastmastersRecord.Data.Entities
open ToastmastersRecord.Domain.Infrastructure
open ToastmastersRecord.Domain.DomainTypes
open ToastmastersRecord.Domain.RoleRequests

open Newtonsoft.Json
open System.Data.Entity

let persist (userId:UserId) (streamId:StreamId) (state:RoleRequestState option) =
    use context = new ToastmastersEFDbContext () 
    let entity = context.RoleRequests.Find (StreamId.unbox streamId)
    match entity, state with
    | null, Option.None -> ()
    | null, Option.Some(item) ->              
        let meetings =
            item.Meetings
            |> List.map (fun mtgid -> 
                RoleRequestMeeting (
                    RoleRequestId = StreamId.unbox streamId,
                    MeetingId = MeetingId.unbox mtgid))
            |> System.Collections.Generic.List 
        context.RoleRequests.Add (
            RoleRequestEntity (
                Id = StreamId.unbox streamId,
                State = int item.State, 
                MessageId = MessageId.unbox item.MessageId,
                Brief = item.Brief,
                MemberId = MemberId.unbox item.MemberId,
                Meetings = meetings
            )) |> ignore
    | _, Option.None -> context.RoleRequests.Remove entity |> ignore        
    | _, Some(item) -> 
        () // TODO: update
    context.SaveChanges () |> ignore
    
let find (userId:UserId) (streamId:StreamId) =
    use context = new ToastmastersEFDbContext () 
    context.RoleRequests.Find (StreamId.unbox streamId)

let getMemberRequest (userId:UserId) (MemberId.Id memberId) (MeetingId.Id meetingId) =
    use context = new ToastmastersEFDbContext () 
    let absent = 
        query { for dayOff in context.DaysOff do
                where (dayOff.MeetingId = meetingId && dayOff.MemberId = memberId)
                select true
                exactlyOneOrDefault }

    let request = 
       query {  for meeting in context.RoleRequestMeetings do
                join request in context.RoleRequests 
                    on (meeting.RoleRequestId = request.Id)
                where (request.MemberId = memberId && meeting.MeetingId = meetingId)
                select request.Brief
                exactlyOneOrDefault } 
 
    absent, request
