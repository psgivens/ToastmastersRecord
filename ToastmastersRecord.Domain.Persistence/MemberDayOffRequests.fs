module ToastmastersRecord.Domain.Persistence.MemberDayOffRequests

open ToastmastersRecord.Data
open ToastmastersRecord.Domain.Infrastructure
open ToastmastersRecord.Domain.DomainTypes
open ToastmastersRecord.Domain.MemberMessages
open ToastmastersRecord.Data.Entities

open Newtonsoft.Json
open System.Data.Entity

let persist (userId:UserId) (streamId:StreamId) (state:Envelope<DayOffRequestCommand> option) =
    use context = new ToastmastersEFDbContext () 
    let entity = context.DaysOff.Find (StreamId.unbox streamId)
    match entity, state with
    | null, Option.None -> ()
    | null, Option.Some(env) -> 
        match env.Item with 
        | DayOffRequestCommand.Create(memberId, meetingId, messageId) ->
            context.DaysOff.Add (
                DayOffEntity (
                    Id = StreamId.unbox streamId,                    
                    MeetingId = MeetingId.unbox meetingId,
                    MemberId = MemberId.unbox memberId,
                    MessageId = MessageId.unbox messageId
                )) |> ignore
    | _, Option.None -> context.DaysOff.Remove entity |> ignore        
    | _, Some(item) -> ()
    context.SaveChanges () |> ignore
