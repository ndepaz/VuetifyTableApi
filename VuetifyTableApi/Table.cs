using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using VuetifyTableApi.Helpers;

namespace VuetifyTableApi
{
    public class Table

    {
        protected List<TableHeader> _headers = null;
        public List<TableHeader> headers {
            get {
                return _headers;
            } set {
                _headers = value;
            }
        }
        public List<object> rows { get; set; }
        public string[] filterableColumns { get; set; }
        public List<string> selectedHeaders { get; set; }
        /// <summary>
        /// SkipHeaders is used to inform the front end that it should hide these column headers. 
        /// In a future version, this property maybe renamed.
        /// </summary>
        public List<string> skipHeaders { get; set; }
        /// <summary>
        /// skipHeaderVirtualProps is best to inform the front end to hide all virtual properties in a model class.
        /// This is because some entity classes can have relations we may want in front end but we don't necessarly want to display them.
        /// </summary>
        public bool skipHeaderVirtualProps { get; set; }
        /// <summary>
        /// This is used when a strongly typed model class has virtual properties and you don't want to display these but request front-end to delete these.
        /// </summary>
        public bool shouldDeleteHeaderVirtualProps { get; set; }
        /// <summary>
        /// This property can be used when you want to inform front end to include the virtual properties in front end.
        /// </summary>
        public string[] includeOnlyTheseVirtualHeadersFromObject { get; set; }
        /// <summary>
        /// This should be what's needed for most cases, when you only want a specific amount of headers displayed in frontend.
        /// </summary>
        public List<string> useHeaders { get; set; }
        /// <summary>
        /// Can be used when you have unwanted column headers.
        /// </summary>
        public List<string> shouldDeleteColumns { get; set; }
        /// <summary>
        /// When set, property names matching a name, will be added to skipHeaders.
        /// </summary>
        public Regex skipHeadersRegex { get; set; }
        public Table()
        {
            this.headers = new List<TableHeader>();
            this.filterableColumns = new string[] { };
            this.selectedHeaders = new List<string>();
            this.skipHeaders = new List<string>{};
            this.skipHeaderVirtualProps = false;
            this.shouldDeleteColumns = new List<string>();
            this.includeOnlyTheseVirtualHeadersFromObject = new string[] { };
        }
        public void SkipHeadersMatchinRegex(List<string> headers)
        {
            if(skipHeadersRegex != null)
            foreach(var header in headers)
            {
                if (skipHeadersRegex.IsMatch(header))
                {
                    this.skipHeaders.Add(header);
                }
            }
        }
        /// <summary>
        /// Assumes that the collection objects (usually anonymous/dynamic types) will have matching property names that later will be translated into columns headers when displayed via front-end.
        /// </summary>
        /// <param name="useTheseHeaders"></param>
        /// <param name="headerType"></param>
        /// <param name="valueHeaderType"></param>
        public void SetHeadersOverride(List<string> useTheseHeaders, DisplayHeaderType headerType = DisplayHeaderType.UPPER, ValueHeaderType valueHeaderType = ValueHeaderType.firstCharLower)
        {
            if(this.skipHeaders.Count() > 0)
            {
                useTheseHeaders.AddRange(this.skipHeaders);
            }
            this.SkipHeadersMatchinRegex(useHeaders);
            var columnNames = useTheseHeaders;
            this.setHeaders(columnNames, headerType, valueHeaderType);
        }
        /// <summary>
        /// When using collection which contains model classes, this function can be used to control the what to display as column headers. 
        /// </summary>
        /// <typeparam name="RecordSample"></typeparam>
        /// <param name="headerType"></param>
        /// <param name="valueHeaderType"></param>
        public void SetHeaders<RecordSample>(DisplayHeaderType headerType = DisplayHeaderType.UPPER, ValueHeaderType valueHeaderType = ValueHeaderType.firstCharLower)
        {
            var objectType = typeof(RecordSample);
            var columnNames = objectType.GetRuntimeProperties()
                .Select(t => t.Name).ToList<string>();
            this.SkipHeadersMatchinRegex(columnNames);
            this.setHeaders(columnNames,headerType,valueHeaderType, objectType);
        }
        protected void setHeaders(List<string> columnNames, DisplayHeaderType headerType = DisplayHeaderType.UPPER,ValueHeaderType valueHeaderType = ValueHeaderType.firstCharLower, Type objectType = null)
        {
            foreach (string columnName in columnNames)
            {
                bool shouldDelete = shouldDeleteColumns.Any(s => s.ToLowerInvariant() == columnName.ToLower());
                bool shouldSkip = false;
                if (this.skipHeaderVirtualProps 
                    && !includeOnlyTheseVirtualHeadersFromObject.Any(header => header.ToLowerInvariant() == columnName.ToLowerInvariant())
                    && objectType != null && objectType.GetProperty(columnName) != null && objectType.GetProperty(columnName).GetGetMethod().IsVirtual)
                {
                    if (!shouldDeleteHeaderVirtualProps)
                    {
                        continue;
                    }
                    shouldDeleteColumns.Add(columnName);
                    shouldDelete = true;
                }
                
                var isExcludedHeader = skipHeaders.Count > 0 && skipHeaders.Any(header => header.ToLowerInvariant() == columnName.ToLowerInvariant());
                if (isExcludedHeader)
                {
                    shouldSkip = true;
                }
                var isNotSelectedHeaderSkip = selectedHeaders.Count > 0 && !selectedHeaders.Any(header => header.ToLowerInvariant() == columnName.ToLowerInvariant());
                if (isNotSelectedHeaderSkip)
                {
                    shouldSkip = true;
                }
                string newHeaderName = setDisplayHeaderCase(headerType, columnName);
                string newValueHeader = setValueHeaderCase(valueHeaderType, columnName);

                var header = new TableHeader()
                {
                    text = newHeaderName.ToUpperInvariant(),
                    value = newValueHeader,
                    shouldDeleteColumn = shouldDelete,
                    align = shouldSkip ? @" d-none":"start"
                };

                if (filterableColumns.Length > 0)
                {
                    var isFilterable = filterableColumns.Any(s => s.ToLowerInvariant() == newHeaderName);
                    header.filterable = isFilterable;
                } 
                
                this.headers.Add(header);
            }
        }

        protected string setValueHeaderCase(ValueHeaderType valueHeaderType, string headerName)
        {
            string newHeaderName = headerName;
            newHeaderName = valueHeaderType switch
            {
                ValueHeaderType.lower => newHeaderName.ToLowerInvariant(),
                ValueHeaderType.firstCharLower => char.ToLower(newHeaderName[0]) + newHeaderName.Substring(1),
                _ => newHeaderName,
            };
            return newHeaderName;
        }

        protected static string setDisplayHeaderCase(DisplayHeaderType headerType, string columnName)
        {
            string newHeaderName = columnName;
            newHeaderName = headerType switch
            {
                DisplayHeaderType.UPPER => newHeaderName.ToUpperInvariant(),
                DisplayHeaderType.lower => newHeaderName.ToLowerInvariant(),
                DisplayHeaderType.UPPER_SNAKE_CASE => ExtensionMethods.ToUnderscoreCase(newHeaderName).ToUpperInvariant(),
                DisplayHeaderType.lower_snake_case => ExtensionMethods.ToUnderscoreCase(newHeaderName).ToLowerInvariant(),
                _ => newHeaderName,
            };
            return newHeaderName;
        }
    }
    public enum DisplayHeaderType {
        UPPER,
        UPPER_SNAKE_CASE,
        lower,
        lower_snake_case
    }
    public enum ValueHeaderType
    {
        lower,
        original,
        firstCharLower,
    }
}