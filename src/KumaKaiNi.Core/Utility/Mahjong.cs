namespace KumaKaiNi.Core.Utility;

public class Mahjong
{
    private const string MahjongWallRedisListName = "mahjong:wall";

    private static readonly string[] MahjongTiles =
    [
        "Bamboo1",
        "Bamboo2",
        "Bamboo3",
        "Bamboo4",
        "Bamboo5",
        "Bamboo6",
        "Bamboo7",
        "Bamboo8",
        "Bamboo9",
        "Chara1",
        "Chara2",
        "Chara3",
        "Chara4",
        "Chara5",
        "Chara6",
        "Chara7",
        "Chara8",
        "Chara9",
        "Pin1",
        "Pin2",
        "Pin3",
        "Pin4",
        "Pin5",
        "Pin6",
        "Pin7",
        "Pin8",
        "Pin9",
        "DragonGreen",
        "DragonRed",
        "DragonWhite",
        "WindEast",
        "WindWest",
        "WindNorth",
        "WindSouth"
    ];

    private static readonly string[] QingqueTiles =
    [
        "QingqueHua",
        "QingqueTong",
        "QingqueWan",
        "QingqueYu"
    ];

    public static async Task<string> GetTile()
    {
        if (Rng.OneTo(100)) return Rng.PickRandom(QingqueTiles);

        long tilesLeft = await Redis.ListLengthAsync(MahjongWallRedisListName);
        if (tilesLeft == 0) await ShuffleWall();

        return (await Redis.ListPopAsync(MahjongWallRedisListName))!;
    }

    private static async Task ShuffleWall()
    {
        List<string> wallList = [];

        for (int i = 1; i <= 4; i++)
        {
            foreach (string tile in MahjongTiles)
            {
                if (i == 4 && tile == "Bamboo5") wallList.Add("Bamboo5Dora");
                if (i == 4 && tile == "Chara5") wallList.Add("Chara5Dora");
                if (i == 4 && tile == "Pin5") wallList.Add("Pin5Dora");

                wallList.Add(tile);
            }
        }

        string[] wall = wallList.ToArray();
        Random random = new();
        random.Shuffle(wall);

        foreach (string tile in wall) await Redis.ListAddAsync(MahjongWallRedisListName, tile);
    }
}