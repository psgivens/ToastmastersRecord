module ToastmastersRecord.SampleApp.SampleScript

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

open ToastmastersRecord.SampleApp.Initialize
open ToastmastersRecord.SampleApp.Infrastructure

let scriptInteractions roleRequesStreamId system actorGroups =
        onEvents system "onMemberCreated_createMemberMessage" actorGroups.MemberManagementActors.Events 
        <| fun (mailbox:Actor<Envelope<MemberManagementEvent>>) cmdenv ->
            printfn "onMemberCreated_createMemberMessage"             
            match cmdenv.Item with
            | MemberManagementEvent.Created (details) ->
                ((cmdenv.StreamId, System.DateTime.Now, "Here is a sample message from our member.")
                |> MemberMessageCommand.Create
                |> envelopWithDefaults
                    cmdenv.UserId
                    cmdenv.TransactionId
                    (StreamId.create ())
                    (Version.box 0s))
                |> actorGroups.MessageActors.Tell
            | _ -> ()
    
        // Wire up the Role Request actors    
        onEvents system "onMessageCreated_createRolerequest" actorGroups.MessageActors.Events
        <| fun (mailbox:Actor<Envelope<MemberMessageCommand>>) cmdenv ->        
            printfn "onMessageCreated_createRolerequest"
            match cmdenv.Item with
            | MemberMessageCommand.Create (mbrid, date, message) ->                
                ((mbrid, cmdenv.StreamId,"S,TM",[])
                    |> RoleRequestCommand.Request
                    |> envelopWithDefaults
                        cmdenv.UserId
                        cmdenv.TransactionId
                        roleRequesStreamId
                        (Version.box 0s))
                |> actorGroups.RoleRequestActors.Tell

let doig system actorGroups = 
    use signal = new System.Threading.AutoResetEvent false

    // Sample data
    let userId = UserId.create ()
    let memberId = TMMemberId.box 456123
    let memberStreamId = StreamId.create ()
    let roleRequesStreamId = StreamId.create ()
    let meetingStreamId = StreamId.create ()

    actorGroups |> scriptInteractions roleRequesStreamId system         
    actorGroups.MessageActors.Errors |> debugger system "messageErrors"
    actorGroups.RoleRequestActors.Errors |> debugger system "requestErrors"

    // Set the wait event when the Role Request is created
    onEvents system "onRoleRequestCreated_signal" actorGroups.RoleRequestActors.Events
    <| fun (mailbox:Actor<Envelope<RoleRequestEvent>>) cmdenv ->        
        printfn "onRoleRequestCreated_signal"
        match cmdenv.Item with
        | Requested _ ->
            signal.Set () |> ignore
        | _ -> ()

    // Set the wait event when Clug Meeting is initialized
    onEvents system "onClubMeetingEvent_Signal" actorGroups.ClubMeetingActors.Events
    <| fun (mailbox:Actor<Envelope<ClubMeetings.ClubMeetingEvent>>) cmdenv ->        
            printfn "onClubMeetingEvent_Signal"
            match cmdenv.Item with
            | Initialized _ ->
                signal.Set () |> ignore
            | _ -> ()

    // Start by creating a member    
    ({ MemberDetails.ToastmasterId = memberId;
        Name = "Phillip Scott Givens";             
        DisplayName = "Phillip Scott Givens";
        Awards="CC";
        Email="psgivens@gmail.com";
        HomePhone="949.394.2349";
        MobilePhone="949.394.2349";
        PaidUntil=System.DateTime.Now;
        ClubMemberSince=System.DateTime.Now;
        OriginalJoinDate=System.DateTime.Now;
        PaidStatus="paid";
        CurrentPosition="Vice President Education";
        SpeechCountConfirmedDate=System.DateTime.Now;
        }
        |> MemberManagementCommand.Create
        |> envelopWithDefaults
        (userId)
        (TransId.create ())
        (memberStreamId)
        (Version.box 0s))
    |> actorGroups.MemberManagementActors.Tell

    printfn "waiting on role request"
    signal.WaitOne -1 |> ignore
    printfn "role request created, done waiting"

    // [x] Query: Verify that the member exists
    let memberEntity = Persistence.MemberManagement.find userId memberStreamId
    if memberEntity = null then failwith "Member was not created"

    // [x] Query: Verify that a role request has been created 
    let requestEntity = Persistence.RoleRequests.find userId roleRequesStreamId
    if requestEntity = null then failwith "Role request was not created"

    // Create a meeting        
    ((2017,10,24)
    |> System.DateTime
    |> ClubMeetings.ClubMeetingCommand.Create
    |> envelopWithDefaults
        (userId)
        (TransId.create ())
        (meetingStreamId)
        (Version.box 0s))
    |> actorGroups.ClubMeetingActors.Tell

    printfn "waiting on club meeting"
    signal.WaitOne -1 |> ignore
    printfn "club meeting initialized, done waiting"

    // TODO: Query: Verify that role placements have been created
    let placements = Persistence.RolePlacements.findMeetingPlacements <| StreamId.unbox meetingStreamId
//    let meeting = Persistence.ClubMeetings.find userId meetingStreamId
    if placements.Length = 0 then failwith "Meeting created without role placements"
    // TODO: Command: Assign role as per request
    // TODO: Query: Verify that role request is marked assigned
    // TODO: Query: Verify that role placement is marked assigned
