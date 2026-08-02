using SweetFlowerShop.Domain.Entities;
using SweetFlowerShop.Domain.Exceptions;

namespace SweetFlowerShop.Domain.Tests.Entities;

public sealed class CategoryTests
{
    [Fact]
    public void CreateChild_DerivesLevelAndParentIdentity()
    {
        var root = new Category("Flowers", "All flowers");
        var child = Category.CreateChild("Roses", "Rose varieties", root);

        Assert.Equal(root.Id, child.ParentCategoryId);
        Assert.Equal(2, child.Level);
    }

    [Fact]
    public void CreateChild_RejectsFourthLevel()
    {
        var root = new Category("One", "");
        var second = Category.CreateChild("Two", "", root);
        var third = Category.CreateChild("Three", "", second);

        Assert.Throws<InvalidCategoryLevelException>(() => Category.CreateChild("Four", "", third));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Delete_RejectsChildrenOrProducts(bool hasChildren, bool hasProducts)
    {
        var category = new Category("Flowers", "");
        Assert.Throws<InvalidOperationException>(() => category.MarkAsDeleted(hasChildren, hasProducts));
    }
}
