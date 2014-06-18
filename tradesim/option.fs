module dke.tradesim.Option

let orElseLazy (defaultValue: Lazy<Option<'t>>) (firstChoiceValue: Option<'t>): Option<'t> =
  match firstChoiceValue with
  | Some _ -> firstChoiceValue
  | None -> defaultValue.Force() 