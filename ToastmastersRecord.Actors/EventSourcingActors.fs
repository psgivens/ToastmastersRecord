﻿[<RequireQualifiedAccess>]
module ToastmastersRecord.Actors.EventSourcingActors

open Akka.Actor
open Akka.FSharp

open ToastmastersRecord.Domain

open ToastmastersRecord.Domain.Infrastructure
open ToastmastersRecord.Domain.DomainTypes
open ToastmastersRecord.Domain.Infrastructure.Envelope
open ToastmastersRecord.Domain.CommandHandlers
open ToastmastersRecord.Actors
open ToastmastersRecord.Actors.Infrastructure

let spawn<'TCommand, 'TEvent, 'TState>
   (    sys,
        name,
        eventStore,
        buildState:'TState option -> 'TEvent list -> 'TState option,
        handle:CommandHandlers<'TEvent, Version> -> 'TState option -> Envelope<'TCommand> -> CommandHandlerFunction<Version>,
        persist:UserId -> StreamId -> 'TState option -> unit) :ActorIO<'TCommand> = 

    // Create a subject so that the next step can subscribe. 
    let persistEventSubject = SubjectActor.spawn sys (name + "_Events")
    let errorSubject = SubjectActor.spawn sys (name + "_Errors")

    let aggregateActor =
              
        spawnOpt sys (name + "_AggregateActor") 
        <| AggregateActor.create
                (persistEventSubject,
                 errorSubject,
                 eventStore,
                 buildState,
                 handle,
                 persist)
        <| [Akka.Routing.ConsistentHashingPool (10, fun msg -> 
                match msg with
                | :? Envelope<'TCommand> as cmdenv -> cmdenv.StreamId :> obj
                | :? Envelope<'TEvent> as evtenv -> evtenv.StreamId :> obj
                | _ -> msg )
            :> Akka.Routing.RouterConfig
            |> SpawnOption.Router]
    
    { Tell=aggregateActor.Tell; 
      Actor=aggregateActor;
      Events=persistEventSubject;
      Errors=errorSubject }
