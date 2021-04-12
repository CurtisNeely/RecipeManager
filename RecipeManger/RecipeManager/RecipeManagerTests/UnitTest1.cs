using Moq;
using RecipeManager.Data;
using RecipeManager.Services;
using System;
using Xunit;

namespace RecipeManagerTests
{
    public class BasicTests
    {
        //Unit test project setup and example tests implemented testing a
        //RecipeService method.
        private readonly ApplicationDbContext _Context;

        [Fact]
        public void IsEven_InputIs10_ReturnTrue()
        {
            var _RecipeService = new RecipeService(_Context);
            bool result = _RecipeService.IsEvenNumber(10);

            Assert.True(result, "10 is even");
        }

        [Fact]
        public void IsEven_InputIs13_ReturnFalse()
        {
            var _RecipeService = new RecipeService(_Context);
            bool result = _RecipeService.IsEvenNumber(13);

            Assert.False(result, "13 is not even");
        }
    }
}
