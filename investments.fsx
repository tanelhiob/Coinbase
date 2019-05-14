#load "client.fsx"
#load "credentials.fsx"
#load "providers.fsx"

open System
open FSharp.Data
open Client
open Providers

fsi.ShowDeclarationValues <- false

let get = getRaw Credentials.apiKey Credentials.apiSecret Credentials.apiUrl Credentials.apiVersion

let accounts = get "/v2/accounts" |> AccountsProvider.Parse

let interestingCoins = ["BTC"; "ETH"; "LTC"; "XRP"]

let getPrice coin =
    let price = get (sprintf "/v2/prices/%s-EUR/spot" coin) |> PriceProvider.Parse
    price.Data.Amount

let coinPrices =
    interestingCoins
    |> List.map getPrice
    |> List.zip interestingCoins

let rec getTransactions page accountId = 
    let url = page |> Option.defaultValue (sprintf "/v2/accounts/%s/transactions" accountId)
    let transactions = get url |> TransactionsProvider.Parse
    if not (String.IsNullOrEmpty transactions.Pagination.NextUri) then
        let head = transactions.Data
        let tail = getTransactions (Some transactions.Pagination.NextUri) accountId
        Array.concat [head; tail]
    else
        transactions.Data

let accountTransactions =
    accounts.Data
    |> Array.map (fun account -> account.Id.JsonValue.AsString())
    |> Array.map (getTransactions None)
    |> Array.zip accounts.Data

let printTransaction (t: TransactionsProvider.Datum) = 
    printfn "%s %s %s %f%s (%.2f%s) %s = %.2f€"
        (t.CreatedAt.ToString("dd-MM-yyyy"))
        t.Details.Title
        t.Details.Subtitle
        t.Amount.Amount
        t.Amount.Currency
        t.NativeAmount.Amount
        t.NativeAmount.Currency
        t.Amount.Currency
        (t.NativeAmount.Amount / t.Amount.Amount)

let printAccountTransactions (account: AccountsProvider.Datum) (transactions: TransactionsProvider.Datum array) =
    let coin =  account.Currency.Code
    let coinPrice = coinPrices |> List.find (fun (c, _) -> c = coin) |> snd
    let accountValue = account.Balance.Amount * coinPrice
    printfn "%s %f %s (%.2f€) %s = %.2f€" account.Name account.Balance.Amount account.Balance.Currency accountValue coin coinPrice
    
    let bought = transactions |> Array.filter (fun t -> t.Type = "buy") |> Array.sumBy (fun t -> t.NativeAmount.Amount)
    let sold = transactions |> Array.filter (fun t -> t.Type = "sell") |> Array.sumBy (fun t -> t.NativeAmount.Amount)
    let received = transactions |> Array.filter (fun t -> t.Type = "send" && t.Amount.Amount > 0m) |> Array.sumBy (fun t -> t.Amount.Amount)
    let sent = transactions |> Array.filter (fun t -> t.Type = "send" && t.Amount.Amount < 0m) |> Array.sumBy (fun t -> t.Amount.Amount)

    let external = -(received + sent)
    let externalValue = external * coinPrice
    let totalSpent = (bought + sold)
    let totalValue = accountValue + externalValue
    let profit = totalValue - totalSpent
    let profitPercentage = profit * 100m / totalValue

    printfn "bought %.2f€; sold %.2f€, total spent %.2f€" bought sold totalSpent
    printfn "received %f%s; sent %f%s, external %f%s (%.2f€)" received coin sent coin external coin externalValue
    printfn "account value %.2f€, external value %.2f€, total value %.2f€, total spent %.2f€" accountValue externalValue totalValue totalSpent
    printfn "profit %.2f€ (%.2f%%)" profit profitPercentage

    transactions
    |> Array.sortBy (fun t -> t.CreatedAt)
    |> Array.iter printTransaction

    printfn ""

    profit

accountTransactions
|> Array.filter (fun (a, _) -> interestingCoins |> List.contains a.Currency.Code)
|> Array.map (fun (a, t) -> printAccountTransactions a t)
|> Array.sum
|> printfn "Total profit %.2f"