
using DataNode.Core;
namespace DataNode.Tests;

public class UnitTest1
{
    [Fact]
    public void Copy_Correctly()
    {
        // Arrange
        var dataNode = new Core.DataNode();
        var item1 = new Item([], "item1", dataNode);

        // Act
        item1.Add("Name", "Apples");
        item1.Add("Price", 1.5m);
        item1.Add("Quantity", 10);

        var item2 = item1.Copy("item2", dataNode );
        item2.Add("Brand", "Sunny");
        item2.Set("Quantity", 20);

        dataNode.Set(item1);
        dataNode.Set(item2);

        // Assert
        var count = dataNode.GetAll().Sum(item => item.Count());
        var total = dataNode.GetAll().Sum(item => item.Get("Price")?.GetDecimal() * item.Get("Quantity")?.GetInteger());
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
        var count = dataNode.GetAll().Sum(item => item.Count());
        var total = dataNode.GetAll().Sum(item => item.Get("Price")?.GetDecimal() * item.Get("Quantity")?.GetInteger());
        Assert.Equal(7, count); 
        Assert.Equal(45m, total); 
    }

    [Fact]
    public void Construct_DataNode_withCollection_correctly()
    {
        // Arrange
        var item1 = new Item([], "item1");

        // Act
        item1.Add("Name", "Apples");
        item1.Add("Price", 1.5m);
        item1.Add("Quantity", 10);
        item1.Add("*idx", -1);

        var item2 = item1.Copy("item2");
        item2.Add("Brand", "Sunny");
        item2.Set("Quantity", 20);

        var items = new Item[] {item1, item2};
        var dn = new Core.DataNode(items);

        // Assert
        var count = dn.GetAll().Sum(item => item.Count());
        var total = dn.GetAll().Sum(item => item.Get("Price")?.GetDecimal() * item.Get("Quantity")?.GetInteger());
        Assert.Equal(7, count); 
        Assert.Equal(45m, total); 
    }

}