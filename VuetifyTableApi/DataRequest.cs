namespace VuetifyTableApi
{
    /// <summary>
    /// This class can be used to bind it to a incoming url query
    /// </summary>
    public class DataRequest
    {
        /// <summary>
        /// Property can be used to return the term search in front end.
        /// </summary>
        public string search { get; set; }
        /// <summary>
        /// Property can be used to set/return the amount of records needed via backend/frontend.
        /// </summary>
        public int maxRecords { get; set; }
    }
}
