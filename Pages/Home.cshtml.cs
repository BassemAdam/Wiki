using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WikiModel = Wiki.Models.Wiki;

namespace Wiki.Pages
{
    public class Home : PageModel
    {
        private readonly WikiModel _wiki;

        public Home(WikiModel wiki)
        {
            _wiki = wiki;
        }

   
       

    }
}