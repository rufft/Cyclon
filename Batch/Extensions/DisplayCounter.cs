namespace Batch.Extensions;

public static class DisplayCounter
{
    public static string ConvertNumToDisplayCoordinates(int num)
    {
        if (num < 10)
            return num.ToString();

        var letter = (char)('A' + (num - 10));
        return letter.ToString();
    }
}