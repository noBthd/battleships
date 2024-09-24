using System;
using System.Drawing;
using System.Linq;

namespace morskoyboi
{
    public class Field
    {
        public string[,] _charField = new string[12, 12];
        public int[,] _player1Field = new int[10, 10]; // 0 - нет, 1 - карабль, 2 - попал, 3 - убил, 4 - мимо
        public int[] borders = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        public int _hitCount = 0;

        // перевод координат типа B2 в XY
        public int[] strToXY(string strCoord)
        {
            strCoord = strCoord.ToUpper();

            int x = Convert.ToInt32(strCoord[0]) - 64;
            int y;

            if (strCoord.Length != 2)
            {
                y = 10;
            }
            else
            {
                y = Convert.ToInt32(strCoord[1]) - 48;
            }

            int[] XY = { x, y };
            return XY;
        }

        // генерация поля 
        public void fieldGen(string[,] _charField)
        {
            int tmp = 0;
            int charTmp = 64;

            for (int i = 0; i < _charField.GetLength(0) - 1; i++)
            {
                for (int j = 1; j < _charField.GetLength(1); j++)
                {
                    _charField[i, j] = "+";
                }
                _charField[i, 1] = (tmp == 10) ? (Convert.ToString(tmp) + "|") : (Convert.ToString(tmp) + " |");
                tmp++;
            }

            for (int i = 1; i < _charField.GetLength(0) - 1; i++)
            {
                _charField[0, 0] = "    ";
                charTmp++;
                _charField[0, i] = Convert.ToString(Convert.ToChar(charTmp));
            }

            _charField[0, 11] = "";
        }

        // вывод поля
        public void fieldPrint(string[,] _charField)
        {
            for (int i = 0; i < _charField.GetLength(0); i++)
            {
                for (int j = 0; j < _charField.GetLength(1); j++)
                {
                    Console.Write(" {0}", _charField[i, j]);
                }
                Console.WriteLine();
            }
        }

        // стрельба
        public bool shoot(int[] xy, Field field)
        {
            bool shooting = true;
            string strXY;

            while (shooting)
            {
                if (field._player1Field[xy[1] - 1, xy[0] - 1] == 4
                    || field._player1Field[xy[1] - 1, xy[0] - 1] == 2
                    || field._player1Field[xy[1] - 1, xy[0] - 1] == 3)
                {
                    Console.WriteLine("\nВ эту точку уже был произведен выстрел! Выберете новую точку: ");
                    strXY = Console.ReadLine();
                    xy = field.strToXY(strXY);
                }
                else if (field._player1Field[xy[1] - 1, xy[0] - 1] == 1)
                {
                    field._player1Field[xy[1] - 1, xy[0] - 1] = 2;
                    field._charField[xy[1], xy[0] + 1] = "◯";
                    field._hitCount++;
                    shooting = false;
                    return true;
                }
                else
                {
                    field._player1Field[xy[1] - 1, xy[0] - 1] = 4;
                    field._charField[xy[1], xy[0] + 1] = " ";
                    shooting = false;
                }
            }

            return false;
        }

        // ходы
        public void shootMove(int[] xy, Field field, Field bot)
        { 
            string strXY;

            while (shoot(xy, bot))
            {
                if (bot._hitCount >= 20 || field._hitCount >= 20)
                {
                    break;
                }
                field.replaceDeadShip(xy, bot);
                Console.Clear();
                field.fieldPrint(field._charField);
                bot.fieldPrint(bot._charField);

                Console.WriteLine("Вы попали! Введите координату: ");
                strXY = Console.ReadLine();
                xy = field.strToXY(strXY);
            }
        }

        // замена и открыие поля
        public void replaceDeadShip(int[] xy, Field field)
        {
            if (field.isKilled(xy, field))
            {
                xy = field.setEP(xy, field);

                int x = xy[0];
                int y = xy[1];

                int dir = whatDir(x, y, 2, field);
                int size = field.sizeOfShip(xy, field);

                int tmpXbordMin = 0, tmpXbordMax = 0;
                int tmpYbordMin = 0, tmpYbordMax = 0;

                Console.WriteLine("x: {0}\ny: {1}\n", x, y);

                switch (dir)
                {
                    case 1: // вверх
                        tmpYbordMin = y - size;
                        tmpYbordMax = y + 1;
                        tmpXbordMin = x - 1;
                        tmpXbordMax = x + 1;
                        break;
                    case 2: // вправо
                        tmpYbordMin = y - 1;
                        tmpYbordMax = y + 1;
                        tmpXbordMin = x - 1;
                        tmpXbordMax = x + size;
                        break;
                    case 3: // влево
                        tmpYbordMin = y - 1;
                        tmpYbordMax = y + 1;
                        tmpXbordMin = x - size;
                        tmpXbordMax = x + 1;
                        break;
                    case 4: // вниз
                        tmpYbordMin = y - 1;
                        tmpYbordMax = y + size;
                        tmpXbordMin = x - 1;
                        tmpXbordMax = x + 1;
                        break;
                    default:
                        tmpYbordMin = y - 1;
                        tmpYbordMax = y + size;
                        tmpXbordMin = x - 1;
                        tmpXbordMax = x + 1;
                        break;
                }

                for (int i = tmpYbordMin; i <= tmpYbordMax; i++)
                {
                    for (int j = tmpXbordMin; j <= tmpXbordMax; j++)
                    {
                        if (j <= 10 && j >= 1 && i <= 10 && i >= 1)
                        {
                            if (field._player1Field[i - 1, j - 1] == 2)
                            {
                                field._charField[i, j + 1] = "⨷";
                                field._player1Field[i - 1, j - 1] = 3;
                            }
                            else
                            { 
                                field._charField[i, j + 1] = " ";
                                field._player1Field[i - 1, j - 1] = 3;
                            }
                        }
                    }
                }
            }
        }

        // провека убит ли корабль
        public bool isKilled(int[] XY, Field field)
        {
            int dir = 0;
            int x = XY[0];
            int y = XY[1];

            while (field._player1Field[y - 1, x - 1] == 2)
            {
                if (field.isPlaceble(x, y, 1, 1, field))
                {
                    if (field.whatDir(x, y, 2, field) != 0)
                    {
                        if (dir == 0)
                        {
                            dir = field.whatDir(x, y, 2, field);
                            if (borders.Contains(y + 1) && borders.Contains(x + 1)
                                && borders.Contains(y - 1) && borders.Contains(x - 1))
                            {
                                if ((dir == 1 && field._player1Field[y + 1, x - 1] == 1 && field._player1Field[y, x - 1] == 1)
                                    || (dir == 3 && field._player1Field[y - 1, x + 1] == 1 && field._player1Field[y - 1, x] == 1))
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            switch (dir)
                            {
                                case 1:
                                    y--;
                                    break;
                                case 2:
                                    x++;
                                    break;
                                case 3:
                                    x--;
                                    break;
                                case 4:
                                    y++;
                                    break;
                                default:
                                    break;
                            }

                            if (!borders.Contains(y - 1) || !borders.Contains(x - 1))
                            {
                                return true;
                            }
                            if (field._player1Field[y - 1, x - 1] != 2)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    break;
                }
            }

            return false;
        }

        // крайняя точка корабля EP - end point
        public int[] setEP(int[] XY, Field field)
        {
            int x = XY[0];
            int y = XY[1];

            int dir = whatDir(x, y, 2, field);

            if (dir == 0)
            {
                return XY;
            }

            while (borders.Contains(y - 1) && borders.Contains(x - 1)
                && field._player1Field[y - 1, x - 1] == 2)
            {
                switch (dir)
                {
                    case 1:
                        y--;
                        break;
                    case 2:
                        x++;
                        break;
                    case 3:
                        x--;
                        break;
                    case 4:
                        y++;
                        break;
                }
                if (borders.Contains(y) && borders.Contains(x))
                {
                    break;
                }
            }

            switch (dir)
            {
                case 1:
                    y++;
                    break;
                case 2:
                    x--;
                    break;
                case 3:
                    x++;
                    break;
                case 4:
                    y--;
                    break;
            }

            int[] resXY = { x, y };
            return resXY;
        }

        // Напарвление корабля
        private int whatDir(int x, int y, int what, Field field)
        {
            if (field._player1Field[y - 1, x - 1] != what)
            {
                return 1;
            }

            if (borders.Contains(y - 2))
            {
                if (field._player1Field[y - 2, x - 1] == what)
                {
                    return 1;
                }
            }

            if (borders.Contains(y))
            {
                if (field._player1Field[y, x - 1] == what)
                {
                    return 4;
                }
            }

            if (borders.Contains(x - 2))
            {
                if (field._player1Field[y - 1, x - 2] == what)
                {
                    return 3;
                }
            }

            if (borders.Contains(x))
            {
                if (field._player1Field[y - 1, x] == what)
                {
                    return 2;
                }
            }

            return 1;
        }

        // размер корабля
        public int sizeOfShip(int[] xy, Field field)
        {
            int x = xy[0];
            int y = xy[1];

            int size = 0;
            int dir = 0;
            dir = whatDir(x, y, 2, field);

            while (borders.Contains(y - 1) && borders.Contains(x - 1)
                && field._player1Field[y - 1, x - 1] == 2)
            {
                switch (dir)
                {
                    case 1:
                        y--;
                        break;
                    case 2:
                        x++;
                        break;
                    case 3:
                        x--;
                        break;
                    case 4:
                        y++;
                        break;
                    default:
                        break;
                }
                size++; 
            }
            return size;
        }

        // проверка на нахождение в границах массива
        public bool isInBounce(int x, int y, int dir, int size)
        {
            switch (dir)
            {
                case 1: // вверх
                    return x - size + 1 >= 1;
                case 2: // вправо
                    return y + size - 1 <= 10;
                case 3: // влево
                    return y - size + 1 >= 1;
                case 4: // вниз
                    return x + size - 1 <= 10;
                default:
                    return false;
            }
        }

        // проверка на возможность его поставить
        public bool isPlaceble(int x, int y, int size, int dir, Field field)
        {
            int tmpXbordMin = 0, tmpXbordMax = 0;
            int tmpYbordMin = 0, tmpYbordMax = 0;      

            switch (dir)
            {
                case 1: // вверх
                    tmpYbordMin = y - size;
                    tmpYbordMax = y + 1;
                    tmpXbordMin = x - 1;
                    tmpXbordMax = x + 1;
                    break;
                case 2: // вправо
                    tmpYbordMin = y - 1;
                    tmpYbordMax = y + 1;
                    tmpXbordMin = x - 1;
                    tmpXbordMax = x + size;
                    break;
                case 3: // влево
                    tmpYbordMin = y - 1;
                    tmpYbordMax = y + 1;
                    tmpXbordMin = x - size;
                    tmpXbordMax = x + 1;
                    break;
                case 4: // вниз
                    tmpYbordMin = y - 1;
                    tmpYbordMax = y + size;
                    tmpXbordMin = x - 1;
                    tmpXbordMax = x + 1;
                    break;
            }

            for (int i = tmpYbordMin; i <= tmpYbordMax; i++)
            {
                for(int j = tmpXbordMin; j <= tmpXbordMax; j++)
                {
                    if(j <= 10 && j >= 1 && i <= 10 && i >= 1)
                    {
                        if (field._player1Field[i - 1, j - 1] == 1)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }

    public class Ship
    {
        public Boat boat = new Boat();
        public Destroyer destroyer = new Destroyer();
        public Cruiser cruiser = new Cruiser();
        public Battleship battleship = new Battleship();

        public int count = 20;

        public class Boat // однопалубный
        {
            public int count; // max = 4
            public int max = 4;
            public int size = 1;

            // установка лодки
            public void place(string XYCoord, Field field)
            {
                int[] XY = field.strToXY(XYCoord);

                int x = XY[1];
                int y = XY[0];

                field._player1Field[x - 1, y - 1] = 1;
                field._charField[x, y + 1] = "◾"; 
            }
        }

        public class Destroyer // двухпалубный
        {
            public int count = 0; // max = 3
            public int max = 3;
            public int size = 2;

            public void place(string XYCoord, int dir, Field field)
            {
                int direction;
                int[] XY = field.strToXY(XYCoord);
                int x = XY[1];
                int y = XY[0];

                if(dir == 0)
                {
                    Console.WriteLine("Напишите направление корабля (1 - верх, 2 - право, 3 - лево, 4 - низ): ");
                    direction = Convert.ToInt32(Console.ReadLine());
                }
                else
                {
                    direction = dir;
                }

                for (int i = 0; i < size; i++)
                {
                    int newX = x;
                    int newY = y;

                    switch (direction)
                    {
                        case 1: // вверх
                            newX = x - i;
                            break;
                        case 2: // вправо
                            newY = y + i;
                            break;
                        case 3: // влево
                            newY = y - i;
                            break;
                        case 4: // вниз
                            newX = x + i;
                            break;
                    }

                    field._player1Field[newX - 1, newY - 1] = 1;
                    field._charField[newX, newY + 1] = "◾";
                }
            }
        }

        public class Cruiser // трехпалубный
        {
            public int count = 0; // max = 2
            public int max = 2;
            public int size = 3;

            public void place(string XYCoord, int dir, Field field)
            {
                int direction;
                int[] XY = field.strToXY(XYCoord);

                int x = XY[1];
                int y = XY[0];

                if (dir == 0)
                {
                    Console.WriteLine("Напишите направление корабля (1 - верх, 2 - право, 3 - лево, 4 - низ): ");
                    direction = Convert.ToInt32(Console.ReadLine());
                }
                else
                {
                    direction = dir;
                }


                for (int i = 0; i < size; i++)
                {
                    int newX = x;
                    int newY = y;

                    switch (direction)
                    {
                        case 1: // вверх
                            newX = x - i;
                            break;
                        case 2: // вправо
                            newY = y + i;
                            break;
                        case 3: // влево
                            newY = y - i;
                            break;
                        case 4: // вниз
                            newX = x + i;
                            break;
                    }

                    field._player1Field[newX - 1, newY - 1] = 1;
                    field._charField[newX, newY + 1] = "◾";
                }
            }
        }

        public class Battleship // четырехпалубный
        {
            public int count = 0; // max = 1
            public int max = 1;
            public int size = 4;

            public void place(string XYCoord, int dir, Field field)
            {
                int direction;
                int[] XY = field.strToXY(XYCoord);

                int x = XY[1];
                int y = XY[0];

                if (dir == 0)
                {
                    Console.WriteLine("Напишите направление корабля (1 - верх, 2 - право, 3 - лево, 4 - низ): ");
                    direction = Convert.ToInt32(Console.ReadLine());
                }
                else
                {
                    direction = dir;
                }

                for (int i = 0; i < size; i++)
                {
                    int newX = x;
                    int newY = y;

                    switch (direction)
                    {
                        case 1: // вверх
                            newX = x - i;
                            break;
                        case 2: // вправо
                            newY = y + i;
                            break;
                        case 3: // влево
                            newY = y - i;
                            break;
                        case 4: // вниз
                            newX = x + i;
                            break;
                    }

                    field._player1Field[newX - 1, newY - 1] = 1;
                    field._charField[newX, newY + 1] = "◾";

                }
            }
        }

        public void boatPlacement(Field field, Ship ships)
        {
            int direction = 0;
            string coord = "";
            int[] XY;

            //1пб
            while (ships.boat.count != ships.boat.max)
            {
                Console.WriteLine("Однопалубные корабли: \n");
                field.fieldPrint(field._charField);

                Console.WriteLine("Напишите координату куда поставить корабль: ");
                coord = Console.ReadLine();

                XY = field.strToXY(coord);

                int x = XY[0], y = XY[1];

                if (field.isPlaceble(x, y, 1, 1, field) && field.isInBounce(y, x, 1, 1))
                {
                    ships.boat.place(coord, field);
                    ships.boat.count++;

                    Console.Clear();
                }
                else
                {
                    Console.WriteLine("Невозможно поставить корабль (Нажмите Enter и введите координату повторно!)");
                    Console.Read();
                    Console.Clear();

                    continue;
                }
            }
            //2пб
            while (ships.destroyer.count != ships.destroyer.max)
            {
                Console.WriteLine("Двухпалубные корабли: \n");
                field.fieldPrint(field._charField);

                Console.WriteLine("Напишите координату куда поставить корабль: ");
                coord = Console.ReadLine();
                XY = field.strToXY(coord);
                int x = XY[0], y = XY[1];

                Console.WriteLine("Напишите направление корабля (1 - верх, 2 - право, 3 - лево, 4 - низ): ");
                direction = Convert.ToInt32(Console.ReadLine());

                if (field.isPlaceble(x, y, ships.destroyer.size, direction, field)
                    && field.isInBounce(y, x, direction, ships.destroyer.size))
                {
                    ships.destroyer.place(coord, direction, field);
                    ships.destroyer.count++;

                    Console.Clear();
                }
                else
                {
                    Console.WriteLine("Невозможно поставить корабль (Нажмите Enter и введите координату повторно!)");
                    Console.Read();
                    Console.Clear();

                    continue;
                }
            }
            //3пб
            while (ships.cruiser.count != ships.cruiser.max)
            {
                Console.WriteLine("Трехпалубные корабли: \n");
                field.fieldPrint(field._charField);

                Console.WriteLine("Напишите координату куда поставить корабль: ");
                coord = Console.ReadLine();
                XY = field.strToXY(coord);
                int x = XY[0], y = XY[1];

                Console.WriteLine("Напишите направление корабля (1 - верх, 2 - право, 3 - лево, 4 - низ): ");
                direction = Convert.ToInt32(Console.ReadLine());

                if (field.isPlaceble(x, y, ships.cruiser.size, direction, field)
                    && field.isInBounce(y, x, direction, ships.cruiser.size))
                {
                    ships.cruiser.place(coord, direction, field);
                    ships.cruiser.count++;

                    Console.Clear();
                }
                else
                {
                    Console.WriteLine("Невозможно поставить корабль (Нажмите Enter и введите координату повторно!)");
                    Console.Read();
                    Console.Clear();

                    continue;
                }
            }
            //4пб
            while (ships.battleship.count != ships.battleship.max)
            {
                Console.WriteLine("Четырехпалубный корабль: \n"); 
                field.fieldPrint(field._charField);

                Console.WriteLine("Напишите координату куда поставить корабль: ");
                coord = Console.ReadLine();
                XY = field.strToXY(coord);
                int x = XY[0], y = XY[1];

                Console.WriteLine("Напишите направление корабля (1 - верх, 2 - право, 3 - лево, 4 - низ): ");
                direction = Convert.ToInt32(Console.ReadLine());

                if (field.isPlaceble(x, y, ships.battleship.size, direction, field)
                    && field.isInBounce(y, x, direction, ships.battleship.size))
                {
                    ships.battleship.place(coord, direction, field);
                    ships.battleship.count++;

                    Console.Clear();
                }
                else
                {
                    Console.WriteLine("Невозможно поставить корабль (Нажмите Enter и введите координату повторно!)");
                    Console.Read();
                    Console.Clear();

                    continue;
                }
            }
        }
    }

    class Bot
    {
        public string randomShoot(Field field)
        {
            Random rnd = new Random();

            string str;
            int x, y;

            while (true)
            {
                x = rnd.Next(1, 11);
                y = rnd.Next(1, 11);

                if (field._player1Field[y - 1, x - 1] != 2
                    && field._player1Field[y - 1, x - 1] != 4
                    && field._player1Field[y - 1, x - 1] != 3)
                {
                    str = Convert.ToString(Convert.ToChar(x + 64)) + Convert.ToString(y);
                    return str;
                }
            }
        }

        public void preFieldLoad(int type, Field field)
        {
            Ship ships = new Ship();

            switch(type)
            {
                case 1:
                    // Одноc3клеточные корабли
                    ships.boat.place("a1", field);   // Одноклеточный
                    ships.boat.place("c3", field);
                    ships.boat.place("e5", field);
                    ships.boat.place("g7", field);

                    // Двухклеточные корабли
                    ships.destroyer.place("i1", 2, field);  // Вправо, 2 клетки
                    ships.destroyer.place("a10", 2, field); // Влево, 2 клетки
                    ships.destroyer.place("h4", 2, field);  // Вправо, 2 клетки

                    // Трехклеточные корабли
                    ships.cruiser.place("f9", 3, field);    // Влево, 3 клетки
                    ships.cruiser.place("b7", 2, field);    // Вправо, 3 клетки

                    // Четырехклеточный корабль
                    ships.battleship.place("d1", 2, field); // Вправо, 4 клетки
                    break;

                case 2:
                    // Одноклеточные корабли
                    ships.boat.place("j10", field); // Одноклеточный
                    ships.boat.place("a3", field);
                    ships.boat.place("h1", field);
                    ships.boat.place("d5", field);

                    // Двухклеточные корабли
                    ships.destroyer.place("c1", 2, field);  // Вправо, 2 клетки
                    ships.destroyer.place("i6", 3, field);  // Влево, 2 клетки
                    ships.destroyer.place("b9", 3, field);  // Влево, 2 клетки

                    // Трехклеточные корабли
                    ships.cruiser.place("g4", 2, field);    // Вправо, 3 клетки
                    ships.cruiser.place("f8", 3, field);    // Влево, 3 клетки

                    // Четырехклеточный корабль
                    ships.battleship.place("d10", 2, field); // Влево, 4 клетки
                    break;

                case 3:
                    // Одноклеточные корабли
                    ships.boat.place("a1", field);  // Одноклеточный
                    ships.boat.place("j10", field);
                    ships.boat.place("d3", field);
                    ships.boat.place("c7", field);

                    // Двухклеточные корабли
                    ships.destroyer.place("a5", 4, field);  // Вниз, 2 клетки
                    ships.destroyer.place("g2", 2, field);  // Вправо, 2 клетки
                    ships.destroyer.place("i6", 1, field);  // Вверх, 2 клетки

                    // Трехклеточные корабли
                    ships.cruiser.place("d5", 2, field);    // Вправо, 3 клетки
                    ships.cruiser.place("h8", 4, field);    // Вниз, 3 клетки

                    // Четырехклеточный корабль
                    ships.battleship.place("e10", 3, field); // Влево, 4 клетки
                    break;

            }
        }

        public void randomMapChoose(Field field)
        {
            Random rnd = new Random();
            int type = rnd.Next(1, 4);

            preFieldLoad(type, field);
        }

        public void randomPlace(Field field)
        {
            Random rnd = new Random();

            string str;
            int x = 0, y = 0, dir = 0;
            int i = 0;
            bool isPlasedTmp = false;

            Ship ships = new Ship();

            while (i < ships.boat.max && !isPlasedTmp)
            {
                x = rnd.Next(1, 11);
                y = rnd.Next(1, 11);

                if (field.isPlaceble(x, y, ships.boat.size, dir, field)
                    && field.isInBounce(y, x, dir, ships.boat.size))
                {
                    str = Convert.ToString(Convert.ToChar(x + 64)) + Convert.ToString(y);
                    ships.boat.place(str, field);
                    isPlasedTmp = true;
                    i++;
                }
                else
                {
                    isPlasedTmp = false;
                }

            }
            i = 0;

            while (i < ships.destroyer.max && !field.isPlaceble(x, y, ships.destroyer.size, dir, field))
            {
                x = rnd.Next(1, 11);
                y = rnd.Next(1, 11);
                dir = rnd.Next(1, 5);

                str = Convert.ToString(Convert.ToChar(x + 64)) + Convert.ToString(y);
                ships.destroyer.place(str, dir, field);
                i++;
            }
            i = 0;

            while (i < ships.cruiser.max && !field.isPlaceble(x, y, ships.cruiser.size, dir, field))
            {
                x = rnd.Next(1, 11);
                y = rnd.Next(1, 11);
                dir = rnd.Next(1, 5);

                str = Convert.ToString(Convert.ToChar(x + 64)) + Convert.ToString(y);
                ships.cruiser.place(str, dir, field);
                i++;
            }
            i = 0;

            while (i < ships.battleship.max && !field.isPlaceble(x, y, ships.battleship.size, dir, field))
            {
                x = rnd.Next(1, 11);
                y = rnd.Next(1, 11);
                dir = rnd.Next(1, 5);

                str = Convert.ToString(Convert.ToChar(x + 64)) + Convert.ToString(y);
                ships.battleship.place(str, dir, field);
                i++;
            }
        }
    }

    class Program
    {
        static bool checkWin(Field p1, Field p2)
        {
            Ship ships = new Ship();

            if (p2._hitCount >= ships.count) // заменить под конец на ships.count
            {
                Console.Clear();
                Console.WriteLine("Win");
                Console.Read();

                return false;
            }
            else if (p1._hitCount >= ships.count) // заменить под конец на ships.count
            {
                Console.Clear();
                Console.WriteLine("Lose");
                Console.Read();

                return false;
            }
            return true;
        }

        static void Game()
        {
            Console.Clear();

            // initialization
            Field p1 = new Field();
            Field bot = new Field();
            Bot b = new Bot();
            Ship ships = new Ship();

            // player field
            p1.fieldGen(p1._charField);
            ships.boatPlacement(p1, ships);

            // Placement
            //b.randomPlace(bot); //TODO err
            b.randomMapChoose(bot);
            bot.fieldGen(bot._charField);

            // Field out
            Console.Clear();    
            p1.fieldPrint(p1._charField);
            bot.fieldPrint(bot._charField);

            // main prog
            while (checkWin(p1, bot))
            {
                // init inputs
                string playerInput;
                string botInput;

                // random shoot from bot and getting player input
                Console.WriteLine("Player Hits: " + bot._hitCount);
                Console.WriteLine("Bot Hits: " + p1._hitCount);

                botInput = b.randomShoot(p1);
                playerInput = Console.ReadLine();


                // getting inputs and shooting
                p1.shootMove(p1.strToXY(playerInput), p1, bot);

                while (bot.shoot(p1.strToXY(botInput), p1))
                {
                    bot.replaceDeadShip(p1.strToXY(botInput), p1);
                    botInput = b.randomShoot(p1);
                }

                Console.Clear();

                // field output
                p1.fieldPrint(p1._charField);
                bot.fieldPrint(bot._charField);
            }
        }

        static void Main(string[] args)
        {
            Game();

            //Field field = new Field();
            //Ship ships = new Ship();

            //int[] XY1 = { 1, 1 };
            //int[] XY2 = { 1, 10 };
            //int[] XY3 = { 10, 1 };
            //int[] XY4 = { 10, 10 };

            //field.fieldGen(field._charField);
            //ships.boat.place("a1", field);
            //ships.boat.place("j1", field);
            //ships.boat.place("a", field);
            //ships.boat.place("j", field);

            //field.shoot(XY1, field);
            //field.shoot(XY2, field);
            //field.shoot(XY3, field);
            //field.shoot(XY4, field);

            ////field.isKilled(field.strToXY("b6"), field);
            //field.replaceDeadShip(field.strToXY("a"), field);
            //field.fieldPrint(field._charField);
            //Console.WriteLine(field._hitCount);

            //for (int i = 0; i < 10; i++)
            //{
            //    for (int j = 0; j < 10; j++)
            //    {
            //        Console.Write(field._player1Field[i, j]);
            //    }
            //    Console.WriteLine();
            //}
        }
    }
}

//TODO логику после убития корабля
//TODO проблема в определении направления одинарного корабля