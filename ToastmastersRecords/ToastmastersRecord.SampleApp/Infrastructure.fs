module ToastmastersRecord.SampleApp.Infrastructure

open Akka.Actor
open Akka.FSharp
open ToastmastersRecord.Actors

let onEvents sys name events =
    actorOf2 
    >> spawn sys (name + "_onEvents")
    >> SubjectActor.subscribeTo events

       
let debugger system name errorActor =
    let p mailbox cmdenv =
        System.Diagnostics.Debugger.Break ()
    actorOf2 p         
    |> spawn system name
    |> SubjectActor.subscribeTo errorActor

