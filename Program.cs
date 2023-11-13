using NAudio.Wave;
using ProjectToroidDemo;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Threading;

public interface IMap
{
    void Create(Player player);
    void Map(bool target);
    void TileCreator(IObject[,] map, int numberOfTiles, IObject objectName, int size);
    void Atach(ISpawner spawner);
    void Detach(ISpawner spawner);
    void Notify();
}

public interface IObject
{
    int X(int x);
    int Y(int y);
    void Apearance(bool targeted);
    bool ApplyDmg(int dmg);
    void Colision(IObject name);
}

public interface ISpawner
{
    void Update();
}

public class EnemiesSpawner : ISpawner
{
    private Dungeon dungeon;

    public EnemiesSpawner(Dungeon dungeon)
    {
        this.dungeon = dungeon;
    }

    public void Update()
    {
        Random roll = new();        
        dungeon.UpdateMess1 = true;
        dungeon.Enemies--;
        dungeon.Killcount++;
        if (dungeon.Enemies < 1)
        {
            dungeon.MaxEnemies++; dungeon.Enemies = dungeon.MaxEnemies;
            dungeon.TileCreator(dungeon.DungeonMap, dungeon.MaxEnemies, new Enemy(), dungeon.Size);
            dungeon.TileCreator(dungeon.DungeonMap, roll.Next(1,4), new Potion(), dungeon.Size);
            dungeon.UpdateMess2 = true;
        }
    }
}

public class Dungeon : IMap
{
    private List<ISpawner> spawners = new List<ISpawner>();
    private int size;
    private IObject[,] dungeon;
    private int maxEnemies;
    private int enemies;
    private int targetX = 0;
    private int targetY = 0;

    public int MaxEnemies { get => maxEnemies; set => maxEnemies = value; }
    public int Enemies { get => enemies; set => enemies = value; }
    public IObject[,] DungeonMap { get => dungeon; set => dungeon = value; }
    public int Size { get => size; set => size = value; }
    public int Killcount { get; set; }
    public bool UpdateMess1 { get; set; }
    public bool UpdateMess2 {  get; set; }


    public Dungeon(int size, IObject[,] dungeon, int  maxEnemies, Player player)
    {
        this.size = size;
        this.maxEnemies = maxEnemies;
        this.enemies = maxEnemies;
        this.dungeon = dungeon;
        this.Killcount = 0;
        Create(player);
    }

    public void Atach(ISpawner spawner)
    {
        spawners.Add(spawner);
    }

    public void Detach(ISpawner spawner)
    {
        spawners.Remove(spawner);
    }

    public void Notify()
    {
        foreach (var spawner in spawners)
        {
            spawner.Update();
        }
    }

    public void Create(Player player)
    {
        for (int y = 0; y < size; y++)
        {
            for(int x = 0; x < size; x++)
            {
                dungeon[x, y] = null;
            }
        }
        TileCreator(dungeon, 1, player, size);
        TileCreator(dungeon, maxEnemies, new Enemy(), size);
    }

    public void Map(bool target)
    {
        Console.Write("Kill counter {0}", Killcount);
        if (target)
        {
            Console.WriteLine("\t\t\t     Target:ON");
        }
        else
        {
            Console.WriteLine("\t\t\t     Target:OFF");
        }
        for (int y = 0; y < size; y++)
        {
            Console.Write("X");
            for (int x = 0; x < size; x++)
            {               
                if (dungeon[x, y] == null)
                {
                    if (x == targetX && y == targetY)
                    {
                        Console.Write("{ }");
                    }
                    else Console.Write(": :");
                }
                else
                {
                    if (x == targetX && y == targetY)
                    {
                        dungeon[x, y].Apearance(true);
                    }
                    else dungeon[x, y].Apearance(false);
                }
            }
            Console.WriteLine("X");
        }
        if (UpdateMess1)
        {
            Console.WriteLine("You slain an enemy");
            UpdateMess1 = false;
        }
        if (UpdateMess2)
        {
            Console.WriteLine("{0} new enemies just spawned", MaxEnemies);
            UpdateMess2 = false;
        }
    }
    public void Move()
    {
        bool target = false;
        int playerX = 0;
        int playerY = 0;
        Player player = new Player();
        ConsoleKeyInfo keyInfo;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (dungeon[x, y] is Player)
                {
                    playerY = y;
                    playerX = x;
                    player = (Player)dungeon[x, y];
                    Console.Clear();
                    Map(target);
                    player.HpBar();
                }
            }
        }      
        while (true)
        {
            if (target)
            {
                Console.Clear();
                Map(target);
                player.HpBar();
            }
            do
            {
                keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key == ConsoleKey.W)
                {
                    if (target) 
                    {
                        targetY--; if (targetY < 0) targetY += size;
                    }
                    else
                    {
                        dungeon[playerX, playerY] = null;
                        playerY--; if (playerY < 0) playerY += size;
                    }
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.S)
                {
                    if (target)
                    {
                        targetY++; if (targetY >= size) targetY -= size;
                    }
                    else
                    {
                        dungeon[playerX, playerY] = null;
                        playerY++; if (playerY >= size) playerY -= size;
                    }
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.A)
                {
                    if (target)
                    {
                        targetX--; if (targetX < 0) targetX += size;
                    }
                    else
                    {
                        dungeon[playerX, playerY] = null;
                        playerX--; if (playerX < 0) playerX += size;
                    }
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.D)
                {
                    if (target)
                    {
                        targetX++; if (targetX >= size) targetX -= size;
                    }
                    else
                    {
                        dungeon[playerX, playerY] = null;
                        playerX++; if (playerX >= size) playerX -= size;
                    }
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.Q)
                {
                    if (!target) target = true;
                    else target = false;
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.E)
                {
                    if (dungeon[targetX, targetY] is Enemy)
                    {
                        if (!dungeon[targetX, targetY].ApplyDmg(1)) 
                        {
                            dungeon[targetX, targetY] = null;
                            Notify();                            
                        }
                        target = false;
                        break;
                    }
                }
            } while (keyInfo.Key != ConsoleKey.Escape);
            if (!target) break;
            else Console.Clear();            
        }
        if (dungeon[playerX, playerY] is Enemy)
        {
            dungeon[playerX, playerY].Colision(player);
            Notify();
        }
        else if (dungeon[playerX,playerY] is Potion)
        {
            dungeon[playerX, playerY].Colision(player);
        }
        
        dungeon[playerX, playerY] = player;        
    }
    public void EnemyMove()
    {
        int playerY = 0;
        int playerX = 0;
        Enemy enemy = new Enemy();
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (dungeon[x, y] is Player)
                {
                    playerY = y;
                    playerX = x;
                }
            }
        }
        //movement of enemy
        for (int X = 0; X < size; X++)
        {
            for (int Y = 0; Y < size; Y++)
            {
                if (dungeon[X, Y] is Enemy)
                {
                    enemy = (Enemy)dungeon[X, Y];
                    int hold;
                    if (playerY < Y && enemy.CanMove)
                    {
                        hold = Y - 1;
                        if (dungeon[X, hold] is Enemy) ;
                        else if (dungeon[X, hold] is Player)
                        {
                            dungeon[X, Y].Colision(dungeon[X, hold]);
                            dungeon[X, Y] = null;
                            Notify();
                        }
                        else
                        {
                            dungeon[X, hold] = dungeon[X, Y];
                            dungeon[X, Y] = null;
                        }
                    }
                    else if (playerY > Y && enemy.CanMove)
                    {
                        hold = Y + 1;
                        if (dungeon[X, hold] is Enemy) ;
                        else if (dungeon[X, hold] is Player)
                        {
                            dungeon[X, Y].Colision(dungeon[X, hold]);
                            dungeon[X, Y] = null;
                            Notify();
                        }
                        else
                        {
                            dungeon[X, hold] = dungeon[X, Y];
                            dungeon[X, Y] = null;
                        }
                    }
                    else if (playerX < X && enemy.CanMove)
                    {
                        hold = X - 1;
                        if (dungeon[hold, Y] is Enemy) ;
                        else if (dungeon[hold, Y] is Player)
                        {
                            dungeon[X, Y].Colision(dungeon[hold, Y]);
                            dungeon[X, Y] = null;
                            Notify();
                        }
                        else
                        {
                            dungeon[hold, Y] = dungeon[X, Y];
                            dungeon[X, Y] = null;
                        }
                    }
                    else if (playerX > X && enemy.CanMove)
                    {
                        hold = X + 1;
                        if (dungeon[hold, Y] is Enemy) ;
                        else if (dungeon[hold, Y] is Player)
                        {
                            dungeon[X, Y].Colision(dungeon[hold, Y]);
                            dungeon[X, Y] = null;
                            Notify();
                        }
                        else
                        {
                            dungeon[hold, Y] = dungeon[X, Y];
                            dungeon[X, Y] = null;
                        }
                    }
                    enemy.CanMove = false;
                }
            }
        }
        foreach (var e in dungeon)
        {
            if(e != null)
            {
                if (e is Enemy)
                {
                    enemy = e as Enemy;
                    enemy.CanMove = true;
                }
            }
        }
    }
    public void TileCreator(IObject[,] map, int numberOfTiles, IObject objectName, int size)
    {
        Random roll = new();
        if (numberOfTiles > Freespace(map, size))
        {
            numberOfTiles = Freespace(map, size);
        }
        for (int i = 0; i < numberOfTiles; i++)
        {
            if (objectName is Enemy) objectName = new Enemy();
            int x = roll.Next(0, size);
            int y = roll.Next(0, size);
            if (map[x, y] == null)
            {
                map[x, y] = objectName;
                map[x, y].X(x);
                map[x, y].Y(y);                
            }
            else //gdy wylosuje ponownie zajęte miejsce
            {
                bool loopset = true;
                while (loopset)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        x++;
                        if (x < 0) { x += size; }
                        if (x >= size) { x -= size; }
                        if (map[x, y] == null && loopset)
                        {
                            map[x, y] = objectName;
                            map[x, y].X(x);
                            map[x, y].Y(y);
                            loopset = false;
                        }
                        y++;
                        if (y < 0) { y += size; }
                        if (y >= size) { y -= size; }
                        if (map[x, y] == null && loopset)
                        {
                            map[x, y] = objectName;
                            map[x, y].X(x);
                            map[x, y].Y(y);
                            loopset = false;
                        }
                        x--;
                        if (x < 0) { x += size; }
                        if (x >= size) { x -= size; }
                        if (map[x, y] == null && loopset)
                        {
                            map[x, y] = objectName;
                            map[x, y].X(x);
                            map[x, y].Y(y);
                            loopset = false;
                        }
                    }
                    if (loopset) //gdy pętla będzie za długa
                    {
                        for (int jy = 0; jy < size; jy++)
                        {
                            for (int jx = 0; jx < size; jx++)
                            {
                                if (loopset && map[jx, jy] == null) 
                                { 
                                    map[jx, jy] = objectName;
                                    map[jx, jy].X(jx);
                                    map[jx, jy].Y(jy);
                                    loopset = false; break; 
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    static int Freespace(IObject[,] map, int size)
    {
        int zero = 0;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (map[x, y] == null) { zero++; }
            }
        }
        return zero;
    }
}
public class Potion : IObject
{
    private int hp = 2;
    public int Hp { get => hp; }
    private int x;
    private int y;

    public int X(int x)
    {
        if (x >= 0) this.x = x;
        return this.x;
    }
    public int Y(int y)
    {
        if (y >= 0) this.y = y;
        return this.y;
    }
    public void Apearance(bool targeted)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        if (targeted)
        {
            Console.Write("{H}");
        }
        else
        {
            Console.Write(" H ");
        }
        Console.ForegroundColor = ConsoleColor.White;
    }

    public bool ApplyDmg(int dmg)
    {
        throw new NotImplementedException();
    }

    public void Colision(IObject name)
    {
        name.ApplyDmg(-1);
    }
}

public class Enemy : IObject
{
    private int hp = 2;
    public int Hp { get => hp; }
    private int x;
    private int y;
    private bool canMove = false;
    public bool CanMove { get => canMove; set => canMove = value; }

    public int X(int x) 
    {
        if (x >= 0) this.x = x;
        return this.x; 
    }
    public int Y(int y) 
    { 
        if (y >= 0) this.y = y;
        return this.y; 
    }  
    public void Apearance(bool targeted)
    {
        if (hp > 1) Console.ForegroundColor = ConsoleColor.DarkYellow;
        else Console.ForegroundColor = ConsoleColor.Red;
        if (targeted)
        {
            Console.Write("{E}");
        }
        else 
        {
            Console.Write(" E ");
        }
        Console.ForegroundColor = ConsoleColor.White;
    }

    public bool ApplyDmg(int dmg)
    {
        hp = hp - dmg;
        if (hp > 0) return true;
        else return false;
    }

    public void Colision(IObject name)
    {
        name.ApplyDmg(1);
    }
}

public class Player : IObject
{
    private int maxHp = 7;
    private int hp = 7;
    public int Hp { get => hp; }
    private int x;
    private int y;

    public int X(int x)
    {
        if (x >= 0) this.x = x;
        return this.x;
    }
    public int Y(int y)
    {
        if (y >= 0) this.y = y;
        return this.y;
    }
    public void HpBar()
    {
        int hold = maxHp - hp;
        Console.Write("HP: {0}/{1}", hp, maxHp);
        Console.BackgroundColor = ConsoleColor.Green;
        Console.ForegroundColor = ConsoleColor.Black;
        for (int i = 0; i < hp; i++)
        {
            Console.Write("[ ]");
        }
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Green;
        for (int i = 0; i < hold; i++)
        {
            Console.Write("[ ]");
        }
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
    }

    public void Apearance(bool targeted)
    {
        if (targeted)
        {
            Console.BackgroundColor = ConsoleColor.Cyan;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("{P}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
        }
        else
        {
            Console.BackgroundColor = ConsoleColor.Cyan;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write(" P ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
        }
    }

    public bool ApplyDmg(int dmg)
    {
        hp = hp - dmg;
        if(hp > maxHp) { hp = maxHp; }

        if (hp > 0) return true;
        else return false;
    }

    public void Colision(IObject name)
    {
        name.ApplyDmg(2);
    }
}

class Program
{
    static void Main()
    {
        int size = 15;
        IObject[,] dungeon = new IObject[size,size];
        Player player = new Player();
        Dungeon forest = new Dungeon(size,dungeon,2, player);
        EnemiesSpawner spawner = new EnemiesSpawner(forest);
        forest.Atach(spawner);
        //
        // Get the current directory of the executable
        string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // Specify the folder name within your project
        string folderName = "sound/FunRetroGame.mp3";

        // Replace "path/to/your/soundfile.wav" with the path to your sound file
        string soundFilePath = Path.Combine(currentDirectory, folderName);

        // Create a WaveOutEvent to play the sound
        using (var waveOut = new WaveOutEvent())
        {
            // Create a WaveFileReader with the sound file path
            using (var reader = new Mp3FileReader(soundFilePath))
            using (var pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
            {
                
                // Create a LoopStream to loop the sound continuously
                var loopStream = new LoopStream(pcmStream);
                var volumeProvider = new VolumeWaveProvider16(loopStream);
                volumeProvider.Volume = 0.2f; // Set the volume level here (0.0 to 1.0)

                waveOut.Init(volumeProvider);

                // Start playing the sound in the background
                waveOut.Play();
                Console.WriteLine("WASD - move, Q - target mode, E - atack");
                Console.ReadLine();
                do
                {
                    forest.Move();
                    forest.EnemyMove();
                } while (player.Hp > 0);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("YOU DIED");
                Console.WriteLine("You slained {0}", forest.Killcount);
                Console.ReadKey();


            }
        }
        //s
    }
}