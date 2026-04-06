namespace TheLegends.Base.Ads
{
    /// <summary>
    /// Defines the standard and custom element types used to map Unity UI components to the AdMob Native Ad architecture.
    /// </summary>
    public enum NativeAdElement
    {
        RootAdView = 0,

        // Standard AdMob Elements
        Headline = 1,
        CallToAction = 2,
        MediaView = 3,
        IconView = 4,
        Body = 5,
        Advertiser = 6,
        StarRating = 7,
        Price = 8,
        Store = 9,

        // Required Google Policy Label
        AdAttribution = 10,

        // Advanced Decorators (Handled Natively)
        CloseButton = 11,
        CountdownText = 12,

        // Custom Elements
        Background = 13
    }
}
