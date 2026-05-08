namespace UAlbion.Game;

// QoL feature flag keys — stored in settings.json, default true.
// Connected to the Options menu QoL section when that is implemented.
public static class QoLFeature
{
    public const string MerchantDirectBuy    = "qol.merchant_direct_buy";    // Left-click buys immediately (original: pick-to-cursor)
    public const string MerchantCtrlSell    = "qol.merchant_ctrl_sell";
    public const string MerchantAltMultiSell = "qol.merchant_alt_multi_sell";
    // Future entries added here as new QoL features are implemented.
}
