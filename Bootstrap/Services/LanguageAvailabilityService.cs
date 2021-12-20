using System.Collections.Generic;
using GuidedWork;

namespace Bootstrap.Services
{
    public class LanguageAvailabilityService : BaseLanguageAvailabilityService
    {
        /// <summary>
        /// A set of language name strings representing the languages that will be included in AvailableLanguages.
        /// </summary>
        /// <value>Set of language name strings.</value>
        /// TODO: This set can be modified to contain the appropriate languages for your application
        protected override HashSet<string> LanguageNames => new HashSet<string> { "en-US", "es-MX", "fr-CA" };

        /// <summary>
        /// A dictionary used to map non-specific language codes (such as "en" or "fr") to region-specific language codes (such as "en-US" or "fr-CA").
        /// Region-specific language codes are required for Pick Up and Go.
        /// If a non-specific language code does not have a mapping in this dictionary, Pick Up and Go will be unavailable for that language.
        /// </summary>
        /// <value>A dictionary containing non-specific to specific language code mappings.</value>
        /// TODO: Map any non-specific locales to specific locales here
        protected override Dictionary<string, string> PickUpAndGoSpecificLocales => new Dictionary<string, string>
        {
            // Example:
            // { "en", "en-US" }
        };
    }
}
