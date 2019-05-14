#load "client.fsx"

open FSharp.Data

type UserProvider = JsonProvider<"types/user.json">
type AccountsProvider = JsonProvider<"types/accounts.json">
type TransactionsProvider = JsonProvider<"types/transactions.json">
type PaymentMethodsProvider = JsonProvider<"types/paymentsMethods.json">
type BuysProvider = JsonProvider<"types/buys.json">
type PriceProvider = JsonProvider<"types/price.json">