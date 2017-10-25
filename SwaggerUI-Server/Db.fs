namespace DAL
module Db =
    open System.IO
    open System

    type User = { Username: string; PasswordHash :string }

    type QueryResult<'a> = Result of 'a| NotFound
    type Registration  = UserRegistered of string | UsernameAlreadyExists

    let private dbLoc =  __SOURCE_DIRECTORY__ + @"\Db.csv"

    let private users () = 
        File.ReadAllLines(dbLoc)
        |> Seq.skip 1
        |> Seq.map (fun (line: string) -> 
            let userData = line.Split([|','|], StringSplitOptions.RemoveEmptyEntries) 
            { Username = userData.[0].Trim(); PasswordHash = userData.[1].Trim()}
        )
        |> Array.ofSeq
        
    let addUser (username, password, createdBy) =         
        let newUser = username + ", " + password
        File.AppendAllLines(dbLoc, [newUser])

    let loginUser username password = 
        let byNameAndPassword u = 
            u.Username = username && u.PasswordHash = password

        match Seq.tryFind byNameAndPassword (users()) with
        | Some u -> Result u
        | _ -> NotFound

    let registerUser (username, password, createdBy) = 
        match users() |> Seq.exists (fun u -> u.Username = username) with
        | true -> UsernameAlreadyExists
        | false -> 
            addUser (username, password, createdBy) |> ignore
            UserRegistered username
