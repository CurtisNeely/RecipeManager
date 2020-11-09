using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecipeManager.Data;
using RecipeManager.Models;
using RecipeManager.ViewModels;

namespace RecipeManager.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AnalyticsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        //The Analytics homepage. 
        //Get: recipeCount, privateRecipeCount, publicRecipeCount, userCount
        //and data to make a Recipe over time Graph
        //Return the page with an AnalticsViewModel
        public IActionResult Index(DateTime StartDate, DateTime EndDate)
        {
            var recipeCount = _context.Recipes.Count();
            var privateRecipeCount = _context.Recipes.Count(r => r.IsPublic == false);
            var publicRecipeCount = _context.Recipes.Count(r => r.IsPublic == true);
            var userCount = _context.Users.Count();

            if (StartDate == DateTime.MinValue || EndDate == DateTime.MinValue) {
                StartDate = DateTime.Now.AddDays(-60);
                EndDate = DateTime.Now.AddDays(80);
            }

            var monthCount = (((EndDate.Year - StartDate.Year) * 12) + EndDate.Month - StartDate.Month) + 1;
            var startMonth = StartDate.Month;
            var startYear = StartDate.Year;

            List<String> labels = new List<String>();
            List<int> recipeData = new List<int>();

            for (var x = 1; x <= monthCount; x++)
            {
                string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(startMonth);
                labels.Add($"{monthName} {startYear}");
                recipeData.Add(0);

                startMonth++;

                if (startMonth == 13)
                {
                    startMonth = 1;
                    startYear++;
                }
            }           

            var recipeList = (_context.Recipes.Where(r => r.UploadDate >= StartDate && r.UploadDate<= EndDate)).Select( r => new 
            {
                Month = r.UploadDate.Month,
                Year = r.UploadDate.Year
            }).AsEnumerable().GroupBy(x => new { x.Year, x.Month });

            foreach(var i in recipeList)
            {
                string monthYear = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i.Key.Month)} {i.Key.Year}";

                int index = labels.IndexOf(monthYear);
                recipeData[index] = i.Count();
            }

            for ( int x = 0; x < labels.Count; x++)
            {
                if(x != 0)
                {
                    recipeData[x] = recipeData[x] + recipeData[x - 1];
                }
            }

            AnalyticsViewModel analytics = new AnalyticsViewModel()
            {
                recipeCount = recipeCount,
                privateRecipeCount = privateRecipeCount,
                publicRecipeCount = publicRecipeCount,
                userCount = userCount,
                startDate = labels.First(),
                endDate = labels.Last(),
                labels = JsonSerializer.Serialize(labels),
                data = JsonSerializer.Serialize(recipeData)
            };

            return View(analytics);
        }
    }
}
