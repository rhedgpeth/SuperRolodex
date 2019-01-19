using System;
using System.Collections.Generic;
using System.Linq;
using Couchbase.Lite;
using Couchbase.Lite.Query;
using SuperRolodex.Core.Models;

namespace SuperRolodex.Core.Repositories
{
    public class HeroesRepository
    {
        static readonly Lazy<HeroesRepository> lazy = new Lazy<HeroesRepository>(() => new HeroesRepository());
        public static HeroesRepository Instance { get { return lazy.Value; } }

        Database _heroesDatabase;
        Database HeroesDatabase
        {
            get
            {
                if (_heroesDatabase == null)
                {
                    _heroesDatabase = new Database("Heroes");
                }

                return _heroesDatabase;
            }
        }

        HeroesRepository()
        { }

        public void Initalize()
        {
            // Only load the Couchase database once
            if (!Database.Exists("Heroes", null))
            {
                LoadHeroes();

                // To run a full-text search (FTS) query, you must have created a full-text index 
                // on the expression being matched. Unlike regular queries, the index is NOT optional. 
                CreateAliasIndex();
            }
        }

        void CreateAliasIndex()
        {
            var index = IndexBuilder.FullTextIndex(FullTextIndexItem.Property("Alias")).IgnoreAccents(false);
            HeroesDatabase.CreateIndex("AliasIndex", index);
        }

        void LoadHeroes()
        {
            var heroes = GetHeroes();

            if (heroes?.Count > 0)
            {
                AddHeroes(heroes);
            }
        }

        public void AddHeroes(List<Hero> heroes)
        {
            foreach (var hero in heroes)
            {
                AddHero(hero);
            }
        }

        public string AddHero(Hero hero)
        {
            // Create a new document (i.e. a record) in the database
            string id = null;

            using (var mutableDoc = new MutableDocument())
            {
                mutableDoc
                    .SetString("HeroId", hero.HeroId)
                    .SetString("ImageUrl", hero.ImageUrl)
                    .SetString("Alias", hero.Alias)
                    .SetString("Name", hero.Name)
                    .SetInt("Age", hero.Age)
                    .SetString("Location", hero.Location);

                // Save it to the database
                HeroesDatabase.Save(mutableDoc);

                id = mutableDoc.Id;
            }

            return id;
        }

        public List<Hero> GetAll(bool sortAscending)
        {
            var heroes = new List<Hero>();

            // Get all of the hero records, and sort accordingly
            using (var query = QueryBuilder.Select(SelectResult.All())
                                           .From(DataSource.Database(HeroesDatabase))
                                           .OrderBy(sortAscending ? Ordering.Property("Alias").Ascending() 
                                                                    : Ordering.Property("Alias").Descending()))
            {
                heroes = ParseResults(query)?.ToList();
            }

            return heroes;
        }

        public List<Hero> Search(string searchText, bool sortAscending)
        {
            var heroes = new List<Hero>();

            if (!string.IsNullOrEmpty(searchText))
            {
                // Create a FTS expression for the previously created "AliasIndex" index.
                var whereClause = FullTextExpression.Index("AliasIndex").Match($"'{searchText}'");

                using (var query = QueryBuilder.Select(SelectResult.All())
                                               .From(DataSource.Database(HeroesDatabase))
                                               .Where(whereClause)
                                               .OrderBy(sortAscending ? Ordering.Property("Alias").Ascending()
                                                                    : Ordering.Property("Alias").Descending()))
                {
                    heroes = ParseResults(query)?.ToList();
                }
            }
            else
            {
                heroes = GetAll(sortAscending);
            }

            return heroes;
        }

        IEnumerable<Hero> ParseResults(IQuery query)
        {
            // Run the query
            var results = query.Execute();

            // Loop through the results, and parse the document contents to create a Hero object.
            foreach (var result in results)
            {
                var dict = result.GetDictionary("Heroes");

                if (dict != null)
                {
                    var hero = new Hero
                    {
                        HeroId = dict.GetString("HeroId"),
                        Alias = dict.GetString("Alias"),
                        Name = dict.GetString("Name"),
                        Location = dict.GetString("Location"),
                        Age = dict.GetInt("Age"),
                        ImageUrl = dict.GetString("ImageUrl")
                    };

                    yield return hero;
                }
            }
        }

        // Sample data
        List<Hero> GetHeroes()
        {
            return new List<Hero>
            {
                new Hero(NewId(), "Ant-Man", "Hank Pym", "New York City", 32, "http://4.bp.blogspot.com/-JqtTn6YMkcs/Vb40dbY6XXI/AAAAAAAABNw/hbcavSXxJuw/s1600/ant-man%2B%25281%2529.jpg"),
                new Hero(NewId(), "Aquaman", "Arthur Curry", "Atlantis", 27, "https://static.tvtropes.org/pmwiki/pub/images/aquaman.jpg"),
                new Hero(NewId(), "Batman", "Bruce Wayne", "Gotham", 41, "https://static1.squarespace.com/static/5106cf89e4b04827cc5fc5bb/t/5745aaaec2ea51621122d51d/1464183476327/"),
                new Hero(NewId(), "Booster Gold", "Michael Jon Carter", "The Future", 25, "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTQwz7GweiktkKTN4DCc3OwFQMRK0ACEjTDSdMJp7ljtTwz39vtPg"),
                new Hero(NewId(), "Flash", "Barry Allen", "Central City", 31, "https://i.pinimg.com/originals/48/04/9a/48049ae323958ddc5ebaee14d598801d.jpg"),
                new Hero(NewId(), "Green Lantern", "Hal Jordan", "Coast City", 33, "https://i.pinimg.com/originals/18/93/c1/1893c1eb8de99a4f861297d1ff08e1b5.jpg"),
                new Hero(NewId(), "Iron Man", "Tony Stark", "New York City", 37, "https://imgc.allpostersimages.com/img/print/posters/iron-man-marvel-comics-lifesize-standup_a-G-13877143-0.jpg"),
                new Hero(NewId(), "Captain America", "Steve Rogers", "New York City", 33, "https://imgc.allpostersimages.com/img/print/posters/captain-america-marvel-comics-lifesize-standup_a-G-13877144-0.jpg"),
                new Hero(NewId(), "Superman", "Clark Kent", "Metropolis", 30, "https://b.kisscc0.com/20180718/yee/kisscc0-superman-jerry-siegel-batman-comic-book-comics-superman-5b4ecaa323c221.5011794715318903391465.jpg"),
                new Hero(NewId(), "Wonder Woman", "Diana Prince", "Themyscira ", 1270, "https://www.writeups.org/wp-content/uploads/Wonder-Woman-DC-Comics-Gail-Simone-Diana-Themyscira-b.jpg"),
                new Hero(NewId(), "Black Widow", "Natasha Romanova", "Russia", 27, "https://vignette.wikia.nocookie.net/marveldatabase/images/1/1d/Natalia_Romanova_%28Earth-12131%29_from_Marvel_Avengers_Alliance_007.png/revision/latest?cb=20140705221921"),
                new Hero(NewId(), "Scarlet Witch", "Wanda Maximoff", "Transia", 23, "https://www.writeups.org/wp-content/uploads/Scarlet-Witch-Marvel-Comics-Avengers-Early-j.jpg"),
                new Hero(NewId(), "Doctor Strange", "Stephen Strange", "New York City", 42, "https://i.pinimg.com/originals/aa/95/7c/aa957cfd48f3b13aa26da5f7cadd6046.jpg"),
                new Hero(NewId(), "Hulk", "Bruce Banner", "New York City", 35, "https://upload.wikimedia.org/wikipedia/en/5/59/Hulk_%28comics_character%29.png"),
                new Hero(NewId(), "Spider-man", "Peter Parker", "New York City", 17, "https://www.writeups.org/wp-content/uploads/Spider-Man-Marvel-Comics-Peter-Parker-Profile.jpg")
                /*
                new Hero(NewId(), "", "", "", 0, ""),
                new Hero(NewId(), "", "", "", 0, ""),
                new Hero(NewId(), "", "", "", 0, ""),
                new Hero(NewId(), "", "", "", 0, ""),
                new Hero(NewId(), "", "", "", 0, ""),
                new Hero(NewId(), "", "", "", 0, ""),
                new Hero(NewId(), "", "", "", 0, ""),
                new Hero(NewId(), "", "", "", 0, ""),
                new Hero(NewId(), "", "", "", 0, ""),
                new Hero(NewId(), "", "", "", 0, "")
                */               
            };
        }

        string NewId() => Guid.NewGuid().ToString();
    }
}
