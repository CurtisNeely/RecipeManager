using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeManager.Models
{
    public class RecipeCategory
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey("Recipe")]
        public long RecipeId { get; set; }

        [ForeignKey("Category")]
        public long CategoryId { get; set; }
    }
}
