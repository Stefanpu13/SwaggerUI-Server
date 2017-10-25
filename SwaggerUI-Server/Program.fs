open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors
open Suave.Writers
open Suave.Web
open System

open DAL.Db

let setCORSHeaders =
    setHeader "Access-Control-Allow-Origin" "*"     
    >=> setHeader "Access-Control-Allow-Headers" "content-type"     

let setCORS context = 
    context |> (
        setCORSHeaders
        >=> OK "CORS approved" )

let allowCors : WebPart =
    choose [
        OPTIONS >=> setCORS
        //GET >=> setCORS            
    ]

[<EntryPoint>]
let main argv = 
    let webPart = 
        choose (
            [
                //allowCors
                GET >=> pathScan "/webapi/2.0/ApiDocumentationUsers/Login/%s/%s" (fun (username, pass) -> 
                    match loginUser username pass with
                    | Result u -> OK (u.Username)
                    | NotFound -> BAD_REQUEST ("Could not login.")
                )
            
                POST >=> pathScan "/webapi/2.0/ApiDocumentationUsers/Register/%s/%s/%s" (fun data -> 
                    match registerUser data with
                    | UserRegistered username -> OK (username)
                    | UsernameAlreadyExists -> BAD_REQUEST ("User already exists.")
                ) >=> setCORSHeaders
            ] |> List.map ((>=>) setCORSHeaders)
        )

    startWebServer 
        ({defaultConfig with 
            bindings = 
                [ HttpBinding.createSimple HTTP "127.0.0.1" 8081]}
        ) 
        webPart
    0 // return an integer exit code
