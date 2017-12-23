// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open System 
open Akka.Actor
open Akka.FSharp

open ToastmastersRecord.Domain
open ToastmastersRecord.Domain.Infrastructure
open ToastmastersRecord.Domain.DomainTypes
open ToastmastersRecord.Data.Entities
open ToastmastersRecord.SampleApp.Schedule.PrintMeetings
open ToastmastersRecord.SampleApp.Schedule.EditMeeting
open ToastmastersRecord.SampleApp.Schedule.AggregateInformation

open ToastmastersRecord.Actors
open ToastmastersRecord.Domain.RolePlacements
open ToastmastersRecord.Domain.ClubMeetings
open ToastmastersRecord.Domain.MemberMessages
open ToastmastersRecord.Domain.RoleRequests

open ToastmastersRecord.SampleApp.Initialize
open ToastmastersRecord.SampleApp.IngestMembers
open ToastmastersRecord.SampleApp.IngestMembers

open ToastmastersRecord.SampleApp.Schedule.IngestMessages
open ToastmastersRecord.SampleApp.Schedule.MessageReview
open ToastmastersRecord.SampleApp.Schedule.MemberManagement

let processCommands system (actorGroups:ActorGroups) userId = 
    // Process Create command, wait for Completed event
    let rolePlacementRequestReply =
        RequestReplyActor.spawnRequestReplyActor<RolePlacementCommand,RolePlacementEvent> 
            system "rolePlacementRequestReply" actorGroups.RolePlacementActors

    // Create an request-reply actor that we can wait on. 
    let meetingRequestReplyCreate = 
        RequestReplyActor.spawnRequestReplyConditionalActor<ClubMeetingCommand,ClubMeetingEvent> 
            (fun x -> true)
            (fun x -> x.Item = Initialized)
            system "clubMeeting_initialized" actorGroups.ClubMeetingActors

    let dayOffsRequestReply = 
        RequestReplyActor.spawnRequestReplyActor<DayOffRequestCommand,unit> 
            system "dayOffRequest_ingest" actorGroups.DayOffActors

    let roleRequestsRequestReplyCreated = 
        RequestReplyActor.spawnRequestReplyActor<RoleRequestCommand,RoleRequestEvent> 
            system "RoleRequest_ingest" actorGroups.RoleRequestActors

    let rec loop () = 
        printfn """
Please make a selection
0) exit
1) List 5 meetings from date
2) List meeting details
3) Edit meeting details
4) Create new meeting
5) Calculate member statistics
6) Ingest messages
7) Review messages
8) Ingest Club Roster from Toastmasters.org
9) Add member
10) Ingest confirmed speech counts
    """
        match Console.ReadLine () |> Int32.TryParse with
        | true, 0 -> 
            printfn "Exiting"
        | true, 1 -> 
            processDate (Persistence.ClubMeetings.fetchByDate 5 >> displayMeetings)
            loop ()
        | true, 2 ->
            processDate (fun date ->          
                let meeting = Persistence.ClubMeetings.findByDate date
                let placements = Persistence.RolePlacements.findMeetingPlacements meeting.Id  
                displayMeeting userId meeting placements)
            loop ()        
        | true, 3 ->
            processDate (fun date ->          
                let meeting = Persistence.ClubMeetings.findByDate date
                let placements = Persistence.RolePlacements.findMeetingPlacements meeting.Id  
                editMeeting rolePlacementRequestReply userId meeting placements
                printfn "Calculating member statistics..."
                actorGroups |> calculateHistory system userId 
                printfn "Member statistics calculated"
                )
            loop ()        
        | true, 4 -> 
            processDate <| createMeeting meetingRequestReplyCreate userId 
            loop ()
        | true, 5 -> 
            printfn "Calculating member statistics..."
            actorGroups |> calculateHistory system userId 
            printfn "Member statistics calculated"
            loop ()
        | true, 6 -> 
            printfn "Please enter the member messages file name"
            let fileName = Console.ReadLine ()
            if fileName |> IO.File.Exists 
            then fileName |> ingestMemberMessages system userId actorGroups                
            else printfn "File not found"
            loop ()
        | true, 7 -> 
            reviewMessages userId dayOffsRequestReply roleRequestsRequestReplyCreated 
            loop ()
        | true, 8 ->
            printfn "Please enter the file name for the member roster"
            let fileName = Console.ReadLine ()
            if fileName |> IO.File.Exists 
            then fileName |> ingestMembers system userId actorGroups                
            else printfn "File not found"
            loop ()
        | true, 9 ->
            printfn "Please enter the name of the member"
            match Console.ReadLine () with
            | empty when empty |> String.IsNullOrWhiteSpace -> printfn "Nothing entered"
            | name -> 
                createMember system userId actorGroups name  
            loop ()
        | true, 10 ->        
            printfn "What file contains the member ingestions?"
            match Console.ReadLine () with
            | empty when empty |> String.IsNullOrWhiteSpace -> printfn "Nothing entered"
            | fileName -> ingestSpeechCount system userId fileName actorGroups            
            loop ()
        | true, i -> 
            printfn "Number not recognized: %d" i
            loop ()
        | _ -> 
            printfn "Not a number" 
            loop ()
    loop ()    

[<EntryPoint>]
let main argv = 

    // System set up
    NewtonsoftHack.resolveNewtonsoft ()    
    let system = Configuration.defaultConfig () |> System.create "sample-system"
            
    let actorGroups = composeActors system

    let userId = Persistence.Users.findUserId "ToastmastersRecord.SampleApp.Schedule" 
    processCommands system actorGroups userId
    printfn "%A" argv
    0 // return an integer exit code
