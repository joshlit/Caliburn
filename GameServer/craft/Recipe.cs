﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using DOL.Database;
using DOL.GS.ServerProperties;
using log4net;

namespace DOL.GS
{
    public class Recipe
    {
        protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public readonly Lock Lock = new();

        private Ingredient[] ingredients;

        public DbItemTemplate Product { get; }
        public eCraftingSkill RequiredCraftingSkill { get; }
        public int Level { get; }
        public List<Ingredient> Ingredients => new List<Ingredient>(ingredients);
        public bool IsForUniqueProduct { get; } = false;

        public Recipe(DbItemTemplate product, List<Ingredient> ingredients)
        {
            this.ingredients = ingredients.ToArray();
            Product = product;
        }

        public Recipe(DbItemTemplate product, List<Ingredient> ingredients, eCraftingSkill requiredSkill, int level, bool makeTemplated)
            : this(product, ingredients)
        {
            RequiredCraftingSkill = requiredSkill;
            Level = level;
            IsForUniqueProduct = !makeTemplated;
        }

        public long CostToCraft
        {
            get
            {
                long result = 0;
                foreach (var ingredient in ingredients)
                {
                    result += ingredient.Cost;
                }
                return result;
            }
        }

        public void SetRecommendedProductPriceInDB()
        {
            var product = Product;
            var totalPrice = CostToCraft;
            bool updatePrice = !(product.Name.EndsWith("metal bars") ||
                                 product.Name.EndsWith("leather square") ||
                                 product.Name.EndsWith("cloth square") ||
                                 product.Name.EndsWith("wooden boards"));

            if (product.PackageID.Contains("NoPriceUpdate"))
                updatePrice = false;

            if (updatePrice)
            {
                long pricetoset;
                var secondaryCraftingSkills = new List<eCraftingSkill>() { 
                    eCraftingSkill.MetalWorking, eCraftingSkill.LeatherCrafting, eCraftingSkill.ClothWorking, eCraftingSkill.WoodWorking
                };

                if (secondaryCraftingSkills.Contains(RequiredCraftingSkill))
                    pricetoset = Math.Abs((long)(totalPrice * 2 * Properties.CRAFTING_SECONDARYCRAFT_SELLBACK_PERCENT) / 100);
                else
                    pricetoset = Math.Abs(totalPrice * 2 * Properties.CRAFTING_SELLBACK_PERCENT / 100);

                if (pricetoset > 0 && product.Price != pricetoset)
                {
                    long currentPrice = product.Price;
                    product.Price = pricetoset;
                    product.AllowUpdate = true;
                    product.Dirty = true;
                    product.Id_nb = product.Id_nb.ToLower();
                    if (GameServer.Database.SaveObject(product))
                        log.Warn("Craft Price Correction: " + product.Id_nb + " rawmaterials price= " + totalPrice + " Current Price= " + currentPrice + ". Corrected price to= " + pricetoset);
                    else
                        log.Warn("Craft Price Correction Not SAVED: " + product.Id_nb + " rawmaterials price= " + totalPrice + " Current Price= " + currentPrice + ". Corrected price to= " + pricetoset);
                    GameServer.Database.UpdateInCache<DbItemTemplate>(product.Id_nb);
                    product.Dirty = false;
                    product.AllowUpdate = false;
                }
            }
        }
    }

    public class Ingredient
    {
        public int Count { get; }
        public DbItemTemplate Material { get; }

        public Ingredient(int count, DbItemTemplate ingredient)
        {
            Count = count;
            Material = ingredient;
        }

        public long Cost => Count * Material.Price;
    }

    public class RecipeDB
    {
        protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static Dictionary<ushort, Recipe> recipeCache = new Dictionary<ushort, Recipe>();

        public static Recipe FindBy(ushort recipeDatabaseID)
        {
            Recipe recipe;
            recipeCache.TryGetValue(recipeDatabaseID, out recipe);
            if (recipe != null)
            {
                //avoid repeated DB access for invalid recipes
                if (recipe.Product != null) return recipeCache[recipeDatabaseID];
                else throw new KeyNotFoundException("Recipe is marked as invalid. Check your logs for Recipe with ID " + recipeDatabaseID + ".");
            }

            try
            {
                recipe = LoadFromDB(recipeDatabaseID);
                return recipe;
            }
            catch (Exception e)
            {
                log.Error(e);
                recipe = NullRecipe;
                return recipe;
            }
            finally
            {
                if (Properties.CRAFTING_ADJUST_PRODUCT_PRICE)
                    recipe.SetRecommendedProductPriceInDB();
                recipeCache[recipeDatabaseID] = recipe;
            }

        }

        private static Recipe NullRecipe => new Recipe(null, null);

        private static Recipe LoadFromDB(ushort recipeDatabaseID)
        {

            string craftingDebug = string.Empty;
            
            var dbRecipe = GameServer.Database.FindObjectByKey<DbCraftedItem>(recipeDatabaseID.ToString());
            if (dbRecipe == null)
            {
                craftingDebug = "[CRAFTING] No DBCraftedItem with ID " + recipeDatabaseID + " exists.";
                log.Warn(craftingDebug);
                return null;
                //throw new ArgumentException(craftingDebug);
            }
                
            

            DbItemTemplate product = GameServer.Database.FindObjectByKey<DbItemTemplate>(dbRecipe.Id_nb);
            if (product == null)
            {
                craftingDebug = "[CRAFTING] ItemTemplate " + dbRecipe.Id_nb + " for Recipe with ID " + dbRecipe.CraftedItemID + " does not exist.";
                log.Warn(craftingDebug);
                return null;
                //throw new ArgumentException(craftingDebug);
            }

            var rawMaterials = DOLDB<DbCraftedXItem>.SelectObjects(DB.Column("CraftedItemId_nb").IsEqualTo(dbRecipe.Id_nb));
            if (rawMaterials.Count == 0)
            {
                craftingDebug = "[CRAFTING] Recipe with ID " + dbRecipe.CraftedItemID + " has no ingredients.";
                log.Warn(craftingDebug);
                return null;
                //throw new ArgumentException(craftingDebug);
            }

            bool isRecipeValid = true;

            var ingredients = new List<Ingredient>();
            foreach (DbCraftedXItem material in rawMaterials)
            {
                DbItemTemplate template = GameServer.Database.FindObjectByKey<DbItemTemplate>(material.IngredientId_nb);

                if (template == null)
                {
                    craftingDebug = "[CRAFTING] Cannot find raw material ItemTemplate: " + material.IngredientId_nb + ") needed for recipe: " + dbRecipe.CraftedItemID + "\n";
                    isRecipeValid = false;
                }
                ingredients.Add(new Ingredient(material.Count, template));
            }

            if (!isRecipeValid)
            {
                log.Warn(craftingDebug);
                return null;
                //throw new ArgumentException(errorText);
            }

            var recipe = new Recipe(product, ingredients, (eCraftingSkill)dbRecipe.CraftingSkillType, dbRecipe.CraftingLevel, dbRecipe.MakeTemplated);
            return recipe;
        }
    }
}
