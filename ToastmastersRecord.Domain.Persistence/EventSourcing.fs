module ToastmastersRecord.Domain.Persistence.ToastmastersEventStore
open ToastmastersRecord.Data
open ToastmastersRecord.Domain.Infrastructure
open Newtonsoft.Json
open System.Data.Entity


type ToastmastersEFDbContext with 
    member this.GetAggregateEvents<'a,'b when 'b :> EnvelopeEntityBase and 'b: not struct>
        (dbset:ToastmastersEFDbContext->DbSet<'b>)
        (StreamId.Id (aggregateId):StreamId)
        :seq<Envelope<'a>>= 
        query {
            for event in this |> dbset do
            where (event.StreamId = aggregateId)
            select event
        } |> Seq.map (fun event ->
                {
                    Id = event.Id
                    UserId = UserId.box event.UserId
                    StreamId = StreamId.box aggregateId
                    TransactionId = TransId.box event.TransactionId
                    Version = Version.box (event.Version)
                    Created = event.TimeStamp
                    Item = (JsonConvert.DeserializeObject<'a> event.Event)
                }
            )

open ToastmastersRecord.Domain.MemberManagement
type MemberManagementEventStore () =
    interface IEventStore<MemberManagementEvent> with
        member this.GetEvents (streamId:StreamId) =
            use context = new  ToastmastersEFDbContext ()
            streamId
            |> context.GetAggregateEvents (fun i -> i.MemberEvents) 
            |> Seq.toList 
            |> List.sortBy(fun x -> x.Version)
        member this.AppendEvent (envelope:Envelope<MemberManagementEvent>) =
            try
                use context = new ToastmastersEFDbContext ()
                context.MemberEvents.Add (
                    MemberEnvelopeEntity (  Id = envelope.Id,
                                                StreamId = StreamId.unbox envelope.StreamId,
                                                UserId = UserId.unbox envelope.UserId,
                                                TransactionId = TransId.unbox envelope.TransactionId,
                                                Version = Version.unbox envelope.Version,
                                                TimeStamp = envelope.Created,
                                                Event = JsonConvert.SerializeObject(envelope.Item)
                                                )) |> ignore         
                context.SaveChanges () |> ignore
                
            with
                | ex -> System.Diagnostics.Debugger.Break () 



open ToastmastersRecord.Domain.RoleRequests
type RoleRequestEventStore () =
    interface IEventStore<RoleRequestEvent> with
        member this.GetEvents (streamId:StreamId) =
            use context = new  ToastmastersEFDbContext ()
            streamId
            |> context.GetAggregateEvents (fun i -> i.RoleRequestEvents) 
            |> Seq.toList 
            |> List.sortBy(fun x -> x.Version)
        member this.AppendEvent (envelope:Envelope<RoleRequestEvent>) =
            try
                use context = new ToastmastersEFDbContext ()
                context.RoleRequestEvents.Add (
                    RoleRequestEnvelopeEntity (  Id = envelope.Id,
                                                StreamId = StreamId.unbox envelope.StreamId,
                                                UserId = UserId.unbox envelope.UserId,
                                                TransactionId = TransId.unbox envelope.TransactionId,
                                                Version = Version.unbox envelope.Version,
                                                TimeStamp = envelope.Created,
                                                Event = JsonConvert.SerializeObject(envelope.Item)
                                                )) |> ignore         
                context.SaveChanges () |> ignore
                
            with
                | ex -> System.Diagnostics.Debugger.Break () 

open ToastmastersRecord.Domain.RolePlacements
type RolePlacementEventStore () =
    interface IEventStore<RolePlacementEvent> with
        member this.GetEvents (streamId:StreamId) =
            use context = new  ToastmastersEFDbContext ()
            streamId
            |> context.GetAggregateEvents (fun c -> c.RolePlacementEvents)
            |> Seq.toList 
            |> List.sortBy(fun x -> x.Version)
        member this.AppendEvent (envelope:Envelope<RolePlacementEvent>) =
            try
                use context = new ToastmastersEFDbContext ()
                context.RolePlacementEvents.Add (
                    RolePlacementEnvelopeEntity (  Id = envelope.Id,
                                                StreamId = StreamId.unbox envelope.StreamId,
                                                UserId = UserId.unbox envelope.UserId,
                                                TransactionId = TransId.unbox envelope.TransactionId,
                                                Version = Version.unbox envelope.Version,
                                                TimeStamp = envelope.Created,
                                                Event = JsonConvert.SerializeObject(envelope.Item)
                                                )) |> ignore         
                context.SaveChanges () |> ignore
                
            with
                | ex -> System.Diagnostics.Debugger.Break () 

open ToastmastersRecord.Domain.ClubMeetings
type ClubMeetingEventStore () =
    interface IEventStore<ClubMeetingEvent> with
        member this.GetEvents (streamId:StreamId) =
            use context = new  ToastmastersEFDbContext ()
            streamId
            |> context.GetAggregateEvents (fun c -> c.ClubMeetingEvents)
            |> Seq.toList 
            |> List.sortBy(fun x -> x.Version)
        member this.AppendEvent (envelope:Envelope<ClubMeetingEvent>) =
            try
                use context = new ToastmastersEFDbContext ()
                context.ClubMeetingEvents.Add (
                    ClubMeetingEnvelopeEntity (  Id = envelope.Id,
                                                StreamId = StreamId.unbox envelope.StreamId,
                                                UserId = UserId.unbox envelope.UserId,
                                                TransactionId = TransId.unbox envelope.TransactionId,
                                                Version = Version.unbox envelope.Version,
                                                TimeStamp = envelope.Created,
                                                Event = JsonConvert.SerializeObject(envelope.Item)
                                                )) |> ignore         
                context.SaveChanges () |> ignore
                
            with
                | ex -> System.Diagnostics.Debugger.Break () 

//open ToastmastersRecord.Domain.RoleRequests
//type GenericEventStore<'a,'b when 'b :> EnvelopeEntityBase and 'b: not struct and 'b >
//    (dbset:ToastmastersEFDbContext->DbSet<'b>) =
//    
//    member this.GetAggregateEvents
//        (dbset:ToastmastersEFDbContext->DbSet<'b>)
//        (StreamId.Id (aggregateId):StreamId)
//        :seq<Envelope<'a>>= 
//    
//    interface IEventStore<Envelope<'TEvent, 'TEnvelope>> with
//        member this.GetEvents (streamId:StreamId) =
//            use context = new  ToastmastersEFDbContext ()
//            streamId
//            |> context.GetAggregateEvents (fun i -> i.RoleRequestEvents) 
//            |> Seq.toList 
//            |> List.sortBy(fun x -> x.Version)
//        member this.AppendEvent (streamId:StreamId) (envelope:Envelope<RoleRequestEvent>) =
//            try
//                use context = new ToastmastersEFDbContext ()
//                context.RoleRequestEvents.Add (
//                    RoleRequestEnvelopeEntity (  Id = envelope.Id,
//                                                StreamId = StreamId.unbox envelope.StreamId,
//                                                UserId = UserId.unbox envelope.UserId,
//                                                TransactionId = TransId.unbox envelope.TransactionId,
//                                                Version = Version.unbox envelope.Version,
//                                                TimeStamp = envelope.Created,
//                                                Event = JsonConvert.SerializeObject(envelope.Item)
//                                                )) |> ignore         
//                context.SaveChanges () |> ignore
//                
//            with
//                | ex -> System.Diagnostics.Debugger.Break () 
//