// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open System

open Akka.Actor
open Akka.FSharp
open FSharp.Data

open ToastmastersRecord.Actors
open ToastmastersRecord.Domain
open ToastmastersRecord.Domain.Infrastructure
open ToastmastersRecord.Domain.DomainTypes
open ToastmastersRecord.Domain.ClubMeetings
open ToastmastersRecord.SampleApp.Infrastructure
open ToastmastersRecord.SampleApp.Initialize
open ToastmastersRecord.SampleApp.IngestMembers
open ToastmastersRecord.SampleApp.IngestMeetings
open ToastmastersRecord.SampleApp.IngestMessages

let interpret = interpret "NA" "MMM dd, yyyy"
let generateMeetings system userId actorGroups =    
    let fileName = "C:\Users\Phillip Givens\OneDrive\Toastmasters\ClubMeetings.csv"
    
//    let csvFile = new MeetingsCsvType ([("Meeting Id", "Meeting Date", "Concluded")|> MeetingsCsvType.Row])
    let csvFile = new MeetingsCsvType ([])
    let csvFile' = 
        DateTime.Parse("07/11/2017")           
        |> Seq.unfold (fun date -> 
            if date < DateTime.Now then Some(date, date.AddDays 7.0)
            else None)
        |> Seq.map (fun date -> 
            ((Guid.NewGuid ()).ToString "D", interpret date, "true"))
        |> Seq.map MeetingsCsvType.Row
        |> csvFile.Append

    let endOfYear = DateTime.Parse("12/31/2017")
    let csvFile'' = 
        DateTime.Parse("11/28/2017")           
        |> Seq.unfold (fun date -> 
            if date < endOfYear then Some(date, date.AddDays 7.0)
            else None)
        |> Seq.map (fun date -> 
            ((Guid.NewGuid ()).ToString "D", interpret date, "false"))
        |> Seq.map MeetingsCsvType.Row
        |> csvFile'.Append

    csvFile''.Save fileName

let addIdToMemberMessages system userId (actorGroups:ActorGroups) = 
    let messages = 
        CsvFile.Load(
            "C:\Users\Phillip Givens\OneDrive\Toastmasters\RoleRequestMessages.txt",
            separators="\t",
            hasHeaders=true).Cache()
    let fileName = messagesFileName 

//    let csvFile = new MessagesCsvType  ([("Meeting Id", "Name", "Date", "Message")|> MessagesCsvType.Row])
    let csvFile = new MessagesCsvType  ([])

    let csv = 
        messages.Rows 
        |> Seq.map (fun row ->
            ((Guid.NewGuid ()).ToString "D"), (row.GetColumn "Name"), (row.GetColumn "Date"), (row.GetColumn "Message"))
        |> Seq.map MessagesCsvType .Row
        |> csvFile.Append
    csv.Save fileName

type Meeting = {
    Id:MeetingId
    Date:DateTime
    Concluded:bool
    }

type Description = string
type Request =
    | Unavailable of MessageId
    | Available of Description * MessageId list

type RequestInfo = {
    MessageId:MessageId
    Name:string
    Request:Request
    }

let roleRequestsFileName = "C:\Users\Phillip Givens\OneDrive\Toastmasters\RoleRequestMessagesId.txt"    

let buildRequests () = 
    let meetingsFile = MeetingsCsvType.Load (clubMeetingsFileName)//"C:\Users\Phillip Givens\OneDrive\Toastmasters\ClubMeetings.csv" 
    let messagesFile = MessagesCsvType.Load (roleRequestsFileName) //"C:\Users\Phillip Givens\OneDrive\Toastmasters\RoleRequestMessagesId.txt"

//    let absentsFileName = "C:\Users\Phillip Givens\OneDrive\Toastmasters\RequestOff.csv"
//    let requestFileName = "C:\Users\Phillip Givens\OneDrive\Toastmasters\RequestOn.csv"

    IO.File.WriteAllText (absentsFileName, String.Empty)
    IO.File.WriteAllText (requestFileName, String.Empty)

    use absentsFile = new DayOffCsvType      ([])
    use requestFile = new RoleRequestCsvType ([])

    let unscheduledMeetings = 
        meetingsFile.Rows
        |> Seq.map (fun row -> 
            {   Meeting.Id = row.``Meeting Id`` |> Guid.Parse |> MeetingId.box
                Date = row.``Meeting Date`` |> DateTime.Parse
                Concluded = row.Concluded |> Boolean.Parse } )
        |> Seq.where (fun meeting -> not meeting.Concluded)
        |> Seq.toArray

    messagesFile.Rows
    |> Seq.fold (fun (state:RequestInfo list) row -> 
        Seq.unfold (fun unscheduledMeetings ->
            if unscheduledMeetings |> Array.isEmpty then None
            else
            
                
                printfn """
%s said:
%s
----
Which dates would you like to work with? 
List all indices, comma delimitated.
-1 to end message.
"""
                    row.Name
                    row.Message

                unscheduledMeetings
                |> Array.iteri (fun i meeting ->
                    printfn "%d) %s" i (meeting.Date |> interpret))

                let response = Console.ReadLine ()
                if response = "-1" then None
                else
                    let selected =
                        response.Split ','
                        |> Seq.map (fun r ->
                            let i = r.Trim () |> Int32.Parse
                            unscheduledMeetings.[i]
                            )
                        |> Seq.toList                    
                        
                    printfn """
Would you like to 
1) Request the day off
2) Describe your request
"""
                    match Console.ReadLine () |> Int32.TryParse with
                    | (true,1) -> 
                        selected
                        |> List.map (fun meeting ->
                            {   RequestInfo.MessageId = row.``Message Id`` |> Guid.Parse |> MessageId.box
                                RequestInfo.Name = row.Name
                                RequestInfo.Request = meeting.Id |> Request.Unavailable})
                        |> fun l -> 
                            Some(l, 
                                unscheduledMeetings 
                                |> Array.filter (fun meeting -> 
                                    selected 
                                    |> List.contains meeting 
                                    |> not))
                    | (true,2) -> 
                        printfn "Please describe the request"
                        ([{ RequestInfo.MessageId = row.``Message Id`` |> Guid.Parse |> MessageId.box
                            RequestInfo.Name = row.Name
                            RequestInfo.Request = 
                                (Console.ReadLine (), selected |> List.map (fun meeting -> meeting.Id))
                                |> Request.Available }],
                         unscheduledMeetings)
                        |> Some
                    | _ -> 
                        printfn "You've entered an invalid value"
                        Some ([], unscheduledMeetings)

            ) unscheduledMeetings
        |> Seq.fold (fun allItems rowItems -> rowItems@allItems) state
        ) []
    |> Seq.fold (fun ((absents:Runtime.CsvFile<DayOffCsvType.Row>), (requests:Runtime.CsvFile<RoleRequestCsvType.Row>)) request -> 
        match request.Request with
        | Unavailable (MeetingId.Id meetingId) -> 
            [(meetingId.ToString "D", request.MessageId |> MessageId.toString, request.Name)] 
            |> Seq.map DayOffCsvType.Row
            |> absents.Append, 
            requests
        | Available (description, meetingIds) -> 
            absents,
            [(String.Join (";", meetingIds |> List.map (fun (MeetingId.Id i) -> i.ToString "D")), 
              request.MessageId |> MessageId.toString, 
              request.Name, 
              description)] 
            |> Seq.map RoleRequestCsvType.Row
            |> requests.Append            
        ) (absentsFile :> Runtime.CsvFile<DayOffCsvType.Row>, 
           requestFile :> Runtime.CsvFile<RoleRequestCsvType.Row>) 
    |> fun (absents, requests) ->
        absents.Save absentsFileName
        requests.Save requestFileName

let printMessageReport system userId actorGroups =
    let meetingsFile = MeetingsCsvType.Load (clubMeetingsFileName)//"C:\Users\Phillip Givens\OneDrive\Toastmasters\ClubMeetings.csv" 
    let messagesFile = MessagesCsvType.Load (messagesFileName )//"C:\Users\Phillip Givens\OneDrive\Toastmasters\RoleRequestMessagesId.txt"
    let dayOffsFile = DayOffCsvType.Load (absentsFileName)//"C:\Users\Phillip Givens\OneDrive\Toastmasters\RequestOff.csv"
    let requestFile = RoleRequestCsvType.Load (requestFileName )//"C:\Users\Phillip Givens\OneDrive\Toastmasters\RequestOn.csv"
    let messagesReportFileName = "C:\Users\Phillip Givens\OneDrive\Toastmasters\MessagesReport.txt"
    IO.File.WriteAllText (messagesReportFileName , String.Empty)
    
    let meetings =
        meetingsFile.Rows
        |> Seq.map (fun row ->
            let id = row.``Meeting Id`` |> Guid.Parse |> MeetingId.box
            id,
            {   Meeting.Id = id
                Meeting.Date = row.``Meeting Date`` |> DateTime.Parse
                Meeting.Concluded = row.Concluded |> bool.Parse}
            )
        |> Map.ofSeq

    let dayOffRequests = 
        dayOffsFile.Rows
        |> Seq.fold (fun msgmap row ->
            let msgid = row.``Message Id`` |> Guid.Parse |> MessageId.box
            let mtgid = row.``Meeting Id`` |> Guid.Parse |> MeetingId.box
            let requests =
                match msgmap |> Map.tryFind msgid with
                | Some requests -> requests
                | _ -> []
            
            msgmap 
            |> Map.add msgid (
                {   
                    RequestInfo.MessageId = msgid
                    RequestInfo.Name = row.Name
                    RequestInfo.Request = Request.Unavailable mtgid
                }::requests)
            ) Map.empty<FsGuidType, RequestInfo list>

    let roleRequests = 
        requestFile.Rows
        |> Seq.fold (fun msgmap row ->
            let msgid = row.``Message Id`` |> Guid.Parse |> MessageId.box
            let meetingIds = row.``Meeting Id`` |> unpackMeetingIdsFromRequest
//                row.``Meeting Id``.Split ';'
//                |> Array.toList
//                |> List.map (fun idstr -> idstr |> Guid.Parse |> MeetingId.box)
            
            let requests =
                match msgmap |> Map.tryFind msgid with
                | Some requests -> requests
                | _ -> []

            msgmap
            |> Map.add msgid (
                {   RequestInfo.MessageId = msgid
                    RequestInfo.Name = row.Name
                    RequestInfo.Request = Request.Available (row.Description, meetingIds)
                }::requests)) Map.empty<FsGuidType, RequestInfo list>
        
    let joinString separator (strings:string[]) = String.Join (separator, strings)
    use writer = new System.IO.StreamWriter (messagesReportFileName)

    messagesFile.Rows 
    |> Seq.map (fun row ->
        let msgid = row.``Message Id`` |> Guid.Parse |> MessageId.box
            
        let dayOffDates =
            match dayOffRequests |> Map.tryFind msgid with
            | Some(requests) -> 
                requests
                |> Seq.map (fun request ->
                    match request.Request with
                    | Unavailable (meetingId) -> meetingId
                    | _ -> failwith "Unexpected request" )
                |> Seq.map (fun mtgid ->
                    meetings 
                    |> Map.find mtgid
                    |> fun mtg -> mtg.Date.ToString " * MMM dd, yyyy") 
                |> Seq.toArray
                |> joinString "\n"
            | None -> ""

        let roles =
            match roleRequests |> Map.tryFind msgid with
            | Some(requests) ->
                requests
                |> Seq.map (fun request ->
                    match request.Request with
                    | Available (description, meetingIds) -> description, meetingIds
                    | _ -> failwith "Unexpected request" )
                |> Seq.map (fun (description, mtgids) ->
                    mtgids
                    |> Seq.map (fun mtgid -> 
                        meetings 
                        |> Map.find mtgid
                        |> fun mtg -> mtg.Date |> interpret) 
                    |> Seq.toArray
                    |> joinString "; "
                    |> fun dates ->
                        sprintf """
%s requested for: %s
"""                         description dates) 
                |> Seq.toArray
                |> joinString "\n"
            | None -> ""

        sprintf """
-------------------------
Member: %s
Message: 
%s

Not Attending
%s

%s
            """ 
                row.Name
                row.Message
                dayOffDates
                roles
                
            |> writer.Write
        ) 
        |> Seq.toList
        |> ignore


[<EntryPoint>]
let main argv = 
    // System set up
    NewtonsoftHack.resolveNewtonsoft ()    
    let system = Configuration.defaultConfig () |> System.create "sample-system"
            
    let actorGroups = composeActors system
    
    // Sample data
    let userId = Persistence.Users.findUserId "ToastmastersRecord.SampleApp.MessageProcessor" 
    
    actorGroups |> generateMeetings system userId
    actorGroups |> addIdToMemberMessages system userId
    // actorGroups |> ingestMembers system userId
    //actorGroups |> ingestMemberMessages system userId 


    buildRequests ()
    actorGroups |> printMessageReport system userId 
    
    printfn "%A" argv
    0 // return an integer exit code
