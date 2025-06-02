// Required namespaces
using SurveyApp;

namespace Assignment_LifeStyleSurvey
{
    internal class SQLiteCommand
    {
        private string v;
        private SQLiteConnection con;

        public SQLiteCommand(string v, SQLiteConnection con)
        {
            this.v = v;
            this.con = con;
        }
    }
}