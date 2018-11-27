// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open ToastmastersRecord.SampleApp
open ToastmastersRecord.SampleApp.Initialize
open ToastmastersRecord.SampleApp.Infrastructure
open ToastmastersRecord.SampleApp.AggregateInformation

open System
open Akka.Actor
open Akka.FSharp
open FSharp.Data

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

open ToastmastersRecord.SampleApp.IngestMembers
open ToastmastersRecord.SampleApp.IngestMeetings
open ToastmastersRecord.SampleApp.IngestMessages
[<EntryPoint>]
let main argv =

    // System set up
    NewtonsoftHack.resolveNewtonsoft ()    
    let system = Configuration.defaultConfig () |> System.create "sample-system"
            
    let actorGroups = composeActors system
    
    // Sample data
    let userId = Persistence.Users.findUserId "ToastmastersRecord.SampleApp.Initialize" 
    
    let clubRosterFile = "C:\Users\Phillip Givens\OneDrive\Toastmasters\Club-Roster20171126.csv"
    ingestMembers system userId actorGroups clubRosterFile
    actorGroups |> ingestSpeechCount system userId "C:\Users\Phillip Givens\OneDrive\Toastmasters\ConfirmedSpeechCount.csv"

    actorGroups |> ingestMemberMessages system userId 
    actorGroups |> ingestMeetings system userId
    actorGroups |> ingestHistory system userId
    actorGroups |> generateMessagesToMembers system userId
    actorGroups |> ingestDaysOff system userId
    actorGroups |> ingestRoleRequests system userId

    let date = "12/12/2017" |> DateTime.Parse
    actorGroups |> calculateHistory system userId date 
    actorGroups |> generateStatistics system userId

    printfn "Press enter to continue"
    System.Console.ReadLine () |> ignore
    printfn "%A" argv
    0 // return an integer exit code
