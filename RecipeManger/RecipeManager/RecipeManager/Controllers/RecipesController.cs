using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RecipeManager.Data;
using RecipeManager.Models;
using RecipeManager.Services;

namespace RecipeManager.Controllers
{
    public class RecipesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RecipeService _RecipeService;

        public RecipesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RecipeService recipeService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _RecipeService = recipeService;
        }

        //Returns the My Recipes page for the user. 
        //Gets all the user's uploaded recipes.
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return View(await _context.Recipes.Where(r => r.UserId == userId).ToListAsync());
        }

        //Returns the Favourites page for the user. 
        //Gets all the user's favourited recipes.
        [Authorize]
        public async Task<IActionResult> Favourites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var recipes = await _context.Recipes
                    .Join(
                        _context.Favourites,
                        recipe => recipe.Id,
                        favourite => favourite.RecipeId,
                        (recipe, favourite) => new { recipe, favourite })
                    .Where(x => x.favourite.UserId == userId) //&& x.recipe.IsPublic == true
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
                    ).ToListAsync();

            return View(recipes);
        }

        //If a user clicks the 'add to favourites' button while searching
        //the recipe will be added to the user's favourites, and the user
        //will be redirected to their Favourites page
        [Authorize]
        public async Task<IActionResult> AddToFavourites(long id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Favourite favourite = new Favourite() { RecipeId = id, UserId = userId };

            var exists = await _context.Favourites.FirstOrDefaultAsync(f => f.RecipeId == id && f.UserId == userId);

            if(exists == null)
            {
                _context.Favourites.Add(favourite);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Favourites", "Recipes");
        }

        //If a user clicks the 'remove from favourites' button while on their favourites page
        //the recipe will be removed from the user's favourites, and the user
        //will be redirected to their Favourites page
        [Authorize]
        public async Task<IActionResult> RemoveFromFavourites(long id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var favourite = await _context.Favourites.FirstOrDefaultAsync(f => f.RecipeId == id && f.UserId == userId);

            if (favourite != null)
            {
                _context.Favourites.Remove(favourite);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Favourites", "Recipes");
        }

        //Returns the Featured page for the admin
        //Gets all featured recipes 
        [Authorize]
        public async Task<IActionResult> Featured()
        {
            var recipes = await _context.Recipes.Where(r => r.IsFeatured == true).ToListAsync();

            return View(recipes);
        }

        //If an admin clicks the 'add to featured' button while searching
        //the recipe will be added to the admin's featured, and the admin
        //will be redirected to their Featured page
        [Authorize]
        public async Task<IActionResult> AddToFeatured(long id)
        {
            var recipe = await _context.Recipes.FirstOrDefaultAsync(f => f.Id == id);

            if (recipe != null)
            {
                recipe.IsFeatured = true;
                _context.Recipes.Update(recipe);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Featured", "Recipes");
        }

        //If an admin clicks the 'remove from featured' button while on the featured page
        //the recipe will be removed from the admin's featured, and the admin
        //will be redirected to their Featured page
        [Authorize]
        public async Task<IActionResult> RemoveFromFeatured(long id)
        {
            var recipe = await _context.Recipes.FirstOrDefaultAsync(f => f.Id == id);

            if (recipe != null)
            {
                recipe.IsFeatured = false;
                _context.Recipes.Update(recipe);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Featured", "Recipes");
        }

        //Get: Detailed recipe. Returns a view that contains the
        //recipe, ingredients, steps, categories, and rating
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipes
                .FirstOrDefaultAsync(m => m.Id == id);

            recipe.Ingredients = await _context.Ingredients.Where(i => i.RecipeId == recipe.Id).ToListAsync();
            recipe.Steps = await _context.Steps.Where(s => s.RecipeId == recipe.Id).ToListAsync();
            recipe.Categories = await _context.Categories.Where(c => c.RecipeId == recipe.Id).ToListAsync();

            if (recipe == null)
            {
                return NotFound();
            }

            return View(recipe);
        }

        // GET: Returns the Recipe Creation Page
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Recipes/Create
        // If a valid recipe is submitted by the user, add it to the db, redirect them to their
        // My recipes page
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Recipe recipe, IFormFile Photo, String Name, String Time, String Servings, String Description, bool IsPublic, String[] IngredientAmount, String[] IngredientName, String[] StepDescription, String[] Categories)
        {

            //if (ModelState.IsValid)
            if(Photo != null && recipe.Name != null && recipe.Time != null && recipe.Servings != null && recipe.Description != null)
            {
                List<Ingredient> ingredients = new List<Ingredient>();
                List<Step> steps = new List<Step>();
                List<Category> categories = new List<Category>();

                for (int x = 0; x < IngredientAmount.Length; x++)
                {
                    ingredients.Add(new Ingredient(IngredientAmount[x], IngredientName[x]));
                }

                for (int x = 0; x < StepDescription.Length; x++)
                {
                    steps.Add(new Step(x + 1, StepDescription[x]));
                }

                for (int x = 0; x < Categories.Length; x++)
                {
                    categories.Add(new Category() { Name = Categories[x] });
                }

                byte[] p1 = null;
                using (var fs1 = Photo.OpenReadStream())
                using (var ms1 = new MemoryStream())
                {
                    fs1.CopyTo(ms1);
                    p1 = ms1.ToArray();
                }

                var user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));

                recipe.Photo = p1;
                recipe.Ingredients = ingredients;
                recipe.Steps = steps;
                recipe.Categories = categories;
                recipe.UserId = user.Id;
                recipe.UploadDate = DateTime.UtcNow;
                recipe.RatingAverage = 0;
                recipe.RatingCount = 0;
                recipe.UploaderName = user.FirstName;

                _context.Add(recipe);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Recipes");
            }

            return View(recipe);
        }

        // Get: a recipe, and return the user to the Editing page for that recipe
        [Authorize]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipes.FindAsync(id);
            recipe.Ingredients = await _context.Ingredients.Where(i => i.RecipeId == recipe.Id).ToListAsync();
            recipe.Steps = await _context.Steps.Where(s => s.RecipeId == recipe.Id).ToListAsync();
            recipe.Categories = await _context.Categories.Where(c => c.RecipeId == recipe.Id).ToListAsync();

            if (recipe == null)
            {
                return NotFound();
            }

            return View(recipe);
        }

        // POST: Recipes/Edit
        // If a valid recipe edit is submitted by the user, update the db, redirect them to their
        // My recipes page
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(long id, Recipe recipe, IFormFile NewPhoto, String[] IngredientAmount, String[] IngredientName, String[] StepDescription, String[] Categories)
        {
            var oldIngredients = await _context.Ingredients.Where(i => i.RecipeId == recipe.Id).ToListAsync();
            var oldSteps = await _context.Steps.Where(s => s.RecipeId == recipe.Id).ToListAsync();
            var oldCategories = await _context.Categories.Where(c => c.RecipeId == recipe.Id).ToListAsync();

            if (id != recipe.Id)
            {
                return NotFound();
            }

            if (recipe.Name != null && recipe.Time != null && recipe.Servings != null && recipe.Description != null)
            {
                List<Ingredient> ingredients = new List<Ingredient>();
                List<Step> steps = new List<Step>();
                List<Category> categories = new List<Category>();

                for (int x = 0; x < IngredientAmount.Length; x++)
                {
                    ingredients.Add(new Ingredient(IngredientAmount[x], IngredientName[x]));
                }

                for (int x = 0; x < StepDescription.Length; x++)
                {
                    steps.Add(new Step(x + 1, StepDescription[x]));
                }

                for (int x = 0; x < Categories.Length; x++)
                {
                    categories.Add(new Category() { Name = Categories[x] });
                }

                if(NewPhoto != null)
                {
                    byte[] p1 = null;
                    using (var fs1 = NewPhoto.OpenReadStream())
                    using (var ms1 = new MemoryStream())
                    {
                        fs1.CopyTo(ms1);
                        p1 = ms1.ToArray();
                    }

                    recipe.Photo = p1;
                }

                var user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));

                recipe.Ingredients = ingredients;
                recipe.Steps = steps;
                recipe.Categories = categories;

                try
                {
                    _context.RemoveRange(oldIngredients);
                    _context.RemoveRange(oldSteps);
                    _context.RemoveRange(oldCategories);
                    _context.Update(recipe);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecipeExists(recipe.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(recipe);
        }

        // GET: Recipes/Delete
        // Comfirm deletion of a selected recipe
        [Authorize]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (recipe == null)
            {
                return NotFound();
            }

            return View(recipe);
        }

        // POST: Recipes/Delete/5
        //Delete a selected recipe
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Recipes/Search/pizza
        // Search for recipes who's name contains a search phrase
        public async Task<IActionResult> SearchByName(string SearchPhrase, int? pageNumber)
        {
            if(SearchPhrase == null)
            {
                return RedirectToAction("Index","Home");
            }

            ViewBag.SearchPhrase = SearchPhrase;
            ViewBag.Action = "SearchByName";
            int pageSize = 3;

            var recipe = _context.Recipes.Where(r => r.Name.ToLower().Contains(SearchPhrase.ToLower()) && r.IsPublic == true).AsQueryable();
            ViewBag.ResultCount = recipe.Count();

            if (recipe == null)
            {
                return NotFound();
            }

            return View("Search", await PaginatedList<Recipe>.CreateAsync(recipe.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        //Search for recipes who contain a category
        public async Task<IActionResult> SearchByCategory(string Category, int? pageNumber)
        {
            ViewBag.SearchCategory = Category;
            ViewBag.Action = "SearchByCategory";
            int pageSize = 3;

            var recipe = _context.Recipes
                    .Join(
                        _context.Categories,
                        recipe => recipe.Id,
                        category => category.RecipeId,
                        (recipe, category) => new { recipe, category })
                    .Where(x => x.category.Name == Category && x.recipe.IsPublic == true)
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

            ViewBag.ResultCount = recipe.Count();

            if (recipe == null)
            {
                return NotFound();
            }

            return View("Search", await PaginatedList<Recipe>.CreateAsync(recipe.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        //Search for a recipe, and then filter the results by category
        public async Task<IActionResult> SearchByNameAndCategory(string SearchPhrase, string Category, int? pageNumber)
        {

            if (Category != null && SearchPhrase != null)
            {
                ViewBag.SearchPhrase = SearchPhrase;
                ViewBag.SearchCategory = Category;
                ViewBag.Action = "SearchByNameAndCategory";
                int pageSize = 3;

                //var recipe = await _context.Recipes.FromSqlInterpolated($"SELECT r.* FROM dbo.Recipes r JOIN dbo.Categories c ON r.Id = c.RecipeId WHERE r.Name = '{word}'").ToListAsync();

                var recipe = _context.Recipes
                    .Join(
                        _context.Categories,
                        recipe => recipe.Id,
                        category => category.RecipeId,
                        (recipe, category) => new {recipe, category})
                    .Where(x => x.category.Name == Category && x.recipe.Name.ToLower().Contains(SearchPhrase.ToLower()) && x.recipe.IsPublic == true)
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

                ViewBag.ResultCount = recipe.Count();

                if (recipe == null)
                {
                    return NotFound();
                }

                return View("Search", await PaginatedList<Recipe>.CreateAsync(recipe.AsNoTracking(), pageNumber ?? 1, pageSize));

            }
            else
            {
                return RedirectToAction(nameof(SearchByCategory), new { Category = Category });
            }

        }

        //Rate a recipe. If the user has already rated, update the rating, 
        //else create a new rating. Reload the recipe's page
        [Authorize]
        public async Task<IActionResult> Rate(long Id, int stars)
        {
            if (stars > 5)
                stars = 5;
            else if(stars < 1)
                stars = 1;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Rating rating = new Rating() { RecipeId = Id, Stars = stars, UserId = userId };

            var exists = await _context.Ratings.FirstOrDefaultAsync(r => r.RecipeId == rating.RecipeId && r.UserId == rating.UserId);

            if(exists == null)
            {
                _context.Ratings.Add(rating);
                await _context.SaveChangesAsync();
            }
            else
            {
                exists.Stars = rating.Stars;
                _context.Ratings.Update(exists);
                await _context.SaveChangesAsync();
            }

            var ratingCount = _context.Ratings.Count(r => r.RecipeId == rating.RecipeId);
            var ratingAverage = Math.Round(_context.Ratings.Where(r => r.RecipeId == rating.RecipeId).Average(r => r.Stars), 0);

            var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == Id);

            if (recipe == null)
            {
                return NotFound();
            }

            recipe.RatingCount = ratingCount;
            recipe.RatingAverage = ratingAverage;

            _context.Recipes.Update(recipe);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { Id = Id });
        }

        //Reverse a recipe's public or private status
        [Authorize]
        public async Task<IActionResult> ReverseIsPublic(long Id)
        {
            var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == Id);

            if (recipe == null)
            {
                return NotFound();
            }

            if (recipe.IsPublic)
                recipe.IsPublic = false;
            else
                recipe.IsPublic = true;

            _context.Recipes.Update(recipe);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        //Check if a recipe exists
            private bool RecipeExists(long id)
        {
            return _context.Recipes.Any(e => e.Id == id);
        }
    }
}
