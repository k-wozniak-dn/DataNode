
using DataNode.Core;

namespace DataNode.Tests;

public class UnitTest1
{
    [Fact]
    public void Copy_Correctly()
    {
        // Arrange
        var dataNode = new Core.DataNode();
        var item1 = new Item("item1", dataNode);

        // Act
        item1.Set("Name", "Apples");
        item1.Set("Price", 1.5m);
        item1.Set("Quantity", 10);

        var item2 = item1.Copy("item2", dataNode );
        item2.Set("Brand", "Sunny");
        item2.Set("Quantity", 20);

        dataNode.Set("item1", item1);
        dataNode.Set("item2", item2);

        // Assert
        var count = dataNode.GetAll().Sum(kvp => kvp.Value.Count());
        var total = dataNode.GetAll().Sum(kvp => kvp.Value.GetDecimal("Price") * kvp.Value.GetInteger("Quantity"));
        Assert.Equal(7, count); 
        Assert.Equal(45m, total); 
    }

    [Fact]
    public void GetOrCreateKey_Correctly()
    {
        // Arrange
        var dataNode = new Core.DataNode();
        var item1 = dataNode.GetOrCreate("item1") ;
        var item2 = dataNode.GetOrCreate("item2") ;

        // Act
        item1.Set("Name", "Apples");
        item1.Set("Price", 1.5m);
        item1.Set("Quantity", 10);

        item2.Set("Name", "Apples");
        item2.Set("Price", 1.5m);
        item2.Set("Brand", "Sunny");
        item2.Set("Quantity", 20);

        // Assert
        var count = dataNode.GetAll().Sum(kvp => kvp.Value.Count());
        var total = dataNode.GetAll().Sum(kvp => kvp.Value.GetDecimal("Price") * kvp.Value.GetInteger("Quantity"));
        Assert.Equal(7, count); 
        Assert.Equal(45m, total); 
    }


}