using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Insight.Database;
using System.Data.SQLite;
using HealthReporter.Models;
using System.IO;

namespace HealthReporter.Utilities
{
    public class DatabaseUtility
    {
        static String dbName = "HealthReporter.sqlite";

        static public SQLiteConnectionStringBuilder getConnectionStringBuilder()
        {
            return new SQLiteConnectionStringBuilder() { DataSource = dbName };
        }

        static public void checkDb()
        {
            if (!File.Exists(dbName))
            {
                createDatabase();
            }
        }

        static public void createDatabase()
        {
            SQLiteConnection dbConnection;
            SQLiteConnectionStringBuilder builder;
            SQLiteConnection.CreateFile(dbName);
            builder = getConnectionStringBuilder();
            dbConnection = new SQLiteConnection(builder.ConnectionString);

            dbConnection.ExecuteSql(getDbCommand());
            dbConnection.Close();
        }

        static public void dropDatabase()
        {
            if (File.Exists(dbName))
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                File.Delete(dbName);
            }
        }

        static public void resetDb()
        {
            if (File.Exists(dbName))
            {
                SQLiteConnection dbConnection = getConnection();
                dbConnection.ExecuteSql(dropDbCommand());
                dbConnection.ExecuteSql(getDbCommand());
                dbConnection.Close();
            } else
            {
                createDatabase();
            }
        }

        static public SQLiteConnection getConnection()
        {
            var connection = new SQLiteConnection(getConnectionStringBuilder().ConnectionString).OpenConnection();

            return connection;
        }

        static public string dropDbCommand()
        {
            return @"
PRAGMA foreign_keys = OFF; 
DROP TABLE 'presets';
DROP TABLE 'test_categories';
DROP TABLE 'rating_labels';
DROP TABLE 'groups';
DROP TABLE 'appraisers';
DROP TABLE 'tests';
DROP TABLE 'clients';
DROP TABLE 'preset_tests';
DROP TABLE 'ratings';
DROP TABLE 'appraisals';
DROP TABLE 'appraisal_tests';
            ";
        }

        static public string getDbCommand()
        {
            return @"
PRAGMA foreign_keys = ON; 
CREATE TABLE 'presets'(
  'id' BLOB PRIMARY KEY NOT NULL,
  'name' TEXT,
  'updated' DATETIME DEFAULT CURRENT_TIMESTAMP,
  'uploaded' DATETIME,
  CONSTRAINT 'id_UNIQUE'
    UNIQUE('id')
);
CREATE TABLE 'test_categories'(
  'id' BLOB PRIMARY KEY NOT NULL,
  'parentId' BLOB,
  'name' TEXT,
  'position' INTEGER NOT NULL,
  'updated' DATETIME DEFAULT CURRENT_TIMESTAMP,
  'uploaded' DATETIME,
  CONSTRAINT 'id_UNIQUE'
    UNIQUE('id'),
  CONSTRAINT 'test_categories_to_test_categories'
    FOREIGN KEY('parentId')
    REFERENCES 'test_categories'('id')
);
CREATE INDEX 'test_category_to_test_category_idx' ON 'test_categories' ('parentId');
CREATE TABLE 'rating_labels'(
  'id' BLOB PRIMARY KEY NOT NULL,
  'name' TEXT,
  'interpretation' TEXT,
  'updated' DATETIME DEFAULT CURRENT_TIMESTAMP,
  'uploaded' DATETIME,
  'rating' INTEGER CHECK('rating'>=0),
  CONSTRAINT 'id_UNIQUE'
    UNIQUE('id')
);
CREATE TABLE 'groups'(
  'id' BLOB PRIMARY KEY NOT NULL,
  'name' TEXT,
  'updated' DATETIME DEFAULT CURRENT_TIMESTAMP,
  'uploaded' DATETIME,
  CONSTRAINT 'id_UNIQUE'
    UNIQUE('id')
);
CREATE TABLE 'appraisers'(
  'id' BLOB PRIMARY KEY NOT NULL,
  'name' TEXT,
  'updated' DATETIME DEFAULT CURRENT_TIMESTAMP,
  'uploaded' DATETIME,
  CONSTRAINT 'id_UNIQUE'
    UNIQUE('id')
);
CREATE TABLE 'tests'(
  'id' BLOB PRIMARY KEY NOT NULL,
  'categoryId' BLOB NOT NULL,
  'name' TEXT,
  'description' TEXT,
  'units' TEXT,
  'decimals' INTEGER,
  'weight' DECIMAL,
  'formulaF' TEXT,
  'formulaM' TEXT,
  'position' INTEGER,
  'updated' DATETIME DEFAULT CURRENT_TIMESTAMP,
  'uploaded' DATETIME,
  CONSTRAINT 'id_UNIQUE'
    UNIQUE('id'),
  CONSTRAINT 'tests_to_categories'
    FOREIGN KEY('categoryId')
    REFERENCES 'test_categories'('id')
);
CREATE INDEX 'test_to_categories_idx' ON 'tests' ('categoryId');
CREATE TABLE 'clients'(
  'id' BLOB PRIMARY KEY NOT NULL,
  'firstName' TEXT,
  'lastName' TEXT,
  'groupId' BLOB NOT NULL,
  'email' TEXT,
  'gender' INTEGER,
  'birthDate' DATE,
  'updated' DATETIME DEFAULT CURRENT_TIMESTAMP,
  'uploaded' DATETIME,
  CONSTRAINT 'id_UNIQUE'
    UNIQUE('id')
  CONSTRAINT 'clients_to_groups'
    FOREIGN KEY('groupId')
    REFERENCES 'groups' ('id')
    ON DELETE CASCADE
    ON UPDATE CASCADE
);
CREATE INDEX 'clients to groups_idx' ON 'clients' ('groupId');
CREATE TABLE 'preset_tests'(
  'testId' BLOB NOT NULL,
  'presetId' BLOB NOT NULL,
  'updated' DATETIME DEFAULT CURRENT_TIMESTAMP,
  'uploaded' DATETIME,
  CONSTRAINT 'preset_tests_to_tests'
    FOREIGN KEY('testId')
    REFERENCES 'tests'('id'),
  CONSTRAINT 'preset_tests_to_presets'
    FOREIGN KEY('presetId')
    REFERENCES 'presets'('id')
);
CREATE INDEX 'preset_tests_to_presets_idx' ON 'preset_tests' ('testId');
CREATE INDEX 'preset_tests_to_presets_idx1' ON 'preset_tests' ('presetId');
CREATE TABLE 'ratings'(
  'testId' BLOB NOT NULL,
  'labelId' BLOB,
  'age' INTEGER,
  'normF' DECIMAL,
  'normM' DECIMAL,
  'updated' DATETIME DEFAULT CURRENT_TIMESTAMP,
  'uploaded' DATETIME,
  CONSTRAINT 'ratings_to_tests'
    FOREIGN KEY('testId')
    REFERENCES 'tests'('id'),
  CONSTRAINT 'ratings_to_rating_labels'
    FOREIGN KEY('labelId')
    REFERENCES 'rating_labels'('id')
);
CREATE INDEX 'ratings_to_tests_idx' ON 'ratings' ('testId');
CREATE INDEX 'ratings_to_rating_labels_idx' ON 'ratings' ('labelId');
CREATE TABLE 'appraisals'(
  'id' BLOB PRIMARY KEY NOT NULL,
  'appraiserId' BLOB NOT NULL,
  'clientId' BLOB NOT NULL,
  'date' DATE,
  'updated' DATETIME DEFAULT CURRENT_TIMESTAMP,
  'uploaded' DATETIME,
  CONSTRAINT 'id_UNIQUE'
    UNIQUE('id'),
  CONSTRAINT 'appraisals_to_appraisers'
    FOREIGN KEY('appraiserId')
    REFERENCES 'appraisers'('id'),
  CONSTRAINT 'appraisals_to_clients'
    FOREIGN KEY('clientId')
    REFERENCES 'clients'('id')
);
CREATE INDEX 'appraisals_to_appraisers_idx' ON 'appraisals' ('appraiserId');
CREATE INDEX 'appraisals_to_clients_idx' ON 'appraisals' ('clientId');
CREATE TABLE 'appraisal_tests'(
  'appraisalId' BLOB NOT NULL,
  'testId' BLOB NOT NULL,
  'score' DECIMAL NOT NULL,
  'note' TEXT,
  'trial1' DECIMAL,
  'trial2' DECIMAL,
  'trial3' DECIMAL,
  'updated' DATETIME DEFAULT CURRENT_TIMESTAMP,
  'uploaded' DATETIME,
  CONSTRAINT 'appraisal_tests_to_tests'
    FOREIGN KEY('testId')
    REFERENCES 'tests'('id'),
  CONSTRAINT 'appraisal_test_to_appraisals'
    FOREIGN KEY('appraisalId')
    REFERENCES 'appraisals'('id')
);
CREATE INDEX 'appraisal_tests_to_tests_idx' ON 'appraisal_tests' ('testId');
CREATE INDEX 'appraisal_test_to_appraisals_idx' ON 'appraisal_tests' ('appraisalId');
            ";
        }
    }
}
