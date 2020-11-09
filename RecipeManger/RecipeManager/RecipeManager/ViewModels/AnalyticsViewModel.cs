using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeManager.ViewModels
{
    public class AnalyticsViewModel
    {
        public int recipeCount { get; set; }
        public int privateRecipeCount { get; set; }
        public int publicRecipeCount { get; set; }
        public int userCount { get; set; }
    }
}
