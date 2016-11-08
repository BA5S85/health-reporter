using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Insight.Database;
using HealthReporter.Utilities;

namespace HealthReporter.Models
{
    class RatingRepository
    {
        public void Insert(Rating rating)
        {
            var connection = DatabaseUtility.getConnection();
            var res = connection.InsertSql("INSERT INTO ratings (testId, labelId, age, normF, normM, updated, uploaded) values(@testId, @labelId, @age, @normF, @normM, @updated, @uploaded)", rating);
        }

        public void Delete(Test test)
        {
            var res = DatabaseUtility.getConnection().InsertSql("DELETE from ratings where testId=@id", test);
        }
        public void DeleteRating(Rating rating)
        {
            var res = DatabaseUtility.getConnection().InsertSql("DELETE from ratings where testId=@testId AND age=@age AND normF=@normF AND normM=normM", rating);
        }

        public IList<Rating> FindAll()
        {
            return DatabaseUtility.getConnection().QuerySql<Rating>("SELECT * FROM ratings");
        }

        public IList<Rating> getTestRatings(Test test)
        {
            return DatabaseUtility.getConnection().QuerySql<Rating>("SELECT * FROM ratings WHERE testId = @id", test);
        }
        public IList<Rating> getAges(Test test) 
        {
            return DatabaseUtility.getConnection().QuerySql<Rating>("SELECT* FROM ratings WHERE ratings.testId = @id GROUP BY age", test);
        }
        public IList<Rating> getSameAgeRatings(Rating rating)
        {
            return DatabaseUtility.getConnection().QuerySql<Rating>("SELECT* FROM ratings WHERE age=@age AND testId=@testId", rating);
        }
        public void removeRatingsByAge(Test test, int age)
        {
           DatabaseUtility.getConnection().QuerySql<Rating>("DELETE FROM ratings WHERE testId=@id AND age ="+age.ToString(), test);
        }
        public void removeRatingsByTest(Test test)
        {
            DatabaseUtility.getConnection().QuerySql<Rating>("DELETE FROM ratings WHERE testId=@id", test);
        }
        public void Update(Rating old, Rating newR)
        {
            var cmd = DatabaseUtility.getConnection().CreateCommand();
            cmd.CommandText = "UPDATE ratings SET normF= @newNormF, normM=@newNormM, labelId=@newLabelId, updated = CURRENT_TIMESTAMP WHERE testId=@testId AND age=@age AND normM=@normM AND normF=@normF";
            cmd.Parameters.AddWithValue("@newNormF", newR.normF);
            cmd.Parameters.AddWithValue("@newNormM", newR.normM);
            cmd.Parameters.AddWithValue("@newLabelId", newR.labelId);
            cmd.Parameters.AddWithValue("@age", old.age);
            cmd.Parameters.AddWithValue("@normF", old.normF);
            cmd.Parameters.AddWithValue("@normM", old.normM);
            cmd.Parameters.AddWithValue("@labelId", old.labelId);
            cmd.Parameters.AddWithValue("@testId", old.testId);
            cmd.ExecuteNonQuery();
            cmd.ExecuteNonQuery();
        }
    }

    class Rating
    {
        public byte[] testId { get; set; }
        public byte[] labelId { get; set; }
        public int age { get; set; }
        public decimal normF { get; set; }
        public decimal normM { get; set; }
        public string updated { get; set; }
        public string uploaded { get; set; }
    }
}
