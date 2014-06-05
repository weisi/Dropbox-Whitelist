(* Dropbox Whitelist
 *
 * Weisi Dai <weisi@x-research.com>
 *
 * Usage: use cron job to launch this script.
*)

open System
open System.IO
open System.Collections.Generic

[<Literal>]
let whiteListFileNameRelative = "dropbox.wlist"

[<Literal>]
let dropboxCommandLine = "dropbox"

let HOME = Environment.GetFolderPath(Environment.SpecialFolder.Personal)
let whiteListFileName   = HOME + "/" + whiteListFileNameRelative
let dropboxFolder       = HOME + "/" + "Dropbox"
let dropboxDir          = new DirectoryInfo(dropboxFolder)
let pathSep             = string Path.DirectorySeparatorChar

let mutable toAdd       : string list = []
let mutable toRemove    : string list = []

type DictTree = Dictionary<string, WhiteListTree>
and WhiteListTree = {
             path: string;
     mutable isWhiteList: Boolean;
             sub: DictTree
}

let whiteListTree = { path = dropboxDir.ToString(); isWhiteList = false; sub = new DictTree() }

let rec addToTree tree (path: string) = 
    let segments  = path.Split(Path.DirectorySeparatorChar)
    let current   = segments.[0]
    let remaining = segments.[1..] 
                    |> String.concat (pathSep)
    if not <| tree.sub.ContainsKey(current) then
        let subTree = { 
                path = tree.path + pathSep + current;
                isWhiteList = false;
                sub = new DictTree() 
        }
        tree.sub.Add(current, subTree)
    let node = tree.sub.[current]
    if remaining.Length > 0 then
        addToTree node remaining
    else
        node.isWhiteList <- true

let isInWhiteList path = 
    let rec isInWhiteListWithRoot tree (path: string) = 
        let segments  = path.Split(Path.DirectorySeparatorChar)
        let current   = segments.[0]
        let remaining = segments.[1..] 
                        |> String.concat (pathSep)
        if tree.isWhiteList then Some(true) else
        if path.Length = 0 then Some(tree.isWhiteList) else
        if not <| tree.sub.ContainsKey(current) then None else
        if current.Length = 0 then Some(false) else
        isInWhiteListWithRoot tree.sub.[current] remaining
    isInWhiteListWithRoot whiteListTree path

let runCommand filename args = 
    System.Diagnostics.Process.Start(filename, args).WaitForExit()

let addExclude (x: string) = 
    if x.Length = 0 then () else
    runCommand dropboxCommandLine ("exclude add " + x)

let removeExclude x = 
    runCommand dropboxCommandLine ("exclude remove " + x)

let getRelativePath dir = 
    if (dir.ToString()) = dropboxDir.ToString() then "" else
    (dir.ToString()).Replace(dropboxDir.ToString() + pathSep, "")

let rec traverseTreeRemove tree = 
    toRemove <- (tree.path) :: toRemove
    for subtree in tree.sub.Values do
        traverseTreeRemove subtree

let rec traverseDir (dir: DirectoryInfo) =
    if (dir.Attributes &&& FileAttributes.Hidden) <> FileAttributes.Hidden then
        let dirIsInWhiteList = dir |> getRelativePath |> isInWhiteList
        match dirIsInWhiteList with
        | None        -> toAdd <- (dir.ToString()) :: toAdd
        | Some(false) -> for file in dir.GetFiles() do
                             if (file.Attributes &&& FileAttributes.Hidden)
                                <> FileAttributes.Hidden 
                             && (file.ToString() |> isInWhiteList) = None then
                                 toAdd <- (file.ToString()) :: toAdd
                         for subdir in dir.GetDirectories() do
                             traverseDir subdir
        | Some(true)  -> ()

let fixPath (path: string) = "'" + path.Replace("'","\\'") + "'"

let rec main() = 

    let whiteList = System.IO.File.ReadLines whiteListFileName

    for whiteListItem in whiteList do
        addToTree whiteListTree whiteListItem

    traverseDir dropboxDir

    traverseTreeRemove whiteListTree

    let toAddString    = List.map fixPath toAdd    |> String.concat " "
    let toRemoveString = List.map fixPath toRemove |> String.concat " "

    addExclude    toAddString
    removeExclude toRemoveString

    if toAdd.Length > 0 then
        toAdd    <- []
        toRemove <- []
        printfn "One more iteration needed."
        main()

main()
