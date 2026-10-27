open System.IO
open System.IO.Compression

let outputName = "artifact.zip"
let excludedDirs = [| ".vscode"; "Source"; ".git"; ".sharplsp" |]
let excludeFiles = [| "mod.slnx"; ".gitignore"; "pub.fsx" |]

let shouldExclude (relPath: string) =
    excludedDirs |> Array.contains relPath || excludeFiles |> Array.contains relPath

let packRoot (zipName: string) =
    let zipName = Path.GetFileName zipName

    use zipStream = new FileStream(zipName, FileMode.Create, FileAccess.Write)
    use archive = new ZipArchive(zipStream, ZipArchiveMode.Create)

    // rootFiles-> pure file names-> exclude zip itself
    let rootFiles =
        Directory.EnumerateFiles "."
        |> Seq.map Path.GetFileName
        |> Seq.filter (fun name -> not (shouldExclude name) && name <> zipName)
        |> Seq.map (fun name -> name, name)

    let subDirFiles =
        Directory.EnumerateDirectories "."
        |> Seq.map Path.GetFileName
        |> Seq.filter (fun dirName -> not (shouldExclude dirName))
        |> Seq.collect (fun dirName ->
            Directory.EnumerateFiles(dirName, "*", SearchOption.AllDirectories)
            |> Seq.map (fun file -> file, file.Replace('\\', '/')))

    Seq.append rootFiles subDirFiles
    |> Seq.iter (fun (filePath, entryName) ->
        archive.CreateEntryFromFile(filePath, entryName, CompressionLevel.Optimal)
        |> ignore)

    printfn "打包完成: %s" zipName


packRoot outputName
