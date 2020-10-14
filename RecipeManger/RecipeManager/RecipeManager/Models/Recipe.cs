using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace RecipeManager.Models
{
    public class Recipe
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Time { get; set; }
        [Required]
        public string Servings { get; set; }
        [Required]
        public string Description { get; set; }

        public byte[] Photo { get; set; }
        [Required]
        public bool IsPublic { get; set; }

        public bool IsFeatured { get; set; }

        public DateTime UploadDate { get; set; }

        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }
    }
}

