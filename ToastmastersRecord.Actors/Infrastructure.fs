module ToastmastersRecord.Actors.Infrastructure

open Akka.Actor
open Akka.FSharp
open ToastmastersRecord.Domain.Infrastructure

type ActorIO<'a> = { Tell:Envelope<'a> -> unit; Actor:IActorRef; Events:IActorRef; Errors:IActorRef }
