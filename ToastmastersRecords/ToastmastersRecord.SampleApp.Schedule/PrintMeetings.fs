module ToastmastersRecord.SampleApp.Schedule.PrintMeetings

open System 
open ToastmastersRecord.Domain
open ToastmastersRecord.Domain.Infrastructure
open ToastmastersRecord.Domain.DomainTypes
open ToastmastersRecord.Data.Entities

let displayRequests userId (meeting:ClubMeetingEntity) =
    printfn ""
    printfn "#    Name                 Request"
    printfn "---- -------------------- ----------------"
    Persistence.MemberManagement.execQuery (fun context ->
        query { 
            for rrm in context.RoleRequestMeetings do
            join r in context.RoleRequests 
                on (rrm.RoleRequestId = r.Id)
            join m in context.Members 
                on (r.MemberId = m.Id)
            join h in context.MemberHistories
                on (r.MemberId = h.Id)
            where (rrm.MeetingId = meeting.Id && r.State = 0)
            select (m,h,r) })
    |> Seq.iteri (fun i (m,h,r) ->
        printfn "%-4d %-20s %s" i h.DisplayName r.Brief
        )
   

let displayMeeting userId (meeting:ClubMeetingEntity) (placements:RolePlacementEntity seq) =
    printfn ""
    printfn "Meeting: %s\t Status: %d"
        (meeting.Date.ToString ("MMM d, yyyy"))
        meeting.State
    printfn ""
    printfn "Item TMI Id    Name                 Category    Role"
    printfn "---- --------- -------------------- ----------- ---------------------"
    placements     
    |> Seq.iteri (fun i placement ->
        let roleType = placement.RoleTypeId |> enum<RoleTypeId>
        if placement.MemberId = Guid.Empty 
        then printfn "%-4d %-9d %-20s %-11s %-20s" i 0 "--" (roleType |> category) (roleType.ToString ())
        else
            let member' = Persistence.MemberManagement.find userId (placement.MemberId |> StreamId.box)
            let history = Persistence.MemberManagement.getMemberHistory member'.Id        
            printfn "%-4d %-9d %-20s %-11s %-20s" i member'.ToastmasterId history.DisplayName (roleType |> category) (roleType.ToString ()))

let displayMeetings (meetings:ClubMeetingEntity seq) =
    printfn ""
    printfn "Item \tDate         \tStatus    "
    printfn "---- \t------------ \t--------- "
    meetings 
    |> Seq.iteri (fun i meeting ->
        printfn "%-4d \t%-10s \t%d" i (meeting.Date |> interpret "NA" "MMM dd, yyyy") meeting.State)

let processDate (f:DateTime -> unit) =
    printfn "Please enter a date"
    match Console.ReadLine () |> DateTime.TryParse with 
    | true, date -> f date
    | _ -> printfn "You entered an invalid date"

