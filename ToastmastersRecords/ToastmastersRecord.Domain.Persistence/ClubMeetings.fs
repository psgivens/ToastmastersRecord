module ToastmastersRecord.Domain.Persistence.ClubMeetings

open ToastmastersRecord.Data
open ToastmastersRecord.Domain.Infrastructure
open ToastmastersRecord.Domain.DomainTypes
open ToastmastersRecord.Domain.ClubMeetings
open ToastmastersRecord.Data.Entities

open Newtonsoft.Json
open System.Data.Entity

let persist (userId:UserId) (streamId:StreamId) (state:ClubMeetingState option) =
    use context = new ToastmastersEFDbContext () 
    let entity = context.ClubMeetings.Find (StreamId.unbox streamId)
    match entity, state with
    | null, Option.None -> ()
    | null, Option.Some(item) -> 
        context.ClubMeetings.Add (
            ClubMeetingEntity (
                Id = StreamId.unbox streamId,
                Date = item.Date,
                State = int item.State
            )) |> ignore
    | _, Option.None -> context.ClubMeetings.Remove entity |> ignore        
    | _, Some(item) -> entity.State <- int item.State
    context.SaveChanges () |> ignore
    
let find (userId:UserId) (streamId:StreamId) =
    use context = new ToastmastersEFDbContext () 
    let id = StreamId.unbox streamId
    query { for meeting in context.ClubMeetings do
            where (meeting.Id = id)
            select meeting}
//    |> fun meetings -> meetings.Include "RolePlacements"
    |> Seq.head

let findPreviousIds (date:System.DateTime) =
    use context = new ToastmastersEFDbContext () 
    query { for meeting in context.ClubMeetings do
            where (meeting.Date < date)
            sortByDescending meeting.Date
            select (meeting.Id, meeting.Date)
            take 2 }
    |> Seq.map (fun (g, d) -> g |> MeetingId.box, d)
    |> Seq.toList

let findByDate (date:System.DateTime) =
    use context = new ToastmastersEFDbContext () 
    query { for meeting in context.ClubMeetings do
            where (meeting.Date = date)
            select meeting
            exactlyOne }

let fetchByDate count' (date:System.DateTime) =
    use context = new ToastmastersEFDbContext () 
    query { for meeting in context.ClubMeetings do
            where (meeting.Date >= date)
            sortBy meeting.Date
            select meeting 
            take count' }    
    |> Seq.toList

let fetchOpenMeetings () =
    use context = new ToastmastersEFDbContext () 
    let pending = ClubMeetingStateValue.Pending |> int
    query { for meeting in context.ClubMeetings do
            where (meeting.State = pending)
            sortBy meeting.Date
            select meeting }    
    |> Seq.toList

