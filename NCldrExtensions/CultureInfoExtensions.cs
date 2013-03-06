﻿namespace NCldr.Extensions
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Types;

    /// <summary>
    /// CultureInfoExtensions is a collection of extension methods for the CultureInfo class to access CLDR data as well as static methods to access CLDR data from culture names
    /// </summary>
    public static class CultureInfoExtensions
    {
        /// <summary>
        /// GetCurrencyDisplayName gets the localized display name of a currency in a given language
        /// </summary>
        /// <param name="currencyName">The currency to get the localized display name for</param>
        /// <param name="languageId">The language in which to get the localized display name</param>
        /// <returns>The localized display name of a currency in a given language</returns>
        public static string GetCurrencyDisplayName(string currencyName, string languageId)
        {
            Culture culture = Culture.GetCulture(languageId);
            if (culture == null || culture.Numbers == null || culture.Numbers.CurrencyDisplayNameSets == null)
            {
                return null;
            }

            CurrencyDisplayNameSet currencyDisplayNameSet =
                (from cdns in culture.Numbers.CurrencyDisplayNameSets
                 where string.Compare(cdns.Id, currencyName, false, CultureInfo.InvariantCulture) == 0
                 select cdns).FirstOrDefault();

            if (currencyDisplayNameSet != null)
            {
                CurrencyDisplayName currencyDisplayName = (from cdn in currencyDisplayNameSet.CurrencyDisplayNames
                                                           where cdn.Id == null
                                                           select cdn).FirstOrDefault();
                if (currencyDisplayName != null)
                {
                    return currencyDisplayName.Name;
                }
            }

            return null;
        }

        /// <summary>
        /// GetCurrency gets the CLDR currency for a CultureInfo
        /// </summary>
        /// <param name="cultureInfo">The CultureInfo to get the CLDR currency for</param>
        /// <returns>The CLDR currency for a CultureInfo</returns>
        public static string GetCurrency(this CultureInfo cultureInfo)
        {
            return GetCurrency(cultureInfo.Name);
        }

        /// <summary>
        /// GetCurrencyPeriods gets an array of CurrencyPeriods for a CultureInfo for a given datetime
        /// </summary>
        /// <param name="cultureInfo">The CultureInfo to get the CurrencyPeriods for</param>
        /// <param name="dateTime">The DateTime to get the CurrencyPeriods for</param>
        /// <returns>An array of CurrencyPeriods for a CultureInfo for a given datetime</returns>
        public static CurrencyPeriod[] GetCurrencyPeriods(this CultureInfo cultureInfo, DateTime dateTime)
        {
            return GetCurrencyPeriods(cultureInfo.Name, dateTime);
        }

        /// <summary>
        /// GetCurrency gets the current CLDR currency Id for the given culture
        /// </summary>
        /// <param name="cultureName">The name of the culture to get the currency for</param>
        /// <returns>The current CLDR currency Id for the given culture</returns>
        public static string GetCurrency(string cultureName)
        {
            CurrencyPeriod[] currencyPeriods = GetCurrencyPeriods(cultureName, DateTime.Now);
            if (currencyPeriods == null || currencyPeriods.GetLength(0) == 0)
            {
                return null;
            }

            return currencyPeriods[0].Id;
        }

        /// <summary>
        /// GetCurrencyPeriods gets an array of CurrencyPeriods for a culture for a given datetime
        /// </summary>
        /// <param name="cultureName">The culture name to get the CurrencyPeriods for</param>
        /// <param name="dateTime">The DateTime to get the CurrencyPeriods for</param>
        /// <returns>An array of CurrencyPeriods for a culture for a given datetime</returns>
        public static CurrencyPeriod[] GetCurrencyPeriods(string cultureName, DateTime dateTime)
        {
            CultureData culture = CultureData.GetCulture(cultureName);
            if (culture.Numbers != null && culture.Numbers.CurrencyPeriods != null)
            {
                return (from cp in culture.Numbers.CurrencyPeriods
                        where (cp.From == null || cp.From < dateTime)
                        && (cp.To == null || cp.To > dateTime)
                        select cp).ToArray();
            }

            return null;
        }

        /// <summary>
        /// GetCharacters gets the CLDR Characters for the CultureInfo
        /// </summary>
        /// <param name="cultureInfo">The CultureInfo to get the CLDR Characters for</param>
        /// <returns>The CLDR Characters for the CultureInfo</returns>
        public static Characters GetCharacters(this CultureInfo cultureInfo)
        {
            return GetCharacters(cultureInfo.Name);
        }

        /// <summary>
        /// GetCharacters gets the CLDR Characters for the culture
        /// </summary>
        /// <param name="cultureName">The culture name to get the CLDR Characters for</param>
        /// <returns>The CLDR Characters for the culture</returns>
        public static Characters GetCharacters(string cultureName)
        {
            Culture culture = Culture.GetCulture(cultureName);
            if (culture == null)
            {
                return null;
            }

            return culture.Characters;
        }

        /// <summary>
        /// GetDayPeriodRules gets the CLDR DayPeriodRules for the CultureInfo
        /// </summary>
        /// <param name="cultureInfo">The CultureInfo to get the CLDR DayPeriodRules for</param>
        /// <returns>The CLDR DayPeriodRules for the CultureInfo</returns>
        public static DayPeriodRule[] GetDayPeriodRules(this CultureInfo cultureInfo)
        {
            return GetDayPeriodRules(GetNeutralCultureInfo(cultureInfo).Name);
        }

        /// <summary>
        /// GetDayPeriodRules gets the CLDR DayPeriodRules for the culture
        /// </summary>
        /// <param name="cultureName">The culture name to get the CLDR DayPeriodRules for</param>
        /// <returns>The CLDR DayPeriodRules for the culture</returns>
        public static DayPeriodRule[] GetDayPeriodRules(string cultureName)
        {
            string language = GetLanguage(cultureName);

            foreach (DayPeriodRuleSet dayPeriodRuleSet in NCldr.DayPeriodRuleSets)
            {
                string[] cultures = dayPeriodRuleSet.CultureNames;
                if (cultures.Contains(language))
                {
                    return dayPeriodRuleSet.DayPeriodRules;
                }
            }

            // default to the "root"
            return (from dprs in NCldr.DayPeriodRuleSets
                    where dprs.CultureNames.GetLength(0) == 1 && string.Compare(dprs.CultureNames[0], "root", false, CultureInfo.InvariantCulture) == 0
                    select dprs.DayPeriodRules).FirstOrDefault();
        }

        /// <summary>
        /// GetGenderListId gets the CLDR GenderList identifier for the CultureInfo
        /// </summary>
        /// <param name="cultureInfo">The CultureInfo to get the CLDR DayPeriodRules for</param>
        /// <returns>The CLDR GenderList identifier for the CultureInfo</returns>
        public static string GetGenderListId(this CultureInfo cultureInfo)
        {
            return GetGenderListId(GetNeutralCultureInfo(cultureInfo).Name);
        }

        /// <summary>
        /// GetGenderListId gets the CLDR GenderList identifier for the culture
        /// </summary>
        /// <param name="cultureName">The culture name to get the CLDR GenderList identifier for</param>
        /// <returns>The CLDR GenderList identifier for the culture</returns>
        public static string GetGenderListId(string cultureName)
        {
            if (NCldr.GenderLists == null)
            {
                return null;
            }

            string language = GetLanguage(cultureName);

            return (from gl in NCldr.GenderLists
                    where gl.CultureIds.Contains(language)
                    select gl.Id).FirstOrDefault();
        }

        /// <summary>
        /// GetLikelySubTag gets the most likely child culture name from a parent CultureInfo
        /// </summary>
        /// <param name="cultureInfo">The CultureInfo to get the sub tag for</param>
        /// <returns>The most likely child culture name for the parent CultureInfo</returns>
        public static string GetLikelySubTag(this CultureInfo cultureInfo)
        {
            return GetLikelySubTag(cultureInfo.Name);
        }

        /// <summary>
        /// GetLikelySubTag gets the most likely child culture name from a parent culture name
        /// </summary>
        /// <param name="cultureName">The culture name to get the sub tag for</param>
        /// <returns>The most likely child culture name for the parent culture name</returns>
        public static string GetLikelySubTag(string cultureName)
        {
            if (NCldr.LikelySubTags == null)
            {
                return null;
            }

            return (from lst in NCldr.LikelySubTags
                    where string.Compare(lst.FromCultureId, cultureName, true, CultureInfo.InvariantCulture) == 0
                    select lst.ToCultureId).FirstOrDefault();
        }

        /// <summary>
        /// GetPluralRules gets the CLDR PluralRules for the CultureInfo
        /// </summary>
        /// <param name="cultureInfo">The CultureInfo to get the CLDR PluralRules for</param>
        /// <returns>The CLDR PluralRules for the CultureInfo</returns>
        public static PluralRule[] GetPluralRules(this CultureInfo cultureInfo)
        {
            return GetPluralRules(GetNeutralCultureInfo(cultureInfo).Name);
        }

        /// <summary>
        /// GetPluralRule gets the CLDR PluralRule for the integer for the CultureInfo
        /// </summary>
        /// <param name="cultureInfo">The CultureInfo to get the CLDR PluralRule for</param>
        /// <param name="value">The integer to get the PluralRule for</param>
        /// <returns>The CLDR PluralRule for the integer for the CultureInfo</returns>
        public static PluralRule GetPluralRule(this CultureInfo cultureInfo, int value)
        {
            return GetPluralRule(GetNeutralCultureInfo(cultureInfo).Name, value);
        }

        /// <summary>
        /// GetOrdinalRules gets the CLDR ordinal PluralRules for the CultureInfo
        /// </summary>
        /// <param name="cultureInfo">The CultureInfo to get the CLDR ordinal PluralRules for</param>
        /// <returns>The CLDR ordinal PluralRules for the CultureInfo</returns>
        public static PluralRule[] GetOrdinalRules(this CultureInfo cultureInfo)
        {
            return GetOrdinalRules(GetNeutralCultureInfo(cultureInfo).Name);
        }

        /// <summary>
        /// GetOrdinalRule gets the CLDR ordinal PluralRule for the integer for the CultureInfo
        /// </summary>
        /// <param name="cultureInfo">The CultureInfo to get the CLDR ordinal PluralRule for</param>
        /// <param name="value">The integer to get the ordinal PluralRule for</param>
        /// <returns>The CLDR ordinal PluralRule for the integer for the CultureInfo</returns>
        public static PluralRule GetOrdinalRule(this CultureInfo cultureInfo, int value)
        {
            return GetOrdinalRule(GetNeutralCultureInfo(cultureInfo).Name, value);
        }

        /// <summary>
        /// GetPluralRules gets the CLDR PluralRules for the culture
        /// </summary>
        /// <param name="cultureName">The culture name to get the CLDR PluralRules for</param>
        /// <returns>The CLDR PluralRules for the culture</returns>
        public static PluralRule[] GetPluralRules(string cultureName)
        {
            return GetPluralRules(NCldr.PluralRuleSets, cultureName);
        }

        /// <summary>
        /// GetOrdinalRules gets the CLDR ordinal PluralRules for the culture
        /// </summary>
        /// <param name="cultureName">The culture name to get the CLDR ordinal PluralRules for</param>
        /// <returns>The CLDR ordinal PluralRules for the culture</returns>
        public static PluralRule[] GetOrdinalRules(string cultureName)
        {
            return GetPluralRules(NCldr.OrdinalRuleSets, cultureName);
        }

        /// <summary>
        /// GetPluralRules gets the PluralRules that match the language of a culture from an array of sets of PluralRules
        /// </summary>
        /// <param name="pluralRuleSets">An array of sets of PluralRules</param>
        /// <param name="cultureName">The culture name to get the PluralRules for</param>
        /// <returns>An array of PluralRules that match the language of a culture from an array of sets of PluralRules</returns>
        private static PluralRule[] GetPluralRules(PluralRuleSet[] pluralRuleSets, string cultureName)
        {
            string language = GetLanguage(cultureName);
            foreach (PluralRuleSet pluralRuleSet in pluralRuleSets)
            {
                if (pluralRuleSet.CultureNames.Contains(language))
                {
                    return pluralRuleSet.PluralRules;
                }
            }

            return null;
        }

        /// <summary>
        /// GetPluralRule gets the CLDR PluralRule for the integer for the culture
        /// </summary>
        /// <param name="cultureName">The culture name to get the CLDR PluralRule for</param>
        /// <param name="value">The integer to get the PluralRule for</param>
        /// <returns>The CLDR PluralRule for the integer for the culture</returns>
        public static PluralRule GetPluralRule(string cultureName, int value)
        {
            return GetPluralRule(GetPluralRules(cultureName), value);
        }

        /// <summary>
        /// GetOrdinalRule gets the CLDR ordinal PluralRule for the integer for the culture
        /// </summary>
        /// <param name="cultureName">The culture name to get the CLDR ordinal PluralRule for</param>
        /// <param name="value">The integer to get the ordinal PluralRule for</param>
        /// <returns>The CLDR ordinal PluralRule for the integer for the culture</returns>
        public static PluralRule GetOrdinalRule(string cultureName, int value)
        {
            return GetPluralRule(GetOrdinalRules(cultureName), value);
        }

        /// <summary>
        /// GetPluralRule gets the CLDR PluralRule for the integer from the array of PluralRules
        /// </summary>
        /// <param name="pluralRules">The array of PluralRules to get a match from</param>
        /// <param name="value">The integer to find a match for</param>
        /// <returns>The CLDR PluralRule for the integer from the array of PluralRules</returns>
        private static PluralRule GetPluralRule(PluralRule[] pluralRules, int value)
        {
            foreach (PluralRule pluralRule in pluralRules)
            {
                if (pluralRule.IsMatch(value))
                {
                    return pluralRule;
                }
            }

            return null;
        }

        /// <summary>
        /// GetPostcodeRegex gets the postal code regular expression for the CultureInfo
        /// </summary>
        /// <param name="cultureInfo">The CultureInfo to get the postal code regular expression for</param>
        /// <returns>The postal code regular expression for the CultureInfo</returns>
        public static string GetPostcodeRegex(this CultureInfo cultureInfo)
        {
            AssertHasRegionInfo(cultureInfo);
            RegionInfo regionInfo = new RegionInfo(cultureInfo.Name);
            return RegionInfoExtensions.GetPostcodeRegex(regionInfo.TwoLetterISORegionName);
        }

        /// <summary>
        /// GetRegionInformation gets the RegionInformation for the CultureInfo
        /// </summary>
        /// <param name="cultureInfo">The CultureInfo to get the RegionInformation for</param>
        /// <returns>The RegionInformation for the CultureInfo</returns>
        public static RegionInformation GetRegionInformation(this CultureInfo cultureInfo)
        {
            AssertHasRegionInfo(cultureInfo);
            RegionInfo regionInfo = new RegionInfo(cultureInfo.Name);
            return RegionInfoExtensions.GetRegionInformation(regionInfo.TwoLetterISORegionName);
        }

        /// <summary>
        /// GetYes gets the localized 'Yes' string for the CultureInfo
        /// </summary>
        /// <param name="cultureInfo">The CultureInfo to get the localized 'Yes' string for</param>
        /// <returns>The localized 'Yes' string for the CultureInfo</returns>
        public static string GetYes(this CultureInfo cultureInfo)
        {
            return GetYes(cultureInfo.Name);
        }

        /// <summary>
        /// GetYes gets the localized 'Yes' string for the culture
        /// </summary>
        /// <param name="cultureName">The culture name to get the localized 'Yes' string for</param>
        /// <returns>The localized 'Yes' string for the culture</returns>
        public static string GetYes(string cultureName)
        {
            Culture culture = Culture.GetCulture(cultureName);
            if (culture.Messages == null)
            {
                return null;
            }

            return culture.Messages.Yes;
        }


        /// <summary>
        /// GetYes gets the localized short 'Yes' string for the CultureInfo
        /// </summary>
        /// <param name="cultureInfo">The CultureInfo to get the localized short 'Yes' string for</param>
        /// <returns>The localized short 'Yes' string for the CultureInfo</returns>
        public static string GetYesShort(this CultureInfo cultureInfo)
        {
            return GetYesShort(cultureInfo.Name);
        }

        /// <summary>
        /// GetYes gets the localized short 'Yes' string for the culture
        /// </summary>
        /// <param name="cultureName">The culture name to get the localized short 'Yes' string for</param>
        /// <returns>The localized short 'Yes' string for the culture</returns>
        public static string GetYesShort(string cultureName)
        {
            Culture culture = Culture.GetCulture(cultureName);
            if (culture.Messages == null)
            {
                return null;
            }

            return culture.Messages.YesShort;
        }

        /// <summary>
        /// GetNo gets the localized 'No' string for the CultureInfo
        /// </summary>
        /// <param name="cultureInfo">The CultureInfo to get the localized 'No' string for</param>
        /// <returns>The localized 'No' string for the CultureInfo</returns>
        public static string GetNo(this CultureInfo cultureInfo)
        {
            return GetNo(cultureInfo.Name);
        }

        /// <summary>
        /// GetNo gets the localized 'No' string for the culture
        /// </summary>
        /// <param name="cultureName">The culture name to get the localized 'No' string for</param>
        /// <returns>The localized 'No' string for the culture</returns>
        public static string GetNo(string cultureName)
        {
            Culture culture = Culture.GetCulture(cultureName);
            if (culture.Messages == null)
            {
                return null;
            }

            return culture.Messages.No;
        }


        /// <summary>
        /// GetNo gets the localized short 'No' string for the CultureInfo
        /// </summary>
        /// <param name="cultureInfo">The CultureInfo to get the localized short 'No' string for</param>
        /// <returns>The localized short 'No' string for the CultureInfo</returns>
        public static string GetNoShort(this CultureInfo cultureInfo)
        {
            return GetNoShort(cultureInfo.Name);
        }

        /// <summary>
        /// GetNo gets the localized short 'No' string for the culture
        /// </summary>
        /// <param name="cultureName">The culture name to get the localized short 'No' string for</param>
        /// <returns>The localized short 'No' string for the culture</returns>
        public static string GetNoShort(string cultureName)
        {
            Culture culture = Culture.GetCulture(cultureName);
            if (culture.Messages == null)
            {
                return null;
            }

            return culture.Messages.NoShort;
        }

        /// <summary>
        /// GetLanguage gets the language from a culture name
        /// </summary>
        /// <param name="cultureName">The culture name to get the language for</param>
        /// <returns>The language of the culture</returns>
        private static string GetLanguage(string cultureName)
        {
            if (cultureName.IndexOf("-") > -1)
            {
                return cultureName.Split('-')[0];
            }

            return cultureName;
        }

        /// <summary>
        /// GetNeutralCultureInfo gets the neutral culture that the CultureInfo falls back to
        /// </summary>
        /// <param name="cultureInfo">The CultureInfo to get the neutral culture for</param>
        /// <returns>The neutral culture that the CultureInfo falls back to</returns>
        private static CultureInfo GetNeutralCultureInfo(CultureInfo cultureInfo)
        {
            if (cultureInfo.IsNeutralCulture || cultureInfo == CultureInfo.InvariantCulture || cultureInfo.Parent == null)
            {
                return cultureInfo;
            }

            return GetNeutralCultureInfo(cultureInfo.Parent);
        }

        /// <summary>
        /// AssertHasRegionInfo throws an exception if the CultureInfo does not have a region
        /// </summary>
        /// <param name="cultureInfo">The CultureInfo to check</param>
        private static void AssertHasRegionInfo(CultureInfo cultureInfo)
        {
            if (cultureInfo.IsNeutralCulture)
            {
                throw new ArgumentException("CultureInfo must have a region to get a RegionInformation");
            }
        }
    }
}
