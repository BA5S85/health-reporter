using HealthReporter.Utilities;
using Insight.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthReporter.Models
{
    public class Appraisal_tests
    {
        public byte[] appraisalId { get; set; }
        public byte[] testId { get; set; }
        public decimal score { get; set; }
        public string note { get; set; }
        public decimal trial1 { get; set; }
        public decimal trial2 { get; set; }
        public decimal trial3 { get; set; }
        public string updated { get; set; }


    }

    public class Appraisal_tests_repository
    {
        public void Update(Appraisal_tests appraisal_test)
        {
            var connection = DatabaseUtility.getConnection();

            var res = connection.InsertSql("UPDATE appraisal_tests set score=" + appraisal_test.score + " WHERE appraisalId=@appraisalId and testId=@testId", appraisal_test);

        }

        public void Insert(Appraisal_tests test)
        {
            var connection = DatabaseUtility.getConnection();
            var res3 = connection.InsertSql("INSERT INTO appraisal_tests (appraisalId, testId, score, note) values(@appraisalId, @testId, @score, @note)", test);


        }
        public IList<Appraisal_tests> FindAll()
        {
            return DatabaseUtility.getConnection().QuerySql<Appraisal_tests>("SELECT * FROM appraisal_tests");
        }

       
    }
}
