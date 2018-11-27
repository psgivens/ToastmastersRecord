module ToastmastersRecord.SampleApp.Initialize

open Akka.Actor
open Akka.FSharp

open ToastmastersRecord.Domain
open ToastmastersRecord.Domain.Infrastructure
open ToastmastersRecord.Domain.DomainTypes
open ToastmastersRecord.Domain.MemberManagement
open ToastmastersRecord.Domain.RoleRequests
open ToastmastersRecord.Domain.MemberMessages
open ToastmastersRecord.Domain.RolePlacements 
open ToastmastersRecord.Domain.ClubMeetings
open ToastmastersRecord.Actors

open ToastmastersRecord.Domain.Persistence.ToastmastersEventStore
open ToastmastersRecord.Actors.Infrastructure
open ToastmastersRecord.Actors

type ActorGroups = {
    MemberManagementActors:ActorIO<MemberManagementCommand>
    MessageActors:ActorIO<MemberMessageCommand>
    DayOffActors:ActorIO<DayOffRequestCommand>
    RoleRequestActors:ActorIO<RoleRequestCommand>
    RolePlacementActors:ActorIO<RolePlacementCommand>
    ClubMeetingActors:ActorIO<ClubMeetingCommand>
    MemberHistoryActors:ActorIO<MemberHistoryConfirmation>
    }

let composeActors system =
    // Create member management actors
    let memberManagementActors = 
        EventSourcingActors.spawn 
            (system,
             "memberManagement", 
             MemberManagementEventStore (),
             buildState MemberManagement.evolve,
             MemberManagement.handle,
             Persistence.MemberManagement.persist)    

    let messageActors = 
        CrudMessagePersistanceActor.spawn<MemberMessageCommand>
            (system, 
             "memberMessage", 
             Persistence.MemberMessages.persist)

    let dayOffActors =
        CrudMessagePersistanceActor.spawn<DayOffRequestCommand>
            (system,
             "dayOffRequest",
             Persistence.MemberDayOffRequests.persist)

    let historyActors =
        CrudMessagePersistanceActor.spawn<MemberHistoryConfirmation>
            (system,
            "memberHistoryConfirmation",
            Persistence.MemberManagement.persistConfirmation)

    // Create role request actors
    let roleRequestActors =
        EventSourcingActors.spawn
            (system,
             "roleRequests",
             RoleRequestEventStore (),
             buildState RoleRequests.evolve,
             RoleRequests.handle,
             Persistence.RoleRequests.persist)

    // Create role request actors
    let rolePlacementActors =
        EventSourcingActors.spawn<RolePlacementCommand,RolePlacementEvent,RolePlacementState>
            (system,
             "rolePlacements",
             RolePlacementEventStore (),
             buildState RolePlacements.evolve,
             RolePlacements.handle,
             Persistence.RolePlacements.persist)   

    let placementRequestReplyCreate = 
        RequestReplyActor.spawnRequestReplyConditionalActor<RolePlacementCommand,RolePlacementEvent> 
            (fun cmd -> true)
            (fun evt -> 
                match evt.Item with
                | RolePlacementEvent.Opened _ -> true
                | _ -> false)
            system "rolePlacement_create" rolePlacementActors

    let createRolePlacement meetingEnv roleTypeId = 
        ((roleTypeId, MeetingId.box <| StreamId.unbox meetingEnv.StreamId)
        |> RolePlacementCommand.Open
        |> envelopWithDefaults
            (meetingEnv.UserId)
            (meetingEnv.TransactionId)
            (StreamId.create ())
            (Version.box 0s))
        |> placementRequestReplyCreate.Ask
        |> Async.AwaitTask

    let placementRequestReplyCancel = 
        RequestReplyActor.spawnRequestReplyConditionalActor<RolePlacementCommand,RolePlacementEvent> 
            (fun cmd -> true)
            (fun evt -> 
                match evt.Item with
                | RolePlacementEvent.Canceled _ -> true
                | _ -> false)
            system "rolePlacement_cancel" rolePlacementActors

    let cancelRolePlacement findMeetingPlacements meetingEnv =         
        findMeetingPlacements meetingEnv.Id 
        |> List.map (fun placement ->
            RolePlacementCommand.Cancel
            |> envelopWithDefaults
                (meetingEnv.UserId)
                (meetingEnv.TransactionId)
                (StreamId.create ())
                (Version.box 0s)
            |> placementRequestReplyCancel.Ask
            |> Async.AwaitTask)
        |> Async.Parallel
        |> Async.Ignore
        
    // Create member management actors
    let clubMeetingActors = 
        EventSourcingActors.spawn 
            (system,
             "clubMeetings", 
             ClubMeetingEventStore (),
             buildState ClubMeetings.evolve,
             (ClubMeetings.handle {
                RoleActions.createRole=createRolePlacement
                RoleActions.cancelRoles=cancelRolePlacement Persistence.RolePlacements.findMeetingPlacements}),
             Persistence.ClubMeetings.persist)    
             
    { MemberManagementActors=memberManagementActors
      MessageActors=messageActors
      DayOffActors=dayOffActors
      RoleRequestActors=roleRequestActors
      RolePlacementActors=rolePlacementActors
      ClubMeetingActors=clubMeetingActors
      MemberHistoryActors=historyActors
    }
