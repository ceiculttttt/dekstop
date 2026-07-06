namespace AnsemBullOverlay.Services;

/// <summary>Random sayings shown in the chat bubble when the bull is clicked.</summary>
public static class BullSayings
{
    private static readonly Random _rand = new();

    public static readonly string[] Lines =
    {
        "You can touch me 😉",
        "GM, degen.",
        "Diamond hooves only.",
        "Ansem sent me.",
        "Charge it. 🚀",
        "Green candles incoming.",
        "Ser, please...",
        "WAGMI. 🐂",
        "$ANSEM never sleeps.",
        "Do you even hodl?",
        "Sniff... I smell alpha.",
        "One more click and I moon.",
        "Bull run acquired.",
        "Sell? I don't know her.",
        "Have you touched grass today? Touch me instead.",
        "Solana speed. Bull power.",
        "The herd is watching.",
        "Not financial advice. Just vibes.",
        "Buy the dip. Ride the bull.",
        "Zoom out. Trust the bull."
    };

    public static string Random() => Lines[_rand.Next(Lines.Length)];
}
