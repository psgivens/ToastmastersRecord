// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.

//#load "Library1.fs"
//open ToastmastersRecord.Domain

// Define your library scripting code here


(*** Infrastructure ***)
type CommandHandlerState<'a> = (int16 * 'a list)
type CommandHandlerFunction<'a> = (CommandHandlerState<'a> -> Async<'a * CommandHandlerState<'a>>)
type CommandHandlerBuilder<'a> (raise2:int16 -> 'a -> unit) =    
    member this.Bind ((result:Async<'a>), (rest:unit -> CommandHandlerFunction<'a>)) =
        fun (version, history) -> 
            async {
                let! event = result
                raise2 version event
                let state = (version + 1s, event::history)
                return! (rest ()) state
            }
    member this.Return (result:Async<'a>) = 
        fun (version, history) -> 
            async { 
                let! event = result
                return event, (version + 1s, event::history)
            }
let raise event = async { return event }
let commandHandler = CommandHandlerBuilder 




(*** Domain ***)
type SomeEvents =
    | Created
    | Updated
    | Destroyed

let actions raiseEvent = 
    let commandHandler' = commandHandler raiseEvent
    let create = async {
                do! Async.Sleep 20
                return SomeEvents.Created
            }
    let destroy = async {
                do! Async.Sleep 20
                return SomeEvents.Destroyed
            } 
    commandHandler' {
        do! create
        do! async {
                do! Async.Sleep 20
                return SomeEvents.Updated
            }
        do! SomeEvents.Updated |> raise
        return destroy
    }


(*** Integration ***)
let raiseEvent version evt = ()
actions raiseEvent (5s,[]) |> Async.RunSynchronously |> snd

