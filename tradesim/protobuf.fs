module dke.tradesim.Protobuf.FSharp

open FSharpx
open Google.ProtocolBuffers

open dke.tradesim
open Core
open Time

let convertOrderToProtobuf (order: Order): protobuf.Order =
  match order with
  | MarketBuy {time = time; securityId = securityId; qty = qty; fillPrice = fillPrice} ->
    let builder =
      protobuf.Order.CreateBuilder()
        .SetType(protobuf.Order.Types.Type.MarketBuy)
        .SetTime(dateTimeToTimestamp time)
        .SetSecurityId(securityId |> int64)
        .SetQty(qty)
    fillPrice |> Option.iter (fun fillPrice -> builder.SetFillPrice(sprintf "%M" fillPrice) |> ignore)
    builder.Build()
  | MarketSell {time = time; securityId = securityId; qty = qty; fillPrice = fillPrice} ->
    let builder =
      protobuf.Order.CreateBuilder()
        .SetType(protobuf.Order.Types.Type.MarketSell)
        .SetTime(dateTimeToTimestamp time)
        .SetSecurityId(securityId |> int64)
        .SetQty(qty)
    fillPrice |> Option.iter (fun fillPrice -> builder.SetFillPrice(sprintf "%M" fillPrice) |> ignore)
    builder.Build()
  | LimitBuy {time = time; securityId = securityId; qty = qty; fillPrice = fillPrice; limitPrice = limitPrice} ->
    let builder =
      protobuf.Order.CreateBuilder()
        .SetType(protobuf.Order.Types.Type.LimitBuy)
        .SetTime(dateTimeToTimestamp time)
        .SetSecurityId(securityId |> int64)
        .SetQty(qty)
        .SetLimitPrice(sprintf "%M" limitPrice)
    fillPrice |> Option.iter (fun fillPrice -> builder.SetFillPrice(sprintf "%M" fillPrice) |> ignore)
    builder.Build()
  | LimitSell {time = time; securityId = securityId; qty = qty; fillPrice = fillPrice; limitPrice = limitPrice} ->
    let builder =
      protobuf.Order.CreateBuilder()
        .SetType(protobuf.Order.Types.Type.LimitSell)
        .SetTime(dateTimeToTimestamp time)
        .SetSecurityId(securityId |> int64)
        .SetQty(qty)
        .SetLimitPrice(sprintf "%M" limitPrice)
    fillPrice |> Option.iter (fun fillPrice -> builder.SetFillPrice(sprintf "%M" fillPrice) |> ignore)
    builder.Build()

let convertSplitAdjustmentToProtobuf (splitAdjustment: SplitAdjustment): protobuf.SplitAdjustment =
  protobuf.SplitAdjustment.CreateBuilder()
    .SetSecurityId(splitAdjustment.securityId |> int64)
    .SetExDate(localDateToDatestamp splitAdjustment.exDate)
    .SetRatio(sprintf "%M" splitAdjustment.ratio)
    .SetAdjustmentTime(dateTimeToTimestamp splitAdjustment.adjustmentTime)
    .SetShareQtyDelta(splitAdjustment.shareQtyDelta)
    .SetCashPayout(sprintf "%M" splitAdjustment.cashPayout)
    .Build()

let convertCashDividendPaymentToProtobuf (cashDividendPayment: CashDividendPayment): protobuf.CashDividendPayment =
  let builder = 
    protobuf.CashDividendPayment.CreateBuilder()
      .SetSecurityId(cashDividendPayment.securityId |> int64)
      .SetExDate(localDateToDatestamp cashDividendPayment.exDate |> int64)
      .SetAmountPerShare(sprintf "%M" cashDividendPayment.amountPerShare)
      .SetAdjustmentTime(dateTimeToTimestamp cashDividendPayment.adjustmentTime)
      .SetShareQty(cashDividendPayment.shareQty)
      .SetTotal(sprintf "%M" cashDividendPayment.total)
  cashDividendPayment.payableDate |> Option.iter (fun payableDate -> builder.SetPayableDate(localDateToDatestamp payableDate |> int64) |> ignore)
  builder.Build()

let convertTransactionToProtobuf(transaction: Transaction): protobuf.Transaction =
  match transaction with
  | OrderTx order ->
    protobuf.Transaction.CreateBuilder()
      .SetOrder(convertOrderToProtobuf order)
      .Build()
  | SplitAdjustmentTx splitAdjustment ->
    protobuf.Transaction.CreateBuilder()
      .SetSplitAdjustment(convertSplitAdjustmentToProtobuf splitAdjustment)
      .Build()
  | CashDividendPaymentTx cashDividendPayment ->
    protobuf.Transaction.CreateBuilder()
      .SetCashDividendPayment(convertCashDividendPaymentToProtobuf cashDividendPayment)
      .Build()

let convertTransactionsToProtobuf (transactions: TransactionLog): protobuf.TransactionLog =
  protobuf.TransactionLog.CreateBuilder()
    .AddRangeTransactions(Seq.map convertTransactionToProtobuf transactions)
    .Build()

let convertPortfolioValueToProtobuf (portfolioValue: PortfolioValue): protobuf.PortfolioValue =
  protobuf.PortfolioValue.CreateBuilder()
    .SetTime(dateTimeToTimestamp portfolioValue.time)
    .SetValue(sprintf "%M" portfolioValue.value)
    .Build()

let convertPortfolioValuesToProtobuf (portfolioValues: seq<PortfolioValue>): protobuf.PortfolioValueLog =
  protobuf.PortfolioValueLog.CreateBuilder()
    .AddRangePortfolioValues(Seq.map convertPortfolioValueToProtobuf portfolioValues)
    .Build()
