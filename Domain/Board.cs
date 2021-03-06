using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Domain
{
    public class Board : ICloneable
    {
        public int BoardId { get; set; }
        
        [NotMapped]
        public List<List<Tile>> Tiles { get; set; }
        
        public List<Battleship> Battleships { get; set; } = new List<Battleship>();
//        public List<Tile> Bombings { get; set; } = new List<Tile>();
        

        public int CanTouch { get; set; }
        public int MaxCol { get; set; }
        public int MaxRow { get; set; }
        
        [NotMapped]
        public Tile HighlightedStart, HighLightedEnd;

        public Board(int totalRows, int totalCols, int canTouch)
        {
            MaxRow = totalRows - 1;
            MaxCol = totalCols - 1;
            CanTouch = canTouch;
            Tiles = new List<List<Tile>>(totalRows);
            for (var row = 0; row < totalRows; row++)
            {
                Tiles.Add(new List<Tile>(totalCols));
                for (var col = 0; col < totalCols; col++)
                {
                    Tiles[row].Add(new Tile(row, col, this));
                }
            }
        }

        private Board()
        {
        }

        public void AddBattleship((int row, int col) start, (int row, int col) end, Battleship battleship)
        {
            //Check that ship end points are on the same axis
            if (!(start.row == end.row || start.col == end.col))
            {
                throw new ArgumentException("Start and end pieces are not are not on the same axis!");
            }

            //Check that points are inside the board
            if (start.row < 0 || end.row < 0 || start.col < 0 || end.col < 0 ||
                start.row > MaxRow || end.row > MaxRow || start.col > MaxCol || end.col > MaxCol)
            {
                throw new ArgumentException("One of the points is not inside the board!");
            }

            //Check that ship size is correct

            if (start.row == end.row && Math.Abs(start.col - end.col) + 1 != battleship.Size ||
                start.col == end.col && Math.Abs(start.row - end.row) + 1 != battleship.Size)
            {
                Console.WriteLine(Math.Abs(start.col - end.col) + 1);
                Console.WriteLine(battleship.Size);
                Console.WriteLine($"({start.row}:{start.col})({end.row}:{end.col})");
                throw new ArgumentException("Ship is longer than distance between points!");
            }

            //Check, that ship is not on other ships

            foreach (var coord in GetAllCoords((start.row, start.col), (end.row, end.col)))
            {
                if (!Tiles[coord.row][coord.col].IsEmpty())
                {
                    throw new ArgumentException($"({coord.row} : {coord.col}) already contains a ship!");
                }
            }

            //Check that ship is not too close to other ships
            if (CanTouch == 0)
            {
                var neighbourTiles = new List<Tile>();
                foreach (var coord in GetAllCoords((start.row, start.col), (end.row, end.col)))
                {
                    if (coord.col + 1 <= MaxCol) neighbourTiles.Add(Tiles[coord.row][coord.col + 1]);
                    if (coord.col - 1 >= 0)      neighbourTiles.Add(Tiles[coord.row][coord.col - 1]);
                    if (coord.row + 1 <= MaxRow) neighbourTiles.Add(Tiles[coord.row + 1][coord.col]);
                    if (coord.row - 1 >= 0)      neighbourTiles.Add(Tiles[coord.row - 1][coord.col]);
                }
                neighbourTiles.ForEach(Console.WriteLine);

                foreach (var tile in neighbourTiles)
                {
                    if (!tile.IsEmpty() && tile.Battleship != battleship)
                    {
                        throw new ArgumentException($"Another ship is touching current ship at ({tile.Row} : {tile.Col})");
                    }
                }
            }

            Battleships.Add(battleship);
            
            foreach (var coord in GetAllCoords(start:(start.row, start.col), end: (end.row, end.col)))
            {
                var tile = Tiles[coord.row][coord.col];
                tile.Battleship = battleship;
                battleship.Locations.Add(tile);
            }

        }        
        
        public List<Tile> GetBattleshipLocations()
        {
            var result = new List<Tile>();
            
            foreach (var battleship in Battleships)
            {
                foreach (var tile in battleship.Locations)
                {
                    result.Add(tile);
                }
            }

            return result;
        }
            
        public Battleship GetData(int row, int col)
        {
            if (row > Tiles.Count - 1 || col > Tiles[0].Count)
            {
                throw new ArgumentException("Out of range ");
            }
            return Tiles[row][col]?.Battleship;
        }
        
        public void UnBomb(Tile tile)
        {
            if (tile.IsBombed && !tile.IsEmpty()) tile.Battleship.LivesLeft++;
            tile.IsBombed = false;
        }
        
        public bool BombLocation(int row, int col)
        {
            if (row > Tiles.Count - 1 || col > Tiles[0].Count)
            {
                throw new ArgumentException("Out of range ");
            }    
            var targetTile = Tiles[row][col];
            
            if (targetTile.IsBombed) throw new ArgumentException("This tile has been already bombed.");

            targetTile.IsBombed = true;
            if (targetTile.IsEmpty())
            {
                return false;
            }
            targetTile.Battleship.LivesLeft--;            
            return true;
        }

        public static List<(int row, int col)> GetAllCoords((int row, int col) start, (int row, int col) end)
        {
            int loopStart, loopEnd;
            bool isVertical = end.col == start.col;
            
            if (isVertical)
            {
                loopStart = end.row > start.row ? start.row : end.row;
                loopEnd = end.row > start.row ? end.row : start.row;
            }
            else
            {
                loopStart = end.col > start.col ? start.col: end.col;
                loopEnd = end.col > start.col ? end.col: start.col;
            }
            var result = new List<(int, int)>();
            for (var i = loopStart; i <= loopEnd; i++)
            {
                result.Add(isVertical ? (row: i, col: start.col) : (row: start.row, col: i));
            }

            return result;

        }

        public bool AnyShipsLeft()
        {
            foreach (var battleship in Battleships)
            {
                if (battleship.IsAlive()) return true;
            }
            return false;
        }

        public void DeleteShipFromBoard(Tile tile)
        {
            if (tile.IsEmpty()) return;
            var ship = tile.Battleship;
            tile.Battleship.Locations.ForEach(tile1 => tile1.Battleship = null);
            tile.Battleship = null;
            Battleships.Remove(ship);
        }

        public object Clone()
        {
            var boardClone = new Board(MaxRow + 1, MaxCol + 1, CanTouch);
            
            Tiles.ForEach(row => row.ForEach(tile =>
            {
                var tileClone = (Tile) tile.Clone();
                tileClone.Board = boardClone;
                //add battleships as well
                
                boardClone.Tiles[tile.Row][tile.Col] = tileClone;
            }));
            
            Battleships.ForEach(battleship =>
            {
                var shipClone = new Battleship(battleship.Size);
                shipClone.LivesLeft = battleship.LivesLeft;
                battleship.Locations.ForEach(tile =>
                {
                    shipClone.Locations.Add(boardClone.Tiles[tile.Row][tile.Col]);
                    boardClone.Tiles[tile.Row][tile.Col].Battleship = shipClone;
                });
                boardClone.Battleships.Add(shipClone);
            });
            return boardClone;
        }
    }
}