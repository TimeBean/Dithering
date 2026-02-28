namespace DitherConsole;

public static class Constants
{
    public static readonly float[,] Palette0 = new[,]
    {
        { 216f, 245f, 250f }, // #faf5d8
        { 139f, 174f, 216f }, // #d8ae8b
        { 27f, 24f, 33f }, // #21181b
        { 42f, 95f, 205f }, // #cd5f2a
        { 55f, 171f, 242f } // #f2ab37
    };
    
    public static readonly float[,] Palette1 = new[,]
    {
        { 173f, 187f, 251f }, // #fbbbad
        { 150f, 122f, 74f }, // #4a7a96
        { 88f, 63f, 51f }, // #333f58
        { 49f, 40f, 41f } // #292831
    };

    public static readonly float[,] Palette2 = new[,]
    {
        { 35f, 35f, 34f }, // #222323
        { 240f, 246f, 240f } // #f0f6f0
    };

    public static readonly float[,] Palette3 = new[,]
    {
        { 255f, 255f, 255f }, // #ffffff
        { 242f, 230f, 12f }, // #0ce6f2
        { 219f, 152f, 0f }, // #0098db
        { 156f, 87f, 30f }, // #1e579c
        { 98f, 53f, 32f }, // #203562
        { 70f, 36f, 37f }, // #252446
        { 51f, 21f, 32f } // #201533
    };
        
    public static readonly int[,] Bayer2 =
    {
        { 0, 2 },
        { 3, 1 }
    };
            
    public static readonly int[,] Bayer4 =
    {
        {  0,  8,  2, 10 },
        { 12,  4, 14,  6 },
        {  3, 11,  1,  9 },
        { 15,  7, 13,  5 }
    };

    public static readonly int[,] Bayer8 =
    {
        {  0,32, 8,40, 2,34,10,42 },
        { 48,16,56,24,50,18,58,26 },
        { 12,44, 4,36,14,46, 6,38 },
        { 60,28,52,20,62,30,54,22 },
        {  3,35,11,43, 1,33, 9,41 },
        { 51,19,59,27,49,17,57,25 },
        { 15,47, 7,39,13,45, 5,37 },
        { 63,31,55,23,61,29,53,21 }
    };
}