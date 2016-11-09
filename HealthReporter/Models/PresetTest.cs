using HealthReporter.Utilities;
using Insight.Database;
using System.Collections.Generic;

namespace HealthReporter.Models
{
    class PresetTestRepository
    {
        public void Insert(PresetTest test)
        {
            var connection = DatabaseUtility.getConnection();
            var res = connection.InsertSql("INSERT INTO preset_tests (testId, presetId) values(@testId, @presetId)", test);
        }

        public void Delete(Preset preset)
        {
            var res = DatabaseUtility.getConnection().InsertSql("DELETE FROM preset_tests WHERE presetId=@id", preset);
        }

        public IList<PresetTest> FindPresetTests(Preset preset)
        {
            return DatabaseUtility.getConnection().QuerySql <PresetTest>("SELECT * FROM preset_tests WHERE presetId=@id", preset);
        }
    }
        class PresetTest
    {
        public byte[] testId { get; set; }
        public byte[] presetId { get; set; }
        public string updated { get; set; }
        public string uploaded { get; set; }
    }
}
