public class Player
{
    // Singleton
    private static Player instance; 

    // $$$
    private int coins;

    // Inventory
    private int nGrapes;
    private int nSeeds;
    private int nSoil;
    private int nWine;
    private int nBarrels;
    private int nTrellis;

    // Constructor
    public Player(int startingCoins)
    {
        instance = this;
        AddCoins(startingCoins);
        AddGrapes(20); // TODO: replace with 0
        AddWine(0);
        AddBarrels(1);
        nSeeds = 1;
        nSoil = 1;
        nTrellis = 2;
    }

    // Getters/Setters
    public static bool AddCoins(int delta)
    {
        if (instance.coins + delta >= 0)
        {
            instance.coins += delta;
            UIManager.SetCoinDisplay(instance.coins);
            return true;
        }
        return false;
    }

    public static bool AddGrapes(int delta)
    {
        if (instance.nGrapes + delta >= 0)
        {
            instance.nGrapes += delta;
            UIManager.SetGrapesDisplay(instance.nGrapes);
            return true;
        }
        return false;
    }

    const int MAX_WINE = 5;
    public static bool AddWine(int delta)
    {
        if (instance.nWine + delta >= 0 && instance.nWine + delta <= MAX_WINE)
        {
            instance.nWine += delta;
            UIManager.SetWineDisplay(instance.nWine, MAX_WINE);
            return true;
        }
        return false;
    }

    const int MAX_BARRELS = 5;
    public static bool AddBarrels(int delta)
    {
        if (instance.nBarrels + delta >= 0 && instance.nBarrels + delta <= MAX_BARRELS)
        {
            instance.nBarrels += delta;
            UIManager.SetBarrelDisplay(instance.nBarrels, MAX_BARRELS);
            return true;
        }
        return false;
    }
    public static int GetBarrels()
    {
        return instance.nBarrels;
    }

    public static bool AddSoil(int delta)
    {
        if (instance.nSoil + delta >= 0)
        {
            instance.nSoil += delta;
            return true;
        }
        return false;
    }

    public static bool HasSoil()
    {
        return instance.nSoil > 0;
    }

    public static bool AddSeeds(int delta)
    {
        if (instance.nSeeds + delta >= 0)
        {
            instance.nSeeds += delta;
            return true;
        }
        return false;
    }

    public static bool HasSeeds()
    {
        return instance.nSeeds > 0;
    }

    public static int GetTrellisCount()
    {
        return instance.nTrellis;
    }

    const int MAX_TRELLIS = 4;
    public static bool AddTrellis(int delta)
    {
        if (instance.nTrellis + delta >= 0 && instance.nTrellis + delta <= MAX_TRELLIS)
        {
            instance.nTrellis += delta;
            return true;
        }
        return false;
    }
}
