using RecipeManager.Data;
using RecipeManager.Models;
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

        public IQueryable<Recipe> SearchRecipesByCategory(string category)
        {
            var recipes = _RecipeContext.Recipes
                    .Join(
                        _RecipeContext.Categories,
                        recipe => recipe.Id,
                        category => category.RecipeId,
                        (recipe, category) => new { recipe, category })
                    .Where(x => x.category.Name == category && x.recipe.IsPublic == true)
                    .Select(x => new Recipe
                    {
                        Id = x.recipe.Id,
                        Name = x.recipe.Name,
                        Time = x.recipe.Time,
                        Servings = x.recipe.Servings,
                        Description = x.recipe.Description,
                        Photo = x.recipe.Photo,
                        IsPublic = x.recipe.IsPublic,
                        IsFeatured = x.recipe.IsFeatured,
                        UploadDate = x.recipe.UploadDate,
                        RatingCount = x.recipe.RatingCount,
                        RatingAverage = x.recipe.RatingAverage
                    }
                    ).AsQueryable();

            return recipes;
        }

        public IQueryable<Recipe> SearchRecipesByPhrase(string phrase)
        {
            return _RecipeContext.Recipes.Where(r => r.Name.ToLower().Contains(phrase.ToLower()) && r.IsPublic == true).AsQueryable();
        }

        public IQueryable<Recipe> SearchRecipesByPhraseAndFilter(string phrase, string filter)
        {
            var recipes = _RecipeContext.Recipes
                    .Join( _RecipeContext.Categories,
                           recipe => recipe.Id,
                           category => category.RecipeId,
                           (recipe, category) => new { recipe, category })
                    .Where( x => x.category.Name == filter && x.recipe.Name.ToLower().Contains(phrase.ToLower()) && x.recipe.IsPublic == true )
                    .Select( x => new Recipe
                    {
                        Id = x.recipe.Id,
                        Name = x.recipe.Name,
                        Time = x.recipe.Time,
                        Servings = x.recipe.Servings,
                        Description = x.recipe.Description,
                        Photo = x.recipe.Photo,
                        IsPublic = x.recipe.IsPublic,
                        IsFeatured = x.recipe.IsFeatured,
                        UploadDate = x.recipe.UploadDate,
                        RatingCount = x.recipe.RatingCount,
                        RatingAverage = x.recipe.RatingAverage
                    })
                    .AsQueryable();

            return recipes;
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
