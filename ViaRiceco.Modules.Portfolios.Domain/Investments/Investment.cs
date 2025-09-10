namespace ViaRiceco.Modules.Portfolios.Domain.Investments;

public sealed class Investment
{
    // investments are part of investment strategy - every Investment stategy can have multiple investments 
    // name - string name
    // ModelPortfolioPercentage - percentage
    // InvestedAmount - decimal (money) - sum of PurchaseRecords
    
    // PurchaseRecords - list of purchases that builds investedAmount - recalculate every time it hanges and change invested amount by summing up values 
    
    // currentAmount - decimal (money)
    
    // CurrentInvestedDifference - property that calculates difference based on invested and current
    
    // RealPortfolioPercentage - stores percentages taht are calculated by receiving new totalCurrentAmount, 
    // then it is calculated by taking current amount and checking what kind of percent of totalCurrentAmount it is
    // and we save real percent that this asset is taking in our portfolio in this field  
}
