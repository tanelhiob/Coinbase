open FSharp.Data

type UserProvider = JsonProvider<"data/user.json">
type AccountsProvider = JsonProvider<"data/accounts.json">
type TransactionsProvider = JsonProvider<"data/transactions.json">
type PaymentMethodsProvider = JsonProvider<"data/paymentsMethods.json">
type BuysProvider = JsonProvider<"data/buys.json">
type PriceProvider = JsonProvider<"data/price.json">
