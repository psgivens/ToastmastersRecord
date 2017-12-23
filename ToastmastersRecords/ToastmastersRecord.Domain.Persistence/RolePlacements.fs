module ToastmastersRecord.Domain.Persistence.RolePlacements

open ToastmastersRecord.Data
open ToastmastersRecord.Data.Entities
open ToastmastersRecord.Domain.Infrastructure
open ToastmastersRecord.Domain.DomainTypes
open ToastmastersRecord.Domain.RolePlacements

open Newtonsoft.Json
open System.Data.Entity
open System

let persist (userId:UserId) (streamId:StreamId) (state:RolePlacementState option) =
    use context = new ToastmastersEFDbContext () 
    let entity = context.RolePlacements.Find (StreamId.unbox streamId)
    match entity, state with
    | null, Option.None -> ()
    | null, Option.Some(item) ->                     
        let state, memberId, roleRequestId = 
            match item.State with
            | Assigned (mid, rrid) -> 1, MemberId.unbox mid, RoleRequestId.unbox rrid
            | Complete (mid, rrid) -> 2, MemberId.unbox mid, RoleRequestId.unbox rrid
            | _ -> 0, System.Guid.Empty, System.Guid.Empty

        context.RolePlacements.Add (
            RolePlacementEntity (
                Id = StreamId.unbox streamId,
                State = state, 
                MemberId = memberId,
                RoleRequestId = roleRequestId,
                RoleTypeId = int item.RoleTypeId,
                MeetingId = MeetingId.unbox item.MeetingId
            )) |> ignore
    | _, Option.None -> context.RolePlacements.Remove entity |> ignore        
    | _, Some(item) -> 
        let state, memberId, roleRequestId = 
            match item.State with
            | Assigned (mid, rrid) -> 1, MemberId.unbox mid, RoleRequestId.unbox rrid
            | Complete (mid, rrid) -> 2, MemberId.unbox mid, RoleRequestId.unbox rrid
            | _ -> 0, System.Guid.Empty, System.Guid.Empty

        entity.State <- state
        entity.MemberId <- memberId
        entity.RoleRequestId <- roleRequestId
        printfn "Persist: (%A, %A, %A)" state roleRequestId memberId 

    context.SaveChanges () |> ignore
    
let find (userId:UserId) (streamId:StreamId) =
    use context = new ToastmastersEFDbContext () 
    context.RolePlacements.Find (StreamId.unbox streamId)

let findMeetingPlacements id = 
    use context = new ToastmastersEFDbContext () 
    query { for placement in context.RolePlacements do
            where (placement.MeetingId = id)
            sortBy placement.RoleTypeId
            select placement}
    |> Seq.toList

let getPlacmentByMeetingAndRole roleTypeId meetingId = 
    use context = new ToastmastersEFDbContext () 
    query { for placement in context.RolePlacements do
            where (placement.RoleTypeId = roleTypeId 
            && placement.MeetingId = meetingId
            && placement.State = 0)
            select placement
            } |> Seq.head
            
let getRoleTypeId name =
    use context = new ToastmastersEFDbContext ()
    query { for roleType in context.RoleTypes do 
            where (roleType.Title = name)
            select roleType.Id 
            exactlyOne }

let getRolePlacmentsByMember memberId =
    use context = new ToastmastersEFDbContext ()
    query { for placement in context.RolePlacements do
            join meeting in context.ClubMeetings
                on (placement.MeetingId = meeting.Id)
            sortBy meeting.Date 
            where (placement.MemberId = memberId)
            select (meeting.Date, placement)
            }
    |> Seq.toList                

let getRolePlacementsByMemberSinceDate date memberId =
    use context = new ToastmastersEFDbContext ()
    query { for placement in context.RolePlacements do
            join meeting in context.ClubMeetings
                on (placement.MeetingId = meeting.Id)
            sortBy meeting.Date 
            where (placement.MemberId = memberId)
            where (meeting.Date > date)
            select (meeting.Date, placement)
            }
    |> Seq.toList                