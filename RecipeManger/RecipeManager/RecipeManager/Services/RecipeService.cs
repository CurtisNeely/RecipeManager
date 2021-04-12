using RecipeManager.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeManager.Services
{
    public class RecipeService
    {
        readonly ApplicationDbContext _RecipeContext;

        public RecipeService(ApplicationDbContext recipeContext)
        {
            _RecipeContext = recipeContext;
        }

        public bool RecipeExists(long id)
        {
            return _RecipeContext.Recipes.Any(e => e.Id == id);
        }

        public bool IsEvenNumber(int number)
        {
            return (number % 2) == 0;
        }
    }
}
