using BoldareBrewery.Shared.Enums;

namespace BoldareBrewery.Application.Models.Internal
{
    public class SearchContext
    {
        public DataSource DataSource { get; set; }
        public bool IsDataFresh { get; set; }
        public DateTime? LastSync { get; set; }    
    }
}