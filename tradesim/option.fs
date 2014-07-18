module dke.tradesim.Option

let orElseLazy (defaultValue: Lazy<Option<'t>>) (firstChoiceValue: Option<'t>): Option<'t> =
  match firstChoiceValue with
  | Some _ -> firstChoiceValue
  | None -> defaultValue.Force()

let orElseF (defaultValue: unit -> Option<'t>) (firstChoiceValue: Option<'t>): Option<'t> =
  match firstChoiceValue with
  | Some _ -> firstChoiceValue
  | None -> defaultValue ()

// e.g. Option.flatMap (fun (e: Exchange) -> e.id) (Some ExchangeRecord)
let flatMap (f: 't -> Option<'u>) (opt: Option<'t>): Option<'u> = 
  match opt with 
  | None -> None
  | Some value -> f value
