module dke.tradesim.Logging

let logToStdout (msg: string): unit = printfn "%s" msg
let logToStdoutL (msg: Lazy<string>): unit = printfn "%s" (msg.Force())
let mutable verbose = logToStdout
let mutable debug = logToStdout
let mutable info = logToStdout
let mutable warn = logToStdout
let mutable error = logToStdout
let mutable verboseL = logToStdoutL
let mutable debugL = logToStdoutL
let mutable infoL = logToStdoutL
let mutable warnL = logToStdoutL
let mutable errorL = logToStdoutL

type LogLevel = Verbose | Debug | Info | Warn | Error

let setLogLevel = function
  | Verbose -> 
    verbose <- logToStdout
    debug <- logToStdout
    info <- logToStdout
    warn <- logToStdout
    error <- logToStdout
    verboseL <- logToStdoutL
    debugL <- logToStdoutL
    infoL <- logToStdoutL
    warnL <- logToStdoutL
    errorL <- logToStdoutL
  | Debug ->
    verbose <- ignore
    debug <- logToStdout
    info <- logToStdout
    warn <- logToStdout
    error <- logToStdout
    verboseL <- ignore
    debugL <- logToStdoutL
    infoL <- logToStdoutL
    warnL <- logToStdoutL
    errorL <- logToStdoutL
  | Info ->
    verbose <- ignore
    debug <- ignore
    info <- logToStdout
    warn <- logToStdout
    error <- logToStdout
    verboseL <- ignore
    debugL <- ignore
    infoL <- logToStdoutL
    warnL <- logToStdoutL
    errorL <- logToStdoutL
  | Warn ->
    verbose <- ignore
    debug <- ignore
    info <- ignore
    warn <- logToStdout
    error <- logToStdout
    verboseL <- ignore
    debugL <- ignore
    infoL <- ignore
    warnL <- logToStdoutL
    errorL <- logToStdoutL
  | Error ->
    verbose <- ignore
    debug <- ignore
    info <- ignore
    warn <- ignore
    error <- logToStdout
    verboseL <- ignore
    debugL <- ignore
    infoL <- ignore
    warnL <- ignore
    errorL <- logToStdoutL
