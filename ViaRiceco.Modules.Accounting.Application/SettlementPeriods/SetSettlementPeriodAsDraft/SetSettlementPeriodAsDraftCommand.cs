using ViaRiceco.Common.Application.Abstractions;

namespace ViaRiceco.Modules.Accounting.Application.SettlementPeriods.SetSettlementPeriodAsDraft;

public sealed record SetSettlementPeriodAsDraftCommand(string Id) : ICommand;
