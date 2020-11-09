﻿using System;
using System.Collections.Generic;
using System.Linq;
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

        public IActionResult Index(DateTime? StartDate, DateTime? EndDate)
        {
            var recipeCount = _context.Recipes.Count();
            var privateRecipeCount = _context.Recipes.Count(r => r.IsPublic == false);
            var publicRecipeCount = _context.Recipes.Count(r => r.IsPublic == true);
            var userCount = _context.Users.Count();

            var recipeList = (_context.Recipes.Where(r => r.UploadDate.Year >= 2020 && r.UploadDate.Month >= 10 && r.UploadDate.Year <= 2020 && r.UploadDate.Month <= 11)).Select( r => new 
            {
                Month = r.UploadDate.Month,
                Year = r.UploadDate.Year
            }).AsEnumerable().GroupBy(x => new { x.Year, x.Month });

            List<AnalyticDate> recipeData = new List<AnalyticDate>();

            foreach(var i in recipeList)
            {
                AnalyticDate analyticDate = new AnalyticDate() { Month = i.Key.Month, Year = i.Key.Year, Count = i.Count()};
                recipeData.Add(analyticDate);
            }

            AnalyticsViewModel analytics = new AnalyticsViewModel()
            {
                recipeCount = recipeCount,
                privateRecipeCount = privateRecipeCount,
                publicRecipeCount = publicRecipeCount,
                userCount = userCount
            };

            return View(analytics);
        }
    }
}
