// SurveyApp: A polished Windows Forms survey application with modern UI elements

using System.Data;

namespace SurveyApp
{
    internal class SQLiteDataAdapter
    {
        private string query;
        private SQLiteConnection conn;

        public SQLiteDataAdapter(string query, SQLiteConnection conn)
        {
            this.query = query;
            this.conn = conn;
        }

        internal void Fill(DataTable dt)
        {
            throw new NotImplementedException();
        }
    }
}