#I @"..\..\src\Test\bin\Debug"
#r "Goblinfactory.Konsole.dll"
#r "MyTqdm.dll"

open MyTqdm


let s1 = seq{1 .. 10}

let s2 = [1 .. 10] 

  

let isIDE = fsi.CommandLineArgs |> Seq.length < 2
if isIDE then do printfn "IDE mode" else do printfn "Normal mode"

let progressFactory = if isIDE then MyTqdm.ConsoleForwardOnlyProgress else MyTqdm.ConsoleProgress

module Tests =
    let private slow(s) =
        s
        |> Seq.map(fun i ->
            System.Threading.Thread.Sleep(507)
            i)

    let private slowList s=    
         s |> slow |> Seq.toList

    let test(title: string, s: int seq) =
        let withProgress = s.WithProgress(progressFactory, title)
        printfn "Run test 1 %s" title
        withProgress |> slowList |> ignore

        printfn "Run test 2 %s" title

        withProgress
        |> slow
        |> Seq.iter(fun i -> System.Console.WriteLine(sprintf "Line %i" i))

    let testWithExplicitTotal(title, s: int seq,total: int) =
        let withProgress = s.WithProgress(progressFactory, title, total =  (total |> System.Nullable.op_Implicit))
        
        printfn "testWithExplicitTotal"
        withProgress |> slowList

open Tests

test("Simple seq", s1)

test("List ", s2)

// testWithExplicitTotal("Simple seq with explicit", s1, 10)
