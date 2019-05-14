#r "packages/FSharp.Data/lib/net45/FSharp.Data.dll"

open System
open System.Security.Cryptography
open System.Text
open FSharp.Data

let getTimestamp () =
    DateTimeOffset.UtcNow.ToUnixTimeSeconds()

let bytesToHexString bytes =
    bytes |> Array.map (fun (b: Byte) -> b.ToString("x2")) |> String.concat ""

let getSignature (apiSecret: string) timestamp method path body =
    let prehash = sprintf "%i%s%s%s" timestamp method path body

    let key = Encoding.UTF8.GetBytes apiSecret
    let data = Encoding.UTF8.GetBytes prehash

    use hmac = new HMACSHA256(key)
    hmac.ComputeHash data |> bytesToHexString

let getRaw apiKey apiSecret apiUrl apiVersion path =
    let method = HttpMethod.Get
    let timestamp = getTimestamp ()
    let signature = getSignature apiSecret timestamp method path ""
    
    let headers = [
        HttpRequestHeaders.ContentType HttpContentTypes.Json
        ("CB-ACCESS-KEY", apiKey)
        ("CB-ACCESS-SIGN", signature)
        ("CB-ACCESS-TIMESTAMP", timestamp.ToString())
        ("CB-VERSION", apiVersion)
    ]

    let url = sprintf "%s%s" apiUrl path
    
    Http.RequestString (url, headers = headers, httpMethod = method)
