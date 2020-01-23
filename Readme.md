# !WARN. Not ready early alpha version

# MyTqdm

tqdm style wrapper for Goblinfactory.Konsole

# Usage

# FSharp Example

```fsharp
open MyTqdm

let isIDE = fsi.CommandLineArgs |> Seq.length < 2

let progressFactory = 
    if isIDE then 
        MyTqdm.ConsoleForwardOnlyProgress 
    else 
        MyTqdm.ConsoleProgress



module Tests =
    let private slow(s) =
        s
        |> Seq.map(fun i ->
            System.Threading.Thread.Sleep(500)
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
        let withProgress = s.WithProgress(progressFactory, title,   total =  (total |> System.Nullable.op_Implicit))
        
        printfn "testWithExplicitTotal"
        withProgress |> slowList

open Tests

test("Simple seq", s1)

test("List ", s2)


```

# CSharp Example

// TODO