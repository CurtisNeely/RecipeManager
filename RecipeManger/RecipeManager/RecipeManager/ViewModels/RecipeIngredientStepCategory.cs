using RecipeManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace RecipeManager.ViewModels
{
    public class RecipeIngredientStepCategory
    {
        public Recipe recipe { get; set; }
        public List<Ingredient> ingredients { get; set; }
        public List<Step> steps { get; set; }
        public List<Category> categories { get; set; }
    }
}
