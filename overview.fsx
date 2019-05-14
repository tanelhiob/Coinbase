#load "client.fsx"
#load "providers.fsx"

open Client
open Credentials
open Providers

fsi.ShowDeclarationValues <- false

let get = getRaw Credentials.apiKey Credentials.apiSecret Credentials.apiUrl Credentials.apiVersion

let accounts = AccountsProvider.GetSample().Data

let printAccount (account: AccountsProvider.Datum, price: decimal) = 
    printfn "%s %f%s (%.2f€)" account.Name account.Balance.Amount account.Balance.Currency price
    
let attachPrice (account: AccountsProvider.Datum) =
    let price = get (sprintf "/v2/prices/%s-EUR/spot" account.Currency.Code) |> PriceProvider.Parse
    (account, price.Data.Amount * account.Balance.Amount)

accounts
|> Array.sortByDescending (fun a -> a.UpdatedAt)
|> Array.filter (fun a -> a.Balance.Amount <> 0m)
|> Array.map attachPrice
|> Array.map (fun (a, p) -> printAccount (a,p); p)
|> Array.sum
|> printfn "Total value: %.2f"