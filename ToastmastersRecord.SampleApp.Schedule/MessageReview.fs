module ToastmastersRecord.SampleApp.Schedule.MessageReview

open System

open Akka.Actor
open Akka.FSharp

open ToastmastersRecord.Domain.Infrastructure
open ToastmastersRecord.Domain.DomainTypes
open ToastmastersRecord.Data.Entities
open ToastmastersRecord.Domain

open ToastmastersRecord.Domain.RoleRequests
open ToastmastersRecord.Domain.MemberMessages

let displayMessage userId (messageInfo:(MessageId * MemberId * string * DateTime * string) * (MeetingId * DateTime) list * (RoleRequestId * string * int * ((MeetingId * DateTime) list)) list) = 
    let (msgId, memId, name, date, message), daysOff, requests = messageInfo
    printfn """
%s said:
%s
----
"""
        name
        message

    // Days off
    printfn "Days off"
    printfn "----------"
    daysOff 
    |> Seq.iter (fun dayOff -> 
        dayOff 
        |> snd
        |> fun d -> d.ToString "MMM dd, yyyy"
        |> printfn "%s")

    // Special requests
    printfn "----------"
    printfn "Instructions"
    printfn "----------"
    requests 
    |> Seq.iter (fun (id, description, state, r) ->
        printfn "`%s` has state %d" description state
        r 
        |> Seq.iter (fun days ->
            days
            |> snd
            |> fun d -> d.ToString "MMM dd, yyyy"
            |> printfn "    %s"))

    printfn "#####################################################"
    
let reviewMessages userId (dayOffsRequestReply:IActorRef) (roleRequestsRequestReplyCreated:IActorRef) = 
    let messages = Persistence.MemberMessages.fetch ("12/1/2017" |> DateTime.Parse)
    let unscheduledMeetings = 
        Persistence.ClubMeetings.fetchOpenMeetings ()
        |> List.toArray
    let interpret = interpret "NA" "MMM dd, yyyy"

    messages 

    // Remove requested days off
    |> Seq.map (fun message ->
    
        let _, daysOff, _ = message
        message, 
        unscheduledMeetings
        |> Array.filter (fun meeting ->
            daysOff
            |> List.map snd
            |> List.contains meeting.Date
            |> not))

    // Request days off
    |> Seq.map (fun (message, unscheduled) ->         
        let rec procmessage messageInfo (unscheduled':ClubMeetingEntity [])=                                         
            displayMessage userId messageInfo

            let (msgId, memId, name, date, message), daysOff, requests = messageInfo

            printfn "Unscheduled meetings"
            unscheduled'
            |> Array.iteri (fun i meeting ->
                printfn "%d) %s" i (meeting.Date |> interpret))
            |> ignore

            printfn "Which meetings will you NOT attend?"
            match Console.ReadLine () |> Int32.TryParse with
            | true, n when n < unscheduled'.Length ->
                let meeting = unscheduled'.[n]
                (memId, meeting.Id |> MeetingId.box, msgId)
                |> DayOffRequestCommand.Create
                |> envelopWithDefaults
                    (userId)
                    (TransId.create ())
                    (StreamId.create ())
                    (Version.box 0s)
                |> dayOffsRequestReply.Ask
                |> Async.AwaitTask
                |> Async.Ignore
                |> Async.RunSynchronously

                let m = unscheduled' |> Array.find (fun m -> m.Id = meeting.Id)
                let messageInfo' = 
                    (msgId, memId, name, date, message),
                    (m.Id |> MeetingId.box, m.Date)::daysOff,
                    requests

                unscheduled'
                |> Array.filter (fun m -> m.Id = meeting.Id |> not)
                |> procmessage messageInfo'

            | _ -> 
                printfn "input not recognized"
                messageInfo, unscheduled'

        

        procmessage message unscheduled)

    // Make role requests
    |> Seq.map (fun (message, unscheduled) -> 
        let rec procmessage messageInfo =
            displayMessage userId messageInfo

            let (msgId, memId, name, date, message), daysOff, requests = messageInfo

            printfn "Unscheduled meetings"
            unscheduled
            |> Array.iteri (fun i meeting ->
                printfn "%d) %s" i (meeting.Date |> interpret))
            |> ignore

            printfn "Which meetings would you like to request?"
            let response = Console.ReadLine ()
            if response |> String.IsNullOrWhiteSpace then ()
            else
                let selected =
                    response.Split ','
                    |> Seq.map (fun r ->
                        let i = r.Trim () |> Int32.Parse
                        unscheduled.[i].Id
                        |> MemberId.box
                        )
                    |> Seq.toList                    

                printfn "What description is this request?"
                let description = Console.ReadLine ()

                (memId, msgId, description, selected)
                |> RoleRequestCommand.Request 
                |> envelopWithDefaults
                    (userId)
                    (TransId.create ())
                    (StreamId.create ())
                    (Version.box 0s)
                |> roleRequestsRequestReplyCreated.Ask
                |> Async.AwaitTask
                |> Async.Ignore
                |> Async.RunSynchronously

                let messageInfo' = (msgId, memId, name, date, message), daysOff, requests

                // TODO: Build requests 

                procmessage messageInfo'

        procmessage message)
    |> Seq.toList
    |> ignore
