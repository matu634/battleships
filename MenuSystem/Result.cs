namespace MenuSystem
{
    /**
     * class Game methods can return this enum.
     * It is up to a user interface class to decide what to do after these result have been returned
     */
    public enum Result
    {
        None,
        NoPreviousMenuFound,
        ReturnToPreviousMenu,
        GameOver,
        ComputerWon,
        NoSuchTile,        // Returned when tile could not be parsed
        TileAlreadyBombed, // Returned when tile has been already bombed when it shouldn't have been
        InvalidSize,
        InvalidQuantity,
        InvalidInput,
        RulesChanged,
        ShipNotDeleted,
        ShipDeleted,
        PlayerNameChanged,
        TileNotHighlighted,
        PlayerNameNotChanged,
        SuccessfulReplayBombing,
        CouldntPlaceAllShips,
        TwoBombings,        // Returned when a player and computer have made bombings
        OneBombing,          // Returned when a player has made a bombing
        Overlap,
        NoCommonAxis,
        HighlightMissing,
        Continue,
        Break,
        TooLong,
        TooShort,
        GameAlreadyOver,
        TooManyShipTiles
    }
}