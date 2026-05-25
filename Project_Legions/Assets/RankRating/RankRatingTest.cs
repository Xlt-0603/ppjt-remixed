using System;

namespace PPCorps
{
    class RankRatingTest
    {
        static int GetRankSegment(int rating)
        {
            if (rating <= 1000) return 1;
            if (rating <= 2000) return 2;
            return 3;
        }

        static int CalcWinGain(int myRating, int opponentRating)
        {
            int gain;

            if (myRating < 300)
            {
                gain = 30 + (int)Math.Floor((opponentRating - myRating) / 20f);
                gain = Math.Clamp(gain, 20, 45);
            }
            else if (myRating >= 1000)
            {
                if (opponentRating <= myRating - 50)
                {
                    gain = 10;
                }
                else if (opponentRating > myRating)
                {
                    gain = 20 + (int)Math.Floor((opponentRating - myRating) / 15f);
                    gain = Math.Min(gain, 40);
                }
                else
                {
                    gain = 15 + (int)Math.Floor((opponentRating - myRating + 49) / 25f);
                    gain = Math.Clamp(gain, 10, 20);
                }
            }
            else
            {
                gain = 20 + (int)Math.Floor((opponentRating - myRating) / 25f);
                gain = Math.Clamp(gain, 10, 35);
            }

            return gain;
        }

        static int CalcLossChange(int myRating, int opponentRating)
        {
            int loss;

            if (myRating < 300)
            {
                loss = 10 + (int)Math.Floor((myRating - opponentRating) / 20f);
                loss = Math.Clamp(loss, 5, 15);
            }
            else if (myRating >= 1000)
            {
                if (opponentRating >= myRating + 50)
                {
                    loss = Math.Max(1, 3 - (opponentRating - myRating - 50) / 50);
                }
                else if (opponentRating < myRating)
                {
                    loss = 15 + (int)Math.Floor((myRating - opponentRating) / 15f);
                    loss = Math.Min(loss, 18);
                }
                else
                {
                    loss = 10 + (int)Math.Floor((myRating - opponentRating + 49) / 25f);
                    loss = Math.Clamp(loss, 8, 18);
                }
            }
            else
            {
                loss = 15 + (int)Math.Floor((myRating - opponentRating) / 20f);
                loss = Math.Clamp(loss, 5, 18);
            }

            return -loss;
        }

        static void Run(int my, int opp, bool win)
        {
            int change = win ? CalcWinGain(my, opp) : CalcLossChange(my, opp);
            int seg = GetRankSegment(my);
            int newRating = my + change;
            Console.WriteLine($"  [{seg}段] {my,5}  vs  {opp,5}  {(win ? "WIN" : "LOSE"),4}  →  {change,3}  →  {newRating,5}");
        }

        static void Main()
        {
            Console.WriteLine("  ==================================================");
            Console.WriteLine("   段位  我方分   vs  对手分   结果   变动   新分");
            Console.WriteLine("  ==================================================");

            // 低分段快速上分
            Run(100, 80, true);
            Run(100, 200, true);
            Run(200, 350, true);
            Run(250, 100, false);

            // 300~999 正常区间
            Run(500, 400, true);
            Run(500, 600, true);
            Run(800, 900, true);
            Run(800, 700, false);
            Run(600, 800, false);

            // 1000+ 碾压低分(≥50)只+10
            Run(1200, 1100, true);   // 差100 → 差50以内
            Run(1200, 1050, true);   // 差150 → 差50, 10分
            Run(1200, 1140, true);   // 差60  → 差10, 15~20

            // 1000+ 赢高分
            Run(1200, 1300, true);
            Run(1200, 1400, true);
            Run(1500, 1700, true);

            // 1000+ 输
            Run(1200, 1300, false);  // 强敌高100
            Run(1200, 1100, false);  // 输低分
            Run(1200, 1150, false);
            Run(1500, 1100, false);  // 输低分较多

            // 2000+ 三段
            Run(2100, 2200, true);
            Run(2100, 2000, false);

            Console.WriteLine("  ==================================================");
        }
    }
}
