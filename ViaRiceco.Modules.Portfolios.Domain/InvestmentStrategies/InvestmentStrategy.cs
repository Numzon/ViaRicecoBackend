namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

public sealed class InvestmentStrategy
{
    // financial goal - but only those with no parents - Financial goal parent Id - null
    // Financial goal gives name and goal structure, we set financial goal with no parents here,
    // but all children are also displayed here 
    // Then Retirement would be gray and would gather sum of money from all three- but you can't send money to that
    
    //every investment strategy has investment strategy type it helps to group them and filter them
    
    //every time we add, remove or update investment we use method that sets new value for RealPortfolioPercentage property 
    
    //totalInvestedAmount - sum of invested money from investments
    //totalCurrentAmount - sum of current amount from investmens 
    //UninvestedAmount - free amount that can be used to buy new assets 
    
    //investment strategy should let you modify multiple investments at once - as a list
    //every time someone wants to set modePortfolioPercentage for one ore many investments - it should check the sum 
    // if it is more that or less than 100% - it is an error 
    
}
