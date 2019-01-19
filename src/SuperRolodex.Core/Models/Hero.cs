using System;

namespace SuperRolodex.Core.Models
{
    public class Hero
    {
        public string HeroId { get; set; }
        public string ImageUrl { get; set; }
        public string Alias { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Location { get; set; }

        public Hero()
        {  }

        public Hero(string heroId, string alias, string name, string location, int age, string imageUrl)
        {
            HeroId = heroId;
            Alias = alias;
            Name = name;
            Location = location;
            Age = age;
            ImageUrl = imageUrl;
        }
    }
}
