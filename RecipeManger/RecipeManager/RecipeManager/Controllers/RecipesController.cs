using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RecipeManager.Data;
using RecipeManager.Models;

namespace RecipeManager.Controllers
{
    public class RecipesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RecipesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Recipes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Recipes.ToListAsync());
        }

        // GET: Recipes/Details/5
        public async Task<IActionResult> Details(long? id)
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

        // GET: Recipes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Recipes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile Photo, String Name, String Time, String Servings, String Description, bool IsPublic, String[] IngredientAmount, String[] IngredientName, String[] StepDescription, String[] Categories)
        {

            Recipe recipe = new Recipe()
            {
                Name = Name,
                Time = Time,
                Servings = Servings,
                Description = Description,
                IsPublic = IsPublic,
                IsFeatured = false,
                UploadDate = DateTime.UtcNow              
            };

            if (ModelState.IsValid && Photo != null)
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

                recipe.Photo = p1;
                recipe.Ingredients = ingredients;
                recipe.Steps = steps;
                recipe.Categories = categories;
                recipe.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                _context.Add(recipe);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Recipes");
            }

            return View(recipe);
        }

        // GET: Recipes/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }
            return View(recipe);
        }

        // POST: Recipes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Name,Time,Servings,Description,Photo,IsPublic,IsFeatured,UploadDate,UserId")] Recipe recipe)
        {
            if (id != recipe.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
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

        // GET: Recipes/Delete/5
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
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RecipeExists(long id)
        {
            return _context.Recipes.Any(e => e.Id == id);
        }
    }
}
