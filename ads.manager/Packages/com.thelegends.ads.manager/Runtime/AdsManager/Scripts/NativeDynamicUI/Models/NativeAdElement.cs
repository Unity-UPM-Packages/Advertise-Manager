namespace TheLegends.Base.Ads.NativeDynamicUI
{
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
        Decorator_CloseButton = 11,
        Decorator_CountdownText = 12,
        Decorator_RadialTimer = 13
    }
}
