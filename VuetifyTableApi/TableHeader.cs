namespace VuetifyTableApi
{
    public class TableHeader
    {

        private bool _filterable;
        private string _align;// values "start", " d-none"
        public bool shouldDeleteColumn;
        public string text { get; set; }
        public string value { get; set; }
        public bool filterable
        {
            get
            {
                return _filterable;
            }
            set
            {
                _filterable = value;
            }
        }
        public string align
        {
            get
            {
                return _align ?? "start";
            }
            set
            {
                _align = value;
            }
        }
        public TableHeader()
        {
            this._filterable = true;
        }
        public TableHeader(bool filter)
        {
            this._filterable = filter;
        }
    }
}