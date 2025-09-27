namespace ViaRiceco.Modules.Budgets.Domain.Banks;

public sealed class Bank
{
    //lookup table with name only and standard type crud, take care of the fact that banks that are in use in Expanse table can't be deleted
    // bank can but doesn't have to be defined in expense - so the foreign key should be nullable 
}
